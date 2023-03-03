using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ETCancellationTokenAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(
            CheckETCancellTokenAfterAwaitAnalyzerRule.Rule,
            AwaitExpressionCancelTokenParamAnalyzerRule.Rule, AsyncMethodWithCancelTokenParamAnalyzerRule.Rule,
            ExpressionWithCancelTokenParamAnalyzerRule.Rule, ETTaslAsyncMethodHasCancelTokenAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(this.AnalyzeMethodDeclaration, SyntaxKind.MethodDeclaration);
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

            // 检测是否含有cancelToken参数
            if (!methodSymbol.HasParameterType(Definition.ETCancellationToken, out IParameterSymbol? cancelTokenSymbol) || cancelTokenSymbol == null)
            {
                Diagnostic diagnostic = Diagnostic.Create(ETTaslAsyncMethodHasCancelTokenAnalyzerRule.Rule,
                    methodDeclarationSyntax.ParameterList.GetLocation());
                context.ReportDiagnostic(diagnostic);
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

                // await函数调用后 是否判断了canceltoken
                if (!this.HasCheckCancelTokenAfterAwait(awaitExpressionSyntax, cancelTokenSymbol, context))
                {
                    Diagnostic diagnostic = Diagnostic.Create(CheckETCancellTokenAfterAwaitAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
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
            }
        }

        /// <summary>
        /// 调用await函数后，是否判断了所在函数传入的canceltoken参数是否取消 
        /// </summary>
        private bool HasCheckCancelTokenAfterAwait(AwaitExpressionSyntax awaitExpression, IParameterSymbol cancelTokenSymbol,
        SyntaxNodeAnalysisContext context)
        {
            // 检查await表达式的下一个表达式是否为判断 cancelToken.IsCancel()
            StatementSyntax? statementSyntax = awaitExpression.GetNeareastAncestor<StatementSyntax>();

            if (statementSyntax == null)
            {
                return true;
            }

            SyntaxNode? nextNode = statementSyntax.NextNode();
            if (nextNode is not IfStatementSyntax ifStatementSyntax)
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