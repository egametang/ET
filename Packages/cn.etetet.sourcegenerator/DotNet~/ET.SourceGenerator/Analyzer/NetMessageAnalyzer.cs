using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class NetMessageAnalyzer : DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(NetMessageAnalyzerRule.Rule);
        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }
            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterSyntaxNodeAction(this.AnalyzeClassDeclaration, SyntaxKind.ClassDeclaration);
        }
        
        private void AnalyzeClassDeclaration(SyntaxNodeAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllModel))
            {
                return;
            }

            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }

            var namedTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (namedTypeSymbol==null)
            {
                return;
            }

            // 筛选出继承IMessage接口的类
            if (!namedTypeSymbol.HasInterface(Definition.IMessageInterface))
            {
                return;
            }

            foreach (var member in namedTypeSymbol.GetMembers())
            {
                ITypeSymbol? memberType = null;
                
                if (member is IFieldSymbol fieldSymbol)
                {
                    memberType = fieldSymbol.Type;
                }else if (member is IPropertySymbol propertySymbol)
                {
                    memberType = propertySymbol.Type;
                }

                if (memberType==null)
                {
                    continue;
                }

                if (!memberType.IsETEntity())
                {
                    continue;
                }

                var memberSyntax = member.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
                Diagnostic diagnostic = Diagnostic.Create(NetMessageAnalyzerRule.Rule, memberSyntax?.GetLocation(),namedTypeSymbol.Name, member.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}

