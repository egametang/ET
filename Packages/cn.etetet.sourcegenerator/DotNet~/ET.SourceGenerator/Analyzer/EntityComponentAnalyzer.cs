using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityComponentAnalyzer:DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityComponentAnalyzerRule.Rule,DisableAccessEntityChildAnalyzerRule.Rule);
        
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
            
            // 筛选出 Component函数syntax
            string methodName = memberAccessExpressionSyntax.Name.Identifier.Text;

            if (!Definition.ComponentMethod.Contains(methodName))
            {
                return;
            }
            
            if (!(memberAccessExpressionSyntax?.Parent is InvocationExpressionSyntax invocationExpressionSyntax) ||
                !(context.SemanticModel.GetSymbolInfo(invocationExpressionSyntax).Symbol is IMethodSymbol addComponentMethodSymbol))
            {
                return;
            }
            
            // 获取AComponent函数的调用者类型
            ITypeSymbol? parentTypeSymbol = memberAccessExpressionSyntax.GetMemberAccessSyntaxParentType(context.SemanticModel);
            if (parentTypeSymbol==null)
            {
                return;
            }
            
            // 对于Entity基类会报错 除非标记了EnableAccessEntiyChild
            if (parentTypeSymbol.ToString() is Definition.EntityType or Definition.LSEntityType)
            {
                HandleAcessEntityChild(context,memberAccessExpressionSyntax);
                return;
            }

            // 非Entity的子类 跳过
            if (parentTypeSymbol.BaseType?.ToString()!= Definition.EntityType && parentTypeSymbol.BaseType?.ToString()!= Definition.LSEntityType)
            {
                return;
            }

            // 获取 component实体类型
            ISymbol? componentTypeSymbol = null;
            
            // Component为泛型调用
            if (addComponentMethodSymbol.IsGenericMethod)
            {
                GenericNameSyntax? genericNameSyntax = memberAccessExpressionSyntax?.GetFirstChild<GenericNameSyntax>();

                TypeArgumentListSyntax? typeArgumentList = genericNameSyntax?.GetFirstChild<TypeArgumentListSyntax>();

                var componentTypeSyntax = typeArgumentList?.Arguments.First();
                
                if (componentTypeSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(EntityComponentAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    throw new Exception("componentTypeSyntax==null");
                }

                componentTypeSymbol = context.SemanticModel.GetSymbolInfo(componentTypeSyntax).Symbol;
            }
            //Component为非泛型调用
            else
            {
                SyntaxNode? firstArgumentSyntax = invocationExpressionSyntax.GetFirstChild<ArgumentListSyntax>()?.GetFirstChild<ArgumentSyntax>()
                        ?.ChildNodes().First();
                if (firstArgumentSyntax == null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(EntityComponentAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                    return;
                }

                // 参数为typeOf时 提取Type类型
                if (firstArgumentSyntax is TypeOfExpressionSyntax typeOfExpressionSyntax)
                {
                    firstArgumentSyntax = typeOfExpressionSyntax.Type;
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
                }else if (firstArgumentSymbol is INamedTypeSymbol namedTypeSymbol)
                {
                    componentTypeSymbol = namedTypeSymbol;
                }else if (firstArgumentSymbol is ITypeParameterSymbol)
                {
                    // 忽略typeof(T)参数类型
                    return;
                }
                else if (firstArgumentSymbol != null)
                {
                    Diagnostic diagnostic = Diagnostic.Create(EntityComponentAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSymbol.Name, parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
                else
                {
                    Diagnostic diagnostic = Diagnostic.Create(EntityComponentAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(),
                        firstArgumentSyntax.GetText(), parentTypeSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    return;
                }
            }

            if (componentTypeSymbol==null)
            {
                return;
            }

            // 忽略 component类型为泛型类型
            if (componentTypeSymbol is ITypeParameterSymbol typeParameterSymbol)
            {
                return;
            }
            
            // 忽略 Type参数
            if (componentTypeSymbol.ToString()=="System.Type")
            {
                return;
            }

            // 组件类型为Entity时 忽略检查
            if (componentTypeSymbol.ToString() is Definition.EntityType or Definition.LSEntityType)
            {
                return;
            }
            
            // 判断component类型是否属于约束类型

            //获取component类的parentType标记数据
            INamedTypeSymbol? availableParentTypeSymbol = null;
            bool hasParentTypeAttribute = false;
            foreach (AttributeData? attributeData in componentTypeSymbol.GetAttributes())
            {

                if (attributeData.AttributeClass?.ToString() == Definition.ComponentOfAttribute)
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
                Diagnostic diagnostic = Diagnostic.Create(EntityComponentAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation(), componentTypeSymbol?.Name,
                    parentTypeSymbol?.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
        
        private void HandleAcessEntityChild(SemanticModelAnalysisContext context, MemberAccessExpressionSyntax memberAccessExpressionSyntax)
        {
            //在方法体内
            var methodDeclarationSyntax = memberAccessExpressionSyntax?.GetNeareastAncestor<MethodDeclarationSyntax>();
            if (methodDeclarationSyntax!=null)
            {
                var methodSymbol = context.SemanticModel.GetDeclaredSymbol(methodDeclarationSyntax);

                bool? enableAccessEntiyChild = methodSymbol?.GetAttributes().Any(x => x.AttributeClass?.ToString() == Definition.EnableAccessEntiyChildAttribute);
                if (enableAccessEntiyChild == null || !enableAccessEntiyChild.Value)
                {
                    Diagnostic diagnostic = Diagnostic.Create(DisableAccessEntityChildAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
                return;
            }
                
            //在属性内
            var propertyDeclarationSyntax = memberAccessExpressionSyntax?.GetNeareastAncestor<PropertyDeclarationSyntax>();
            if (propertyDeclarationSyntax!=null)
            {
                var propertySymbol = context.SemanticModel.GetDeclaredSymbol(propertyDeclarationSyntax);
                
                bool? enableAccessEntiyChild = propertySymbol?.GetAttributes().Any(x => x.AttributeClass?.ToString() == Definition.EnableAccessEntiyChildAttribute);
                if (enableAccessEntiyChild == null || !enableAccessEntiyChild.Value)
                {
                    Diagnostic diagnostic = Diagnostic.Create(DisableAccessEntityChildAnalyzerRule.Rule, memberAccessExpressionSyntax?.Name.Identifier.GetLocation());
                    context.ReportDiagnostic(diagnostic);
                }
                return;
            }
        }
    }
}