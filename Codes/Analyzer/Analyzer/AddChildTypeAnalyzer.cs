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
    public class AddChildTypeAnalyzer: DiagnosticAnalyzer
    {
        private const string Title = "AddChild方法类型约束错误";

        private const string MessageFormat = "Type: {0} 不允许作为实体: {1} 的AddChild函数参数类型! 若要允许该类型作为参数,请使用ChildOfAttribute对child实体类标记父级类型";

        private const string Description = "AddChild方法类型约束错误.";

        private static readonly string[] AddChildMethods = { "AddChild", "AddChildWithId" };
        
        private const string EntityType = "ET.Entity";

        private static readonly DiagnosticDescriptor Rule =
                new DiagnosticDescriptor(DiagnosticIds.AddChildTypeAnalyzerRuleId,
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
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.Compilation.AssemblyName, AnalyzeAssembly.All))
            {
                return;
            }

            if (!(context.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }

            // 筛选出 AddChild函数syntax
            string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

            if (!AddChildMethods.Contains(methodName))
            {
                return;
            }

            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol addChildMethodSymbol))
            {
                return;
            }

            // 获取AddChild函数的调用者类型
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

            // 获取 child实体类型
            ISymbol? childTypeSymbol = null;
            // addChild为泛型调用
            if (addChildMethodSymbol.IsGenericMethod)
            {
                GenericNameSyntax? genericNameSyntax = memberAccessExpressionSyntax?.GetFirstChild<GenericNameSyntax>();

                TypeArgumentListSyntax? typeArgumentList = genericNameSyntax?.GetFirstChild<TypeArgumentListSyntax>();

                var childTypeSyntax = typeArgumentList?.Arguments.First();

                if (childTypeSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    throw new Exception("childTypeSyntax==null");
                }

                childTypeSymbol = context.SemanticModel.GetSymbolInfo(childTypeSyntax).Symbol;
            }
            // addChild为非泛型调用
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
                    childTypeSymbol = childLocalSymbol.Type;
                }
                else if (firstArgumentSymbol is IParameterSymbol childParamaterSymbol)
                {
                    childTypeSymbol = childParamaterSymbol.Type;
                }
                else if (firstArgumentSymbol is IMethodSymbol methodSymbol)
                {
                    childTypeSymbol = methodSymbol.ReturnType;
                }
                else if (firstArgumentSymbol is IFieldSymbol fieldSymbol)
                {
                    childTypeSymbol = fieldSymbol.Type;
                }
                else if (firstArgumentSymbol is IPropertySymbol propertySymbol)
                {
                    childTypeSymbol = propertySymbol.Type;
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

            if (childTypeSymbol == null)
            {
                return;
            }

            // 忽略 child类型为泛型类型
            if (childTypeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                return;
            }
            
            // 获取ChildOf标签的约束类型

            if (!(childTypeSymbol is ITypeSymbol childType))
            {
                throw new Exception($"{childTypeSymbol} 不是typeSymbol");
            }

            INamedTypeSymbol? availableParentType = null;
            bool hasAttribute = false;

            foreach (AttributeData? attributeData in childType.GetAttributes())
            {
                if (attributeData.AttributeClass?.Name == "ChildOfAttribute")
                {
                    hasAttribute = true;
                    availableParentType = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
                    break;
                }
            }

            if (hasAttribute && availableParentType==null)
            {
                return;
            }
            
            // 判断父级类型是否属于child约束的父级类型
            if (availableParentType?.ToString() == parentTypeSymbol.ToString())
            {
                return;
            }

            {
                Diagnostic diagnostic = Diagnostic.Create(Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(), childTypeSymbol?.Name,
                    parentTypeSymbol?.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
}