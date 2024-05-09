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
            var entityHashCodeMap = new ConcurrentDictionary<long, string>();
            
            context.RegisterCompilationStartAction((analysisContext =>
            {
                CompilationStartAnalysis(analysisContext, entityHashCodeMap);
            } ));
        }

        private void CompilationStartAnalysis(CompilationStartAnalysisContext context,ConcurrentDictionary<long, string> entityHashCodeMap)
        {
            
            context.RegisterSemanticModelAction((analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.SemanticModel.Compilation.AssemblyName,AnalyzeAssembly.AllModel))
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
            var hashCode = GetLongHashCode(entityName);

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

        private long GetLongHashCode(string str)
        {
            const uint seed = 1313; // 31 131 1313 13131 131313 etc..
            
            ulong hash = 0;
            for (int i = 0; i < str.Length; ++i)
            {
                char c = str[i];
                byte high = (byte)(c >> 8);
                byte low = (byte)(c & byte.MaxValue);
                hash = hash * seed + high;
                hash = hash * seed + low;
            }
            return (long)hash;
        }
    }
}

