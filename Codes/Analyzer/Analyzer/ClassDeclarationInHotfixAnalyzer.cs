using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassDeclarationInHotfixAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "Hotfix程序集中 只能声明含有BaseAttribute子类特性的类或静态类";

        private const string MessageFormat = "Hotfix程序集中 只能声明含有BaseAttribute子类特性的类或静态类 类: {0}";

        private const string Description = "Hotfix程序集中 只能声明含有BaseAttribute子类特性的类或静态类.";

        private const string BaseAttribute = "ET.BaseAttribute";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.ClassDeclarationInHotfixAnalyzerRuleId,
            Title,
            MessageFormat,
            DiagnosticCategories.Hotfix,
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
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

            if (namedTypeSymbol.IsStatic)
            {
                return;
            }

            if (!this.CheckIsTypeOrBaseTypeHasBaseAttributeInherit(namedTypeSymbol))
            {
                foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, declaringSyntaxReference.GetSyntax()?.GetLocation(), namedTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        /// <summary>
        ///     检查该类或其基类是否有BaseAttribute的子类特性标记
        /// </summary>
        private bool CheckIsTypeOrBaseTypeHasBaseAttributeInherit(INamedTypeSymbol namedTypeSymbol)
        {
            INamedTypeSymbol? typeSymbol = namedTypeSymbol;
            while (typeSymbol != null)
            {
                if (typeSymbol.HasBaseAttribute(BaseAttribute))
                {
                    return true;
                }

                typeSymbol = typeSymbol.BaseType;
            }

            return false;
        }
    }
}