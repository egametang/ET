using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityFiledAccessAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "实体字段访问错误";

        private const string MessageFormat = "实体: {0} 字段: {1} 只能在实体类生命周期组件或友元类(含有FriendOfAttribute)中访问";

        private const string Description = "请使用实体类属性或方法访问其他实体字段.";

        private static readonly DiagnosticDescriptor Rule = new DiagnosticDescriptor(DiagnosticIds.EntityFiledAccessAnalyzerRuleId,
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
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModelHotfix))
                {
                    analysisContext.RegisterSemanticModelAction((this.AnalyzeSemanticModel));
                }
            } ));
        }
        
        private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext)
        {
            foreach (var memberAccessExpressionSyntax in analysisContext.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<MemberAccessExpressionSyntax>())
            {
                AnalyzeMemberAccessExpression(analysisContext, memberAccessExpressionSyntax);
            }
        }

        private void AnalyzeMemberAccessExpression(SemanticModelAnalysisContext context, MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            // -----筛选出实体类的字段symbol-----
            ISymbol? filedSymbol = context.SemanticModel.GetSymbolInfo(memberAccessExpressionSyntax).Symbol;
            if (filedSymbol == null || !(filedSymbol is IFieldSymbol))
            {
                return;
            }

            if (filedSymbol.IsStatic)
            {
                return;
            }

            if (filedSymbol.ContainingType.BaseType?.ToString() != Definition.EntityType && filedSymbol.ContainingType.BaseType?.ToString() != Definition.LSEntityType)
            {
                return;
            }

            // -----筛选出在实体类和实体System外部字段访问-----
            // 实体System包括awakeSystem updateSystem等生命周期类和 componentSystem静态方法类

            ClassDeclarationSyntax? accessFieldClassDeclaretion = memberAccessExpressionSyntax.GetParentClassDeclaration();
            if (accessFieldClassDeclaretion == null)
            {
                return;
            }

            INamedTypeSymbol? accessFieldClassSymbol = context.SemanticModel.GetDeclaredSymbol(accessFieldClassDeclaretion);

            if (accessFieldClassSymbol == null)
            {
                return;
            }

            // 实体基类忽略处理
            if (accessFieldClassSymbol.ToString() is Definition.EntityType or Definition.LSEntityType)
            {
                return;
            }

            // 允许类内部访问字段
            if (accessFieldClassSymbol.ToString()== filedSymbol.ContainingType.ToString() )
            {
                return;
            }
            
            //判断是否在实体类生命周期System中, 这里做了修改，周期System也不允许

            //判断是否在实体类的友元类中
            if (this.CheckIsEntityFriendOf(accessFieldClassSymbol, filedSymbol.ContainingType))
            {
                return;
            }

            var builder = ImmutableDictionary.CreateBuilder<string, string?>();
            builder.Add("FriendOfType",filedSymbol.ContainingType.ToString());
            Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax.GetLocation(), builder.ToImmutable(),filedSymbol.ContainingType.ToString(),
                filedSymbol.Name);
            context.ReportDiagnostic(diagnostic);
        }

        private bool CheckIsEntityFriendOf(INamedTypeSymbol accessFieldTypeSymbol, INamedTypeSymbol entityTypeSymbol)
        {
            var attributes = accessFieldTypeSymbol.GetAttributes();
            foreach (AttributeData? attributeData in attributes)
            {
                if (!Definition.FriendAttributes.Contains(attributeData.AttributeClass?.ToString()))
                {
                    continue;
                }

                if (!(attributeData.ConstructorArguments[0].Value is INamedTypeSymbol namedTypeSymbol))
                {
                    continue;
                }

                if (namedTypeSymbol.ToString() == entityTypeSymbol.ToString())
                {
                    return true;
                }
            }

            return false;
        }
    }
}