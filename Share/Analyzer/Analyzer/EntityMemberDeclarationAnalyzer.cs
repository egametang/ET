using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityMemberDeclarationAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(EntityDelegateDeclarationAnalyzerRule.Rule,EntityFieldDeclarationInEntityAnalyzerRule.Rule);
        
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
            if (namedTypeSymbol.BaseType?.ToString() != Definition.EntityType)
            {
                return;
            }

            AnalyzeDelegateMember(context, namedTypeSymbol);
            AnalyzeEntityMember(context, namedTypeSymbol);
        }

        /// <summary>
        /// 检查委托成员
        /// </summary>
        private void AnalyzeDelegateMember(SymbolAnalysisContext context,INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                
                if (member is IFieldSymbol fieldSymbol && fieldSymbol.Type.BaseType?.ToString()==typeof(MulticastDelegate).FullName)
                {

                    ReportDiagnostic(fieldSymbol,fieldSymbol.Name);
                    continue;
                }
                
                if (member is IPropertySymbol propertySymbol && propertySymbol.Type.BaseType?.ToString()==typeof(MulticastDelegate).FullName)
                {

                    ReportDiagnostic(propertySymbol,propertySymbol.Name);
                    continue;
                }
            }
            
            void ReportDiagnostic(ISymbol symbol,string delegateName)
            {
                foreach (var syntaxReference in symbol.DeclaringSyntaxReferences)
                {
                    var syntax = syntaxReference.GetSyntax();
                    Diagnostic diagnostic = Diagnostic.Create(EntityDelegateDeclarationAnalyzerRule.Rule, syntax.GetLocation(),namedTypeSymbol.Name,delegateName);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }

        /// <summary>
        /// 检查实体成员
        /// </summary>
        private void AnalyzeEntityMember(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                if (member is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                // 忽略静态字段 允许单例实体类
                if (fieldSymbol.IsStatic)
                {
                    continue;
                }
                if (fieldSymbol.Type.ToString()== Definition.EntityType || fieldSymbol.Type.BaseType?.ToString()== Definition.EntityType)
                {
                    var syntaxReference = fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                    if (syntaxReference==null)
                    {
                        continue;
                    }
                    Diagnostic diagnostic = Diagnostic.Create(EntityFieldDeclarationInEntityAnalyzerRule.Rule, syntaxReference.GetSyntax().GetLocation(),namedTypeSymbol.Name,fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}

