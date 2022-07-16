using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityMethodDeclarationAnalyzer : DiagnosticAnalyzer
    {
        private const string Title = "实体类禁止声明方法";

        private const string MessageFormat = "实体类: {0} 不能在类内部声明方法: {1}";

        private const string Description = "实体类禁止声明方法.";
        
        private const string EntityType = "ET.Entity";

        private const string EnableMethodAttribute = "ET.EnableMethodAttribute";
        
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
            context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        }

        private void Analyzer(SymbolAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllModel))
            {
                return;
            }
            
            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

            // 筛选出实体类
            if (namedTypeSymbol.BaseType?.ToString() != EntityType)
            {
                return;
            }

            // 忽略含有EnableMethod标签的实体类
            if (namedTypeSymbol.HasAttribute(EnableMethodAttribute))
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