using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DiableNewAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(DisableNewAnalyzerRule.Rule);
        
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            
            context.RegisterSyntaxNodeAction(this.AnalyzeObjectCreationExpression, SyntaxKind.ObjectCreationExpression);
        }

        private void AnalyzeObjectCreationExpression(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ObjectCreationExpressionSyntax objectCreationExpressionSyntax)
            {
                return;
            }
            
            var typeSymbol = context.SemanticModel.GetSymbolInfo(objectCreationExpressionSyntax.Type).Symbol as ITypeSymbol;
            if (typeSymbol==null)
            {
                return;
            }
            if (typeSymbol.HasAttributeInTypeAndBaseTyes(Definition.DisableNewAttribute))
            {
                Diagnostic diagnostic = Diagnostic.Create(DisableNewAnalyzerRule.Rule, objectCreationExpressionSyntax?.GetLocation(),typeSymbol);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

