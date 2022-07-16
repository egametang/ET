﻿using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
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
            context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        }

        private void Analyzer(SymbolAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
            {
                return;
            }

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