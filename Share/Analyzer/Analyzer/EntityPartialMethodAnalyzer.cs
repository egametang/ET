using System;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;
[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntityPartialMethodAnalyzer : DiagnosticAnalyzer
{


    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntityPartialMethodAnalyzerRule.Rule);
    
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
        
        if (!IsEntitySystem(namedTypeSymbol, out AttributeData? systemOfAttribute))
        {
            
            return;
        }
        
        // 获取所属的实体类symbol
        if (systemOfAttribute?.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol)
        {
            
            return;
        }

        var baseType = entityTypeSymbol.BaseType?.ToString();
        
        // 筛选出实体类
        if (baseType!= Definition.EntityType && baseType != Definition.LSEntityType)
        {
            return;
        }
        
        ImmutableDictionary<string, string?>.Builder? builder = null;

        AnalyzePartialMethod(context, namedTypeSymbol, entityTypeSymbol, ref builder);
        
        ReportNeedGenerateMethdod(context, namedTypeSymbol, ref builder);
        
        
    }

    /// <summary>
    /// 分析需要生成的实体方法 
    /// </summary>
    private void AnalyzePartialMethod(SymbolAnalysisContext context, INamedTypeSymbol systemTypesymbol, INamedTypeSymbol entityTypeSymbol,
    ref ImmutableDictionary<string, string?>.Builder? builder)
    {
        
        foreach (var member in entityTypeSymbol.GetMembers())
        {
            if (member is not IMethodSymbol partialMethodSymbol)
            {
                continue;
            }

            if (partialMethodSymbol.IsStatic)
            {
                continue;
            }

            if (partialMethodSymbol.IsImplicitlyDeclared)
            {
                continue;
            }
            

            var methodSyntax = partialMethodSymbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as MethodDeclarationSyntax;

            if (methodSyntax==null)
            {
                continue;
            }
            
            // 筛选出public partial method
            if (!methodSyntax.Modifiers.Any(SyntaxKind.PartialKeyword) || !methodSyntax.Modifiers.Any(SyntaxKind.PublicKeyword))
            {
                continue;
            }

            if (IsEntityMethodGenerated(systemTypesymbol,partialMethodSymbol))
            {
                continue;
            }
            
            if (builder==null)
            {
                builder = ImmutableDictionary.CreateBuilder<string, string?>();
            }
            
            // 传入方法信息给codefixer
            // 1.returnType 2.methodArgs 3.fullMethodName
            
            string returnType = partialMethodSymbol.ReturnType.ToString();
            
            string methodArgs;
            if (partialMethodSymbol.Parameters.Length!=0)
            {
                methodArgs = $"{entityTypeSymbol} self,{partialMethodSymbol.GetMethodParamsString()}";
            }
            else
            {
                methodArgs = $"{entityTypeSymbol} self";
            }
            
            string fullMethodName = $"{entityTypeSymbol}.{partialMethodSymbol.Name}";
            
            string valueString = $"{returnType}/{methodArgs}/{fullMethodName}";

            if (!builder.ContainsKey(partialMethodSymbol.Name))
            {
                builder.Add(partialMethodSymbol.Name,valueString);
            }
        }
    }

    /// <summary>
    /// EntityMethod 是否已在system中生成
    /// </summary>
    private bool IsEntityMethodGenerated(INamedTypeSymbol systemTypesymbol, IMethodSymbol partialMethodSymbol)
    {
        foreach (var member in systemTypesymbol.GetMembers())
        {
            if (member is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            AttributeData? entityMethodOfAttribute = null;

            foreach (AttributeData? attributeData in methodSymbol.GetAttributes())
            {
                if (attributeData?.AttributeClass?.ToString()==Definition.EntityMethodOfAttributeMetaName)
                {
                    entityMethodOfAttribute = attributeData;
                    break;
                }
            }

            if (entityMethodOfAttribute==null)
            {
                continue; 
            }

            if (entityMethodOfAttribute.ConstructorArguments[0].Value is not string methodName)
            {
                continue;
            }

            if (methodName == partialMethodSymbol.Name)
            {
                return true;
            }

        }

        return false;
    }

    /// <summary>
    ///  对需要生成的方法抛出诊断消息
    /// </summary>
    private void ReportNeedGenerateMethdod(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol,
    ref ImmutableDictionary<string, string?>.Builder? builder)
    {
        if (builder==null)
        {
            return;
        }

        foreach (var reference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ClassDeclarationSyntax classDeclarationSyntax)
            {
                Diagnostic diagnostic = Diagnostic.Create(EntityPartialMethodAnalyzerRule.Rule, classDeclarationSyntax.Identifier.GetLocation(),
                    builder.ToImmutable(), namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

    /// <summary>
    /// 是否是enitysystem类
    /// </summary>
    private bool IsEntitySystem(INamedTypeSymbol namedTypeSymbol, out AttributeData? attributeData)
    {
        attributeData = namedTypeSymbol.GetFirstAttribute(Definition.EntitySystemOfAttribute);
        if (attributeData!=null)
        {
            return true;
        }

        attributeData = namedTypeSymbol.GetFirstAttribute(Definition.LSEntitySystemOfAttribute);
        if (attributeData!=null)
        {
            return true;
        }

        return false;
    }
}