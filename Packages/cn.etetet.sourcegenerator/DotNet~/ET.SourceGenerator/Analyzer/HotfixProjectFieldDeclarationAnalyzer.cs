using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class HotfixProjectFieldDeclarationAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "实体字段访问错误";

        private const string MessageFormat = "Hotfix程序集中 不允许声明非Const字段  字段: {0}";

        private const string Description = "请使用实体类属性或方法访问其他实体字段.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.HotfixProjectFieldDeclarationAnalyzerRuleId,
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
            foreach (ISymbol? memberSymbol in namedTypeSymbol.GetMembers())
            {
                // 筛选出属性成员
                if (memberSymbol is IPropertySymbol propertySymbol)
                {
                    ReportDiagnostic(propertySymbol);
                    continue;
                }

                // 筛选出非Const字段成员
                if (memberSymbol is IFieldSymbol fieldSymbol && !fieldSymbol.IsConst)
                {
                    ReportDiagnostic(fieldSymbol);
                }
            }

            void ReportDiagnostic(ISymbol symbol)
            {
                foreach (SyntaxReference? declaringSyntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, declaringSyntaxReference.GetSyntax()?.GetLocation(), symbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        
    }
}