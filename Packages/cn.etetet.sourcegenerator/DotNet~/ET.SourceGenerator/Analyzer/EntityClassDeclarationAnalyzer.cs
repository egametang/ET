using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
     
     [DiagnosticAnalyzer(LanguageNames.CSharp)]
     public class EntityClassDeclarationAnalyzer: DiagnosticAnalyzer
     {
         public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityClassDeclarationAnalyzerRule.Rule,EntityCannotDeclareGenericTypeRule.Rule);

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
             if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
             {
                 return;
             }

             string? t = namedTypeSymbol.BaseType?.BaseType?.ToString();

             if (t=="ET.LSEntity")
             {
                 foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
                 {
                     SyntaxNode classSyntax = declaringSyntaxReference.GetSyntax();
                     Diagnostic diagnostic = Diagnostic.Create(EntityClassDeclarationAnalyzerRule.Rule, classSyntax.GetLocation(), namedTypeSymbol.Name,Definition.LSEntityType );
                     context.ReportDiagnostic(diagnostic);
                 }
                 return;
             }

             if (namedTypeSymbol.BaseType?.ToString()!="ET.LSEntity" && t == Definition.EntityType)
             {
                 foreach (SyntaxReference? declaringSyntaxReference in namedTypeSymbol.DeclaringSyntaxReferences)
                 {
                     SyntaxNode classSyntax = declaringSyntaxReference.GetSyntax();
                     Diagnostic diagnostic = Diagnostic.Create(EntityClassDeclarationAnalyzerRule.Rule, classSyntax.GetLocation(), namedTypeSymbol.Name,Definition.EntityType);
                     context.ReportDiagnostic(diagnostic);
                 }
                 return;
             }

             var baseType = namedTypeSymbol.BaseType?.ToString();
             if (baseType == Definition.EntityType || baseType == Definition.LSEntityType)
             {
                 if (namedTypeSymbol.IsGenericType)
                 {
                     var entitySyntax = namedTypeSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                     Diagnostic diagnostic = Diagnostic.Create(EntityCannotDeclareGenericTypeRule.Rule, entitySyntax?.GetLocation(),
                         namedTypeSymbol.Name);
                     context.ReportDiagnostic(diagnostic);
                 }
             }
         }
     }
     
}