using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityClassDeclarationAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "实体类限制多层继承";

        private const string MessageFormat = "类: {0} 不能继承Entiy的子类 请直接继承Entity";

        private const string Description = "实体类限制多层继承.";

        private const string EntityType = "ET.Entity";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.EntityClassDeclarationAnalyzerRuleId,
            Title,
            MessageFormat,
            DiagnosticCategories.All,
            DiagnosticSeverity.Error, true, Description);

        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(Rule);

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

            if (namedTypeSymbol.BaseType?.BaseType?.ToString() != EntityType)
            {
                return;
            }

            foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                SyntaxNode classSyntax = declaringSyntaxReference.GetSyntax();
                Diagnostic diagnostic = Diagnostic.Create(Rule, classSyntax.GetLocation(), namedTypeSymbol.Name, context.Compilation.AssemblyName);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}