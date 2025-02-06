using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class ClassDeclarationInHotfixAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "Hotfix程序集中 只能声明含有EnableClassAttribute子类特性的类或静态类";

        private const string MessageFormat = "Hotfix程序集中 只能声明含有EnableClassAttribute子类特性的类或静态类 类: {0}";

        private const string Description = "Hotfix程序集中 只能声明含有EnableClassAttribute子类特性的类或静态类.";

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
            context.RegisterCompilationStartAction((analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName,AnalyzeAssembly.AllHotfix))
                {
                    analysisContext.RegisterSemanticModelAction((this.AnalyzeSemanticModel));
                }
            } ));
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext)
        {
            foreach (var classDeclarationSyntax in analysisContext.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<ClassDeclarationSyntax>())
            {
                var classTypeSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (classTypeSymbol!=null)
                {
                    Analyzer(analysisContext, classTypeSymbol);
                }
            }
        }
        
        private void Analyzer(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsStatic)
            {
                return;
            }

            if (!this.CheckIsTypeOrBaseTypeHasBaseAttributeInherit(namedTypeSymbol))
            {
                foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, declaringSyntaxReference.GetSyntax()?.GetLocation(), namedTypeSymbol.Name);
                    //Diagnostic diagnostic = Diagnostic.Create(Rule, declaringSyntaxReference.GetSyntax()?.GetLocation(), context.SemanticModel.SyntaxTree.FilePath);
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
                if (typeSymbol.HasAttribute(Definition.EnableClassAttribute))
                {
                    return true;
                }

                typeSymbol = typeSymbol.BaseType;
            }

            return false;
        }
    }
}