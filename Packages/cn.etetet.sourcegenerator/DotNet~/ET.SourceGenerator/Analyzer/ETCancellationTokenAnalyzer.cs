using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.FlowAnalysis;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ETCancellationTokenAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            CheckETCancellTokenAfterAwaitAnalyzerRule.Rule,
            AwaitExpressionCancelTokenParamAnalyzerRule.Rule, AsyncMethodWithCancelTokenParamAnalyzerRule.Rule,
            ExpressionWithCancelTokenParamAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            //context.RegisterSyntaxNodeAction(this.AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
        }

        private void AnalyzeMethodDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (context.Node is not MethodDeclarationSyntax methodDeclarationSyntax)
            {
                return;
            }

            // 检查含有ETCancelToken参数的函数调用 是否传入null 或未赋值
            foreach (InvocationExpressionSyntax? invocationExpressionSyntax in methodDeclarationSyntax.DescendantNodes<InvocationExpressionSyntax>())
            {
                if (this.CancelTokenArguIsNullOrNotSet(invocationExpressionSyntax, context))
                {
                    Diagnostic diagnostic = Diagnostic.Create(ExpressionWithCancelTokenParamAnalyzerRule.Rule,
                        invocationExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }

            // 忽略非异步函数
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            if (methodSymbol is not { IsAsync: true })
            {
                return;
            }

            string methodReturnType = $"{methodSymbol.ReturnType.ContainingNamespace}.{methodSymbol.ReturnType.Name}";
            if (methodReturnType != Definition.ETTaskFullName)
            {
                return;
            }

            bool isGenericReturnTYpe = methodDeclarationSyntax.ReturnType.IsKind(SyntaxKind.GenericName);

            // 检测是否含有cancelToken参数
            if (!methodSymbol.HasParameterType(Definition.ETCancellationToken, out IParameterSymbol? cancelTokenSymbol) || cancelTokenSymbol == null)
            {
                return;
            }

            // 函数定义处 ETcanceltoken参数 是否有默认值
            if (cancelTokenSymbol.HasExplicitDefaultValue)
            {
                SyntaxNode? cancelTokenParamSyntax = cancelTokenSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                if (cancelTokenParamSyntax == null)
                {
                    cancelTokenParamSyntax = methodDeclarationSyntax;
                }

                Diagnostic diagnostic = Diagnostic.Create(AsyncMethodWithCancelTokenParamAnalyzerRule.Rule, cancelTokenParamSyntax.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }

            foreach (AwaitExpressionSyntax? awaitExpressionSyntax in methodDeclarationSyntax.DescendantNodes<AwaitExpressionSyntax>())
            {
                // 跳过await 返回值不是ETTask的
                TypeInfo awaitExpressionTypeSymbol = context.SemanticModel.GetTypeInfo(awaitExpressionSyntax.Expression);
                if (awaitExpressionTypeSymbol.Type == null)
                {
                    continue;
                }

                string awaitExpressionType = $"{awaitExpressionTypeSymbol.Type.ContainingNamespace}.{awaitExpressionTypeSymbol.Type.Name}";
                if (awaitExpressionType != Definition.ETTaskFullName)
                {
                    continue;
                }

                // 跳过await字段的表达式
                InvocationExpressionSyntax? awaitInvocationSyntax = awaitExpressionSyntax.Expression as InvocationExpressionSyntax;
                if (awaitInvocationSyntax == null)
                {
                    continue;
                }

                // await 函数是否带canceltoken参数
                if (!HasCancelTokenInAwait(awaitInvocationSyntax, cancelTokenSymbol, context))
                {
                    Diagnostic diagnostic = Diagnostic.Create(AwaitExpressionCancelTokenParamAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }

                StatementSyntax? statementSyntax = awaitExpressionSyntax.GetNeareastAncestor<StatementSyntax>();
                if (statementSyntax == null)
                {
                    continue;
                }

                BasicBlock? block = AnalyzerHelper.GetAwaitStatementControlFlowBlock(statementSyntax, awaitExpressionSyntax, context.SemanticModel);

                if (block == null)
                {
                    if (statementSyntax.IsKind(SyntaxKind.LocalDeclarationStatement) &&
                        !HasCheckCancelTokenAfterAwaitForLocalDeclaration(statementSyntax, cancelTokenSymbol, context))
                    {
                        Diagnostic diagnostic =
                                Diagnostic.Create(CheckETCancellTokenAfterAwaitAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                        context.ReportDiagnostic(diagnostic);
                        //throw new Exception($"block == null {statementSyntax.IsKind(SyntaxKind.LocalDeclarationStatement)} file {awaitExpressionSyntax.SyntaxTree.FilePath}");
                    }

                    continue;
                }

                bool isMethodExitPoint = IsMethodExitPoint(block, statementSyntax, out int statementIndex);

                if (isMethodExitPoint)
                {
                    if (!isGenericReturnTYpe)
                    {
                        continue;
                    }

                    Diagnostic diagnostic = Diagnostic.Create(CheckETCancellTokenAfterAwaitAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }

                // await函数调用后 是否判断了canceltoken
                if (!this.HasCheckCancelTokenAfterAwaitForExpression(statementSyntax, cancelTokenSymbol, block, statementIndex, context))
                {
                    Diagnostic diagnostic = Diagnostic.Create(CheckETCancellTokenAfterAwaitAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        private bool IsMethodExitPoint(BasicBlock block, StatementSyntax statementSyntax, out int statementIndex)
        {
            IOperation statementOperation = block.Operations.First(x => x.Syntax.Contains(statementSyntax));
            // 如果表达式在block最后一个，是出口函数则跳过 否则报错
            statementIndex = block.Operations.IndexOf(statementOperation);
            if (statementIndex == block.Operations.Length - 1)
            {
                return block.FallThroughSuccessor?.Destination?.Kind == BasicBlockKind.Exit;
            }

            return false;
        }

        /// <summary>
        /// 调用await函数后，是否判断了所在函数传入的canceltoken参数是否取消 
        /// </summary>
        private bool HasCheckCancelTokenAfterAwaitForExpression(StatementSyntax statementSyntax, IParameterSymbol cancelTokenSymbol, BasicBlock block,
        int statementIndex,
        SyntaxNodeAnalysisContext context)
        {
            // 判断表达式是否为block最后一个
            if (block.Operations.Length - 1 == statementIndex)
            {
                return false;
            }

            // 检查await表达式的下一个表达式是否为判断 cancelToken.IsCancel()
            StatementSyntax? nextStatement =
                    block.Operations[statementIndex + 1].Syntax.DescendantNodesAndSelf().OfType<StatementSyntax>().FirstOrDefault();
            if (nextStatement == null)
            {
                return false;
            }

            return IsCheckCancelTokenStatement(nextStatement, cancelTokenSymbol);
        }

        /// <summary>
        /// 调用await函数后，是否判断了所在函数传入的canceltoken参数是否取消
        /// LocalDeclaration 表达式 无法使用控制流图
        /// </summary>
        private bool HasCheckCancelTokenAfterAwaitForLocalDeclaration(StatementSyntax statementSyntax, IParameterSymbol cancelTokenSymbol,
        SyntaxNodeAnalysisContext context)
        {
            // 检查await表达式的下一个表达式是否为判断 cancelToken.IsCancel()
            StatementSyntax? nextStatement = statementSyntax.NextNode() as StatementSyntax;
            if (nextStatement == null)
            {
                return false;
            }

            return IsCheckCancelTokenStatement(nextStatement, cancelTokenSymbol);
        }

        /// <summary>
        /// 判断下个表达式是否为检查canceltoken
        /// </summary>
        private bool IsCheckCancelTokenStatement(StatementSyntax nextStatement, IParameterSymbol cancelTokenSymbol)
        {
            if (nextStatement is not IfStatementSyntax ifStatementSyntax)
            {
                return false;
            }

            string conditionStr = ifStatementSyntax.Condition.ToString().Replace(" ", string.Empty);
            if (conditionStr != $"{cancelTokenSymbol.Name}.IsCancel()")
            {
                return false;
            }

            // 检查判断表达式内是否直接return
            if (ifStatementSyntax.Statement.GetFirstChild<ReturnStatementSyntax>() == null)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 调用await函数时, 是否带了所在函数传入的canceltoken参数
        /// </summary>
        private bool HasCancelTokenInAwait(InvocationExpressionSyntax awaitInvocationSyntax, IParameterSymbol cancelTokenSymbol,
        SyntaxNodeAnalysisContext context)
        {
            return awaitInvocationSyntax.ArgumentList.Arguments.Any(x => x.ToString() == cancelTokenSymbol.Name);
        }

        /// <summary>
        /// 表达式canceltoken参数是否为null或未赋值
        /// </summary>
        private bool CancelTokenArguIsNullOrNotSet(InvocationExpressionSyntax invocationExpressionSyntax, SyntaxNodeAnalysisContext context)
        {
            IMethodSymbol? methodSymbol = context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol as IMethodSymbol;
            if (methodSymbol == null)
            {
                return false;
            }

            // 忽略没有ETCancellationToken参数的表达式
            if (!methodSymbol.HasParameterType(Definition.ETCancellationToken, out IParameterSymbol? cancelTokenSYmbol))
            {
                return false;
            }

            var arguments = invocationExpressionSyntax.ArgumentList.Arguments;
            for (int i = 0; i < arguments.Count; i++)
            {
                ITypeSymbol? typeInfo = context.SemanticModel.GetTypeInfo(arguments[i].Expression).Type;
                if (typeInfo == null)
                {
                    continue;
                }

                if (typeInfo.ToString() == Definition.ETCancellationToken)
                {
                    return false;
                }
            }

            return true;
        }
    }
}