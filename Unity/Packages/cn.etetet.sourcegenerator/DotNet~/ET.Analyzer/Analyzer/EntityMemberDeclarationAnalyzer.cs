using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer
{
    [DiagnosticAnalyzer(LanguageNames.CSharp)]
    public class EntityMemberDeclarationAnalyzer: DiagnosticAnalyzer
    {
        public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics =>ImmutableArray.Create(EntityDelegateDeclarationAnalyzerRule.Rule,
            EntityFieldDeclarationInEntityAnalyzerRule.Rule, LSEntityFloatMemberAnalyzer.Rule, EntityComponentChildAnalyzerRule.Rule);
        
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
                if (AnalyzerHelper.IsAssemblyNeedAnalyze(analysisContext.Compilation.AssemblyName,AnalyzeAssembly.AllModel))
                {
                    analysisContext.RegisterSemanticModelAction((this.AnalyzeSemanticModel));
                }
            } ));
        }
        
        private void AnalyzeSemanticModel(SemanticModelAnalysisContext analysisContext)
        {
            foreach (var classDeclarationSyntax in analysisContext.SemanticModel.SyntaxTree.GetRoot().DescendantNodes<ClassDeclarationSyntax>())
            {
                var classTypeSymbol = analysisContext.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (classTypeSymbol!=null)
                {
                    Analyzer(analysisContext, classTypeSymbol);
                }
            }
        }

        private void Analyzer(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            
            var baseType = namedTypeSymbol.BaseType?.ToString();
            // 筛选出实体类
            if (baseType== Definition.EntityType)
            {
                AnalyzeDelegateMember(context, namedTypeSymbol);
                AnalyzeEntityMember(context, namedTypeSymbol);
                AnalyzeComponentChildAttr(context, namedTypeSymbol);
            }else if (baseType == Definition.LSEntityType)
            {
                AnalyzeDelegateMember(context, namedTypeSymbol);
                AnalyzeEntityMember(context, namedTypeSymbol);
                AnalyzeFloatMemberInLSEntity(context,namedTypeSymbol);
                AnalyzeComponentChildAttr(context, namedTypeSymbol);
            }
        }

        /// <summary>
        /// 检查委托成员
        /// </summary>
        private void AnalyzeDelegateMember(SemanticModelAnalysisContext context,INamedTypeSymbol namedTypeSymbol)
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
        private void AnalyzeEntityMember(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
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

                // 字段类型是否是实体类型数组
                if (fieldSymbol.Type is IArrayTypeSymbol arrayTypeSymbol)
                {
                    if (arrayTypeSymbol.ElementType.IsETEntity())
                    {
                        var syntaxReference = fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                        if (syntaxReference==null)
                        {
                            continue;
                        }
                        Diagnostic diagnostic = Diagnostic.Create(EntityFieldDeclarationInEntityAnalyzerRule.Rule, syntaxReference.GetSyntax().GetLocation(),namedTypeSymbol.Name,fieldSymbol.Name);
                        context.ReportDiagnostic(diagnostic);
                    }
                    continue;
                }

                if (fieldSymbol.Type is not INamedTypeSymbol namedTypeSymbol2)
                {
                    continue;
                }

                // 字段类型是否是实体类
                if (namedTypeSymbol2.IsETEntity())
                {
                    var syntaxReference = fieldSymbol.DeclaringSyntaxReferences.FirstOrDefault();
                    if (syntaxReference==null)
                    {
                        continue;
                    }
                    Diagnostic diagnostic = Diagnostic.Create(EntityFieldDeclarationInEntityAnalyzerRule.Rule, syntaxReference.GetSyntax().GetLocation(),namedTypeSymbol.Name,fieldSymbol.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }

                // 字段类型是否是含实体类参数的泛型类
                if (namedTypeSymbol2.IsGenericType&&GenericTypeHasEntityTypeArgs(namedTypeSymbol2))
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

        /// <summary>
        /// 检查LSEntity中 是否有浮点数字段
        /// </summary>
        private void AnalyzeFloatMemberInLSEntity(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            
            foreach (var member in namedTypeSymbol.GetMembers())
            {
                INamedTypeSymbol? memberType = null;
                
                if (member is IFieldSymbol fieldSymbol)
                {
                    memberType = fieldSymbol.Type as INamedTypeSymbol;
                }

                if (member is IPropertySymbol propertySymbol)
                {
                    memberType = propertySymbol.Type as INamedTypeSymbol;
                }

                if (memberType==null)
                {
                    continue;
                }
                
                if (memberType.SpecialType is  SpecialType.System_Single or SpecialType.System_Double)
                {
                    var syntaxReference = member.DeclaringSyntaxReferences.FirstOrDefault();
                    if (syntaxReference==null)
                    {
                        continue;
                    }
                    Diagnostic diagnostic = Diagnostic.Create(LSEntityFloatMemberAnalyzer.Rule, syntaxReference.GetSyntax().GetLocation(),namedTypeSymbol.Name,member.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }

                if (memberType.IsGenericType && GenericTypeHasFloatTypeArgs(memberType))
                {
                    var syntaxReference = member.DeclaringSyntaxReferences.FirstOrDefault();
                    if (syntaxReference==null)
                    {
                        continue;
                    }
                    Diagnostic diagnostic = Diagnostic.Create(LSEntityFloatMemberAnalyzer.Rule, syntaxReference.GetSyntax().GetLocation(),namedTypeSymbol.Name,member.Name);
                    context.ReportDiagnostic(diagnostic);
                    continue;
                }
            }
        }


        /// <summary>
        /// 泛型类 是否含有浮点数类型参数
        /// 对于嵌套泛型参数 递归判断
        /// </summary>
        private bool GenericTypeHasFloatTypeArgs(INamedTypeSymbol namedTypeSymbol)
        {
            var typeArgs = namedTypeSymbol.TypeArguments;
            foreach (var typeSymbol in typeArgs)
            {
                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol2)
                {
                    break;
                }

                if (namedTypeSymbol2.IsGenericType)
                {
                    if (GenericTypeHasFloatTypeArgs(namedTypeSymbol2))
                    {
                        return true;
                    }
                }
                else
                {
                    if (namedTypeSymbol2.SpecialType is SpecialType.System_Single or SpecialType.System_Double)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// 实体类是否同时标记为component child
        /// </summary>
        private void AnalyzeComponentChildAttr(SemanticModelAnalysisContext context, INamedTypeSymbol namedTypeSymbol)
        {
            bool hasComponentOf = namedTypeSymbol.HasAttribute(Definition.ComponentOfAttribute);
            bool hasChildOf = namedTypeSymbol.HasAttribute(Definition.ChildOfAttribute);
            if (hasComponentOf && hasChildOf)
            {
                var syntax = namedTypeSymbol.DeclaringSyntaxReferences.First().GetSyntax() as ClassDeclarationSyntax;
                Diagnostic diagnostic = Diagnostic.Create(EntityComponentChildAnalyzerRule.Rule, syntax?.Identifier.GetLocation(),namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }


        /// <summary>
        /// 泛型类 是否含有的实体类型参数 
        /// 对于嵌套泛型参数 递归判断
        /// </summary>
        private bool GenericTypeHasEntityTypeArgs(INamedTypeSymbol namedTypeSymbol)
        {
            if (namedTypeSymbol.IsEntityRefOrEntityWeakRef())
            {
                return false;
            }
            
            var typeArgs = namedTypeSymbol.TypeArguments;
            foreach (var typeSymbol in typeArgs)
            {
                if (typeSymbol is not INamedTypeSymbol namedTypeSymbol2)
                {
                    break;
                }

                if (namedTypeSymbol2.IsGenericType)
                {
                    if (GenericTypeHasEntityTypeArgs(namedTypeSymbol2))
                    {
                        return true;
                    }
                }
                else
                {
                    if (namedTypeSymbol2.IsETEntity())
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}

