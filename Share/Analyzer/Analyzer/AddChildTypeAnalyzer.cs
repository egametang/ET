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
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>
                ImmutableArray.Create(AddChildTypeAnalyzerRule.Rule, DisableAccessEntityChildAnalyzerRule.Rule);

        public override void Initialize(AnalysisContext context)
        {
            if (!AnalyzerGlobalSetting.EnableAnalyzer)
            {
                return;
            }

            context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
            context.EnableConcurrentExecution();
            context.RegisterCompilationStartAction(analysisContext =>
            {
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName, AnalyzeAssembly.AllModelHotfix))
                {
                    analysisContext.RegisterSyntaxNodeAction(this.AnalyzeMemberAccessExpression, SyntaxKind.SimpleMemberAccessExpression);
                }
            });
        }

        private void AnalyzeMemberAccessExpression(SyntaxNodeAnalysisContext context)
        {
            if (!(context.Node is MemberAccessExpressionSyntax memberAccessExpressionSyntax))
            {
                return;
            }

            // 筛选出 AddChild函数syntax
            string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

            if (!Definition.AddChildMethods.Contains(methodName))
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
            if (parentTypeSymbol == null)
            {
                return;
            }

            // 对于Entity基类会报错 除非标记了EnableAccessEntiyChild
            if (parentTypeSymbol.ToString() is Definition.EntityType or Definition.LSEntityType)
            {
                HandleAcessEntityChild(context);
                return;
            }

            // 非Entity的子类 跳过
            if (parentTypeSymbol.BaseType?.ToString() != Definition.EntityType && parentTypeSymbol.BaseType?.ToString() != Definition.LSEntityType)
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

                TypeSyntax? childTypeSyntax = typeArgumentList?.Arguments.First();

                if (childTypeSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(AddChildTypeAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
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
                    Diagnostic diagnostic = Diagnostic.Create(AddChildTypeAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
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
                    Diagnostic diagnostic = Diagnostic.Create(AddChildTypeAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSymbol.Name, parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                else
                {
                    Diagnostic diagnostic = Diagnostic.Create(AddChildTypeAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
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
                if (attributeData.AttributeClass?.ToString() == Definition.ChildOfAttribute)
                {
                    hasAttribute = true;
                    availableParentType = attributeData.ConstructorArguments[0].Value as INamedTypeSymbol;
                    break;
                }
            }

            if (hasAttribute && availableParentType == null)
            {
                return;
            }

            // 判断父级类型是否属于child约束的父级类型
            if (availableParentType?.ToString() == parentTypeSymbol.ToString())
            {
                return;
            }

            {
                Diagnostic diagnostic = Diagnostic.Create(AddChildTypeAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                    childTypeSymbol?.Name,
                    parentTypeSymbol?.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }

        private void HandleAcessEntityChild(SyntaxNodeAnalysisContext context)
        {
            MemberAccessExpressionSyntax? memberAccessExpressionSyntax = context.Node as MemberAccessExpressionSyntax;
            //在方法体内
            MethodDeclarationSyntax? methodDeclarationSyntax = memberAccessExpressionSyntax?.GetNeareastAncestor<MethodDeclarationSyntax>();
            if (methodDeclarationSyntax != null)
            {
                IMethodSymbol? methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);

                bool? enableAccessEntiyChild = methodSymbol?.GetAttributes()
                        .Any(x => x.AttributeClass?.ToString() == Definition.EnableAccessEntiyChildAttribute);
                if (enableAccessEntiyChild == null || !enableAccessEntiyChild.Value)
                {
                    Diagnostic diagnostic = Diagnostic.Create(DisableAccessEntityChildAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }

                return;
            }

            //在属性内
            PropertyDeclarationSyntax? propertyDeclarationSyntax = memberAccessExpressionSyntax?.GetNeareastAncestor<PropertyDeclarationSyntax>();
            if (propertyDeclarationSyntax != null)
            {
                IPropertySymbol? propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);

                bool? enableAccessEntiyChild = propertySymbol?.GetAttributes()
                        .Any(x => x.AttributeClass?.ToString() == Definition.EnableAccessEntiyChildAttribute);
                if (enableAccessEntiyChild == null || !enableAccessEntiyChild.Value)
                {
                    Diagnostic diagnostic = Diagnostic.Create(DisableAccessEntityChildAnalyzerRule.Rule,
                        memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
            }
        }
    }
}