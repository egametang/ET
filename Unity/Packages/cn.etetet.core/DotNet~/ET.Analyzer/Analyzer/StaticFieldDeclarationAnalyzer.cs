using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class StaticFieldDeclarationAnalyzer : DiagnosticAnalyzer
    {
        
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(StaticFieldDeclarationAnalyzerRule.Rule);
        
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
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.All))
            {
                return;
            }

            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

            foreach (ISymbol? memberSymbol in namedTypeSymbol.GetMembers())
            {
                if (memberSymbol is IFieldSymbol { IsConst: false,IsStatic:true } or IPropertySymbol { IsStatic: true })
                {
                    bool hasAttr = memberSymbol.GetAttributes().Any(x => x.AttributeClass?.ToString() == Definition.StaticFieldAttribute);
                    if (!hasAttr)
                    {
                        ReportDiagnostic(memberSymbol);
                    }
                }
            }

            void ReportDiagnostic(ISymbol symbol)
            {
                foreach (SyntaxReference? declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    Diagnostic diagnostic = Diagnostic.Create(StaticFieldDeclarationAnalyzerRule.Rule, declaringSyntaxReference.GetSyntax()?.GetLocation(), symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
        
    }
}

