using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class UniqueIdAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(UniqueIdRangeAnaluzerRule.Rule,UniqueIdDuplicateAnalyzerRule.Rule);
        
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

            // 筛选出含有UniqueId标签的类
            var attr = namedTypeSymbol.GetFirstAttribute(Definition.UniqueIdAttribute);
            if (attr==null)
            {
                return;
            }

            // 获取id 最小值最大值
            var minIdValue = attr.ConstructorArguments[0].Value;

            var maxIdValue = attr.ConstructorArguments[1].Value;

            if (minIdValue==null || maxIdValue==null)
            {
                return;
            }

            int minId = (int)minIdValue;

            int maxId = (int)maxIdValue;

            HashSet<int> IdSet = new HashSet<int>();

            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is IFieldSymbol { IsConst: true, ConstantValue: int id } fieldSymbol)
                {
                    if (id<minId || id>maxId)
                    {
                        ReportDiagnostic(fieldSymbol,id, UniqueIdRangeAnaluzerRule.Rule);
                    }else if (IdSet.Contains(id))
                    {
                        ReportDiagnostic(fieldSymbol,id,UniqueIdDuplicateAnalyzerRule.Rule);
                    }
                    else
                    {
                        IdSet.Add(id);
                    }
                }
            }
            
            
            void ReportDiagnostic(IFieldSymbol fieldSymbol, int idValue, DiagnosticDescriptor rule)
            {
                ET.Analyzer.ClientClassInServerAnalyzer analyzer = new ClientClassInServerAnalyzer();
                foreach (var syntaxReference in fieldSymbol.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxReference.GetSyntax();
                    Diagnostic diagnostic = Diagnostic.Create(rule, syntax.GetLocation(),namedTypeSymbol.Name,fieldSymbol.Name,idValue.ToString());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}

