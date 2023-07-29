using System;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AsyncMethodReturnTypeAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(AsyncMethodReturnTypeAnalyzerRule.Rule);
        
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(this.Analyzer, SyntaxKind.MethodDeclaration,SyntaxKind.LocalFunctionStatement);
        }

        private void Analyzer(SyntaxNodeAnalysisContext context)
        {
            IMethodSymbol? methodSymbol = null;
            
            if (context.Node is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);
            }
            else if (context.Node is LocalFunctionStatementSyntax localFunctionStatementSyntax)
            {
                methodSymbol = context.SemanticModel.GetDeclaredSymbol(localFunctionStatementSyntax) as IMethodSymbol;

            }
            if (methodSymbol==null)
            {
                return;
            }
            
            if (methodSymbol.IsAsync && methodSymbol.ReturnsVoid)
            {
                Diagnostic diagnostic = Diagnostic.Create(AsyncMethodReturnTypeAnalyzerRule.Rule, context.Node.GetLocation());
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

