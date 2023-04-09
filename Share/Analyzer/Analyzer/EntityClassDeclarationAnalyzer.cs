using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityClassDeclarationAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityClassDeclarationAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        }

        private void Analyzer(SymbolAnalysisContext context)
        {
            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

            if (namedTypeSymbol.BaseType?.BaseType?.ToString() != Definition.EntityType)
            {
                return;
            }

            foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                SyntaxNode classSyntax = declaringSyntaxReference.GetSyntax();
                Diagnostic diagnostic = Diagnostic.Create(EntityClassDeclarationAnalyzerRule.Rule, classSyntax.GetLocation(), namedTypeSymbol.Name, context.Compilation.AssemblyName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}