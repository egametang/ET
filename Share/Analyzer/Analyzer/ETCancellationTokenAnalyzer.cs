using System;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ETCancellationTokenAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(ETCancellationTokenAnalyzerRule.Rule);

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
            // 只检查异步函数
            IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            if (methodSymbol is not { IsAsync: true })
            {
                return;
            }

            // 只检查入参含有ETCancellationToken的函数
            if (!methodSymbol.HasParameterType(Definition.ETCancellationToken, out IParameterSymbol? cancelTokenSymbol) || cancelTokenSymbol == null)
            {
                return;
            }

            
            foreach (AwaitExpressionSyntax? awaitExpressionSyntax in methodDeclarationSyntax.DescendantNodes<AwaitExpressionSyntax>())
            {
                if (!Check(awaitExpressionSyntax))
                {
                    Diagnostic diagnostic = Diagnostic.Create(ETCancellationTokenAnalyzerRule.Rule, awaitExpressionSyntax.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }

                bool Check(AwaitExpressionSyntax awaitExpression)
                {
                    // 跳过ETTaskCompleteTask
                    if (awaitExpression.Expression.ToString() == Definition.ETTaskCompleteTask)
                    {
                        return true;
                    }

                    // 检查await表达式的上一个表达式是否为判断 cancelToken.IsCancel()
                    StatementSyntax? statementSyntax = awaitExpression.GetNeareastAncestor<StatementSyntax>();

                    if (statementSyntax == null)
                    {
                        return true;
                    }

                    SyntaxNode? previousNode = statementSyntax.PreviousNode();
                    if (previousNode is not IfStatementSyntax ifStatementSyntax)
                    {
                        return false;
                    }

                    string conditionStr = ifStatementSyntax.Condition.ToString().Replace(" ", string.Empty);
                    if (conditionStr!= $"{cancelTokenSymbol.Name}.IsCancel()")
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
            }
        }
    }
}