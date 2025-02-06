using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class DisableNormalClassDeclaratonInModelAssemblyAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityClassDeclarationAnalyzerrRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModel))
                {
                    analysisContext.RegisterSyntaxNodeAction(Analyze,SyntaxKind.ClassDeclaration);
                }
            });
            
        }
        
        private void Analyze(SyntaxNodeAnalysisContext context)
        {
            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }

            var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);

            if (namedTypeSymbol==null)
            {
                return;
            }

            if (namedTypeSymbol.IsStatic)
            {
                return;
            }

            if (namedTypeSymbol.HasAttributeInTypeAndBaseTyes(Definition.EnableClassAttribute))
            {
                return;
            }
            
            Diagnostic diagnostic = Diagnostic.Create(EntityClassDeclarationAnalyzerrRule.Rule, classDeclarationSyntax.Identifier.GetLocation(), namedTypeSymbol);
            context.ReportDiagnostic(diagnostic);
        }
    }
}

