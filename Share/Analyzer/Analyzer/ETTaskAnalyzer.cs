using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ETTaskAnalyzer:DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => 
                ImmutableArray.Create(ETTaskInSyncMethodAnalyzerRule.Rule,ETTaskInAsyncMethodAnalyzerRule.Rule);
        
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction((analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModelHotfix))
                {
                    analysisContext.RegisterSemanticModelAction((this.AnalyzeSemanticModel));
                }
            } ));
        }
        
        private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext)
        {
            foreach (var memberAccessExpressionSyntax in analysisContext.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<MemberAccessExpressionSyntax>())
            {
                AnalyzeMemberAccessExpression(analysisContext, memberAccessExpressionSyntax);
            }
        }

        private void AnalyzeMemberAccessExpression(SemanticModelAnalysisContext context, MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            // 获取方法调用Syntax 对应的methodSymbol
            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol methodSymbol))
            {
                return;
            }

            //忽略void返回值函数
            if (methodSymbol.ReturnsVoid)
            {
                return;
            }
            
            if (!(methodSymbol.ReturnType is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }
            
            // 筛选出返回值为ETTask 和ETTask<T>的函数
            if (namedTypeSymbol.Name!=Definition.ETTask)
            {
                return;
            }
            
            // 获取ETTask函数调用处所在的函数体
            var containingMethodDeclarationSyntax = memberAccessExpressionSyntax?.GetNeareastAncestor<MethodDeclarationSyntax>();
            if (containingMethodDeclarationSyntax==null)
            {
                return;
            }
            
            IMethodSymbol? containingMethodSymbol = context.SemanticModel.GetDeclaredSymbol(containingMethodDeclarationSyntax);
            if (containingMethodSymbol==null)
            {
                return;
            }
            
            // ETTask函数在 ()=>Function(); 形式的lanmda表达式中时 
            if (invocationExpressionSyntax.Parent is ParenthesizedLambdaExpressionSyntax)
            {
                Diagnostic diagnostic = Diagnostic.Create(ETTaskInSyncMethodAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                    memberAccessExpressionSyntax?.Name);
                context.ReportDiagnostic(diagnostic);
                return;
            }
            
            
            // 方法体内ETTask单独调用时
            if (invocationExpressionSyntax.Parent is ExpressionStatementSyntax)
            {
                if (containingMethodSymbol.IsAsync)
                {
                    Diagnostic diagnostic = Diagnostic.Create(ETTaskInAsyncMethodAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        memberAccessExpressionSyntax?.Name);
                    context.ReportDiagnostic(diagnostic);
                }
                else
                {
                    Diagnostic diagnostic = Diagnostic.Create(ETTaskInSyncMethodAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        memberAccessExpressionSyntax?.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}