using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class AddComponentAnalyzer:DiagnosticAnalyzer
    {
        private const string Title = "AddComponent方法类型约束错误";

        private const string MessageFormat = "Type: {0} 不允许作为实体: {1} 的AddComponent函数参数类型! 若要允许该类型作为参数,请使用ParentTypeAttribute对组件实体类进行标记";

        private const string Description = "AddComponent方法类型约束错误.";

        private const string AddComponentMethod = "AddComponent";
        
        private const string EntityType = "ET.Entity";
        
        private static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AddComponentAnalyzerRuleId,
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
            context.RegisterSyntaxNodeAction(this.AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
        }

        private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.AllHotfix))
            {
                return;
            }

            if (!(context.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }
            
            // 筛选出 AddComponent函数syntax
            string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

            if (methodName!=AddComponentMethod)
            {
                return;
            }
            
            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol addComponentMethodSymbol))
            {
                return;
            }
            
            // 获取AddComponent函数的调用者类型
            ITypeSymbol? parentTypeSymbol = memberAccessExpressionSyntax.GetMemberAccessSyntaxParentType(context.SemanticModel);
            if (parentTypeSymbol==null)
            {
                return;
            }
            
            // 只检查Entity的子类
            if (parentTypeSymbol.BaseType?.ToString()!= EntityType)
            {
                return;
            }
            
            
            
            // 获取 component实体类型
            ISymbol? componentTypeSymbol = null;
            
            // addComponent为泛型调用
            if (addComponentMethodSymbol.IsGenericMethod)
            {
                GenericNameSyntax? genericNameSyntax = memberAccessExpressionSyntax?.GetFirstChild<GenericNameSyntax>();

                TypeArgumentListSyntax? typeArgumentList = genericNameSyntax?.GetFirstChild<TypeArgumentListSyntax>();

                IdentifierNameSyntax? componentTypeSyntax = typeArgumentList?.GetFirstChild<IdentifierNameSyntax>();

                if (componentTypeSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    throw new Exception("componentTypeSyntax==null");
                }

                componentTypeSymbol = context.SemanticModel.GetSymbolInfo(componentTypeSyntax).Symbol;
            }
            //addComponent为非泛型调用
            else
            {
                SyntaxNode? firstArgumentSyntax = invocationExpressionSyntax.GetFirstChild<ArgumentListSyntax>()?.GetFirstChild<ArgumentSyntax>()
                        ?.ChildNodes().First();
                if (firstArgumentSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                ISymbol? firstArgumentSymbol = context.SemanticModel.GetSymbolInfo(firstArgumentSyntax).Symbol;

                if (firstArgumentSymbol is ILocalSymbol childLocalSymbol)
                {
                    componentTypeSymbol = childLocalSymbol.Type;
                }
                else if (firstArgumentSymbol is IParameterSymbol childParamaterSymbol)
                {
                    componentTypeSymbol = childParamaterSymbol.Type;
                }
                else if (firstArgumentSymbol is IMethodSymbol methodSymbol)
                {
                    componentTypeSymbol = methodSymbol.ReturnType;
                }
                else if (firstArgumentSymbol is IFieldSymbol fieldSymbol)
                {
                    componentTypeSymbol = fieldSymbol.Type;
                }
                else if (firstArgumentSymbol is IPropertySymbol propertySymbol)
                {
                    componentTypeSymbol = propertySymbol.Type;
                }
                else if (firstArgumentSymbol != null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSymbol.Name, parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                else
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSyntax.GetText(), parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            if (componentTypeSymbol==null)
            {
                return;
            }

            // 组件类型为Entity时 忽略检查
            if (componentTypeSymbol.ToString()== EntityType)
            {
                return;
            }
            
            // 判断component类型是否属于约束类型

            //获取component类的parentType标记数据
            INamedTypeSymbol? availableParentTypeSymbol = null;
            bool hasParentTypeAttribute = false;
            foreach (AttributeData? attributeData in componentTypeSymbol.GetAttributes())
            {

                if (attributeData.AttributeClass?.Name == "ComponentOfAttribute")
                {
                    hasParentTypeAttribute = true;
                    if (attributeData.ConstructorArguments[0].Value is INamedTypeSymbol typeSymbol)
                    {
                        availableParentTypeSymbol = typeSymbol;
                        break;
                    }
                }
            }

            if (hasParentTypeAttribute&&availableParentTypeSymbol==null)
            {
                return;
            }

            // 符合约束条件 通过检查
            if (availableParentTypeSymbol!=null && availableParentTypeSymbol.ToString()==parentTypeSymbol.ToString())
            {
                return;
            }

            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(), componentTypeSymbol?.Name,
                    parentTypeSymbol?.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}