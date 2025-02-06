using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityMethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "实体类禁止声明方法";

        private const string MessageFormat = "实体类: {0} 不能在类内部声明方法: {1}";

        private const string Description = "实体类禁止声明方法.";

        
        
        private static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.EntityMethodDeclarationAnalyzerRuleId,
                    Title,
                    MessageFormat,
                    DiagnosticCategories.Hotfix,
                    DiagnosticSeverity.Error,
                    true,
                    Description);

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
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName,AnalyzeAssembly.AllModel))
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
            // 筛选出实体类
            if (namedTypeSymbol.BaseType?.ToString() != Definition.EntityType && namedTypeSymbol.BaseType?.ToString() != Definition.LSEntityType)
            {
                return;
            }

            // 忽略含有EnableMethod标签的实体类
            if (AnalyzerHelper.HasAttribute((ITypeSymbol)namedTypeSymbol, Definition.EnableMethodAttribute))
            {
                return;
            }

            foreach (var syntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
            {
                var classSyntax = syntaxReference.GetSyntax();
                if (!(classSyntax is ClassDeclarationSyntax classDeclarationSyntax))
                {
                    return;
                }

                foreach (var memberDeclarationSyntax in classDeclarationSyntax.Members)
                {
                    // 筛选出类声明语法节点下的所有方法声明语法节点
                    if (memberDeclarationSyntax is MethodDeclarationSyntax methodDeclarationSyntax)
                    {
                        Diagnostic diagnostic = Diagnostic.Create(Rule, methodDeclarationSyntax.GetLocation(),namedTypeSymbol.Name,methodDeclarationSyntax.Identifier.Text);
                        context.ReportDiagnostic(diagnostic);
                    }
                }
            }
            
        }
    }
}