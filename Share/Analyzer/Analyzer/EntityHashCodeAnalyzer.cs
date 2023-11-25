using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityHashCodeAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityHashCodeAnalyzerRule.Rule);
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction((this.CompilationStartAnalysis));
        }

        private void CompilationStartAnalysis(CompilationStartAnalysisContext context)
        {
            var entityHashCodeMap = new ConcurrentDictionary<long, string>();
            context.RegisterSemanticModelAction((analysisContext =>
            {
                if (AnalyzerHelper.IsSemanticModelNeedAnalyze(analysisContext.SemanticModel,UnityCodesPath.UnityModel))
                {
                    AnalyzeSemanticModel(analysisContext, entityHashCodeMap);
                }
            } ));
        }

        private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext, ConcurrentDictionary<long, string> entityHashCodeMap)
        {
            foreach (var classDeclarationSyntax in analysisContext.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<ClassDeclarationSyntax>())
            {
                var classTypeSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (classTypeSymbol!=null)
                {
                    AnalyzeTypeSymbol(analysisContext, classTypeSymbol,entityHashCodeMap);
                }
            }
        }

        private void AnalyzeTypeSymbol(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol,ConcurrentDictionary<long, string> entityHashCodeMap)
        {
            var baseType = namedTypeSymbol.BaseType?.ToString();
            
            // 筛选出实体类
            if (baseType!= Definition.EntityType && baseType != Definition.LSEntityType)
            {
                return;
            }

            var entityName = namedTypeSymbol.ToString();
            var hashCode = entityName.GetLongHashCode();

            if (entityHashCodeMap.TryGetValue(hashCode, out var existEntityName))
            {
                if (existEntityName == entityName)
                {
                    return;
                }
                var classDeclarationSyntax = namedTypeSymbol.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;
                Diagnostic diagnostic = Diagnostic.Create(EntityHashCodeAnalyzerRule.Rule, classDeclarationSyntax?.Identifier.GetLocation(), entityName,existEntityName,hashCode.ToString());
                context.ReportDiagnostic(diagnostic);
            }
            else
            {
                entityHashCodeMap[hashCode] = entityName;
            }
        }
    }
}

