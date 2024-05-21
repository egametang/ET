using System.Collections.Immutable;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Diagnostics;

namespace ET.Analyzer;

[DiagnosticAnalyzer(LanguageNames.CSharp)]
public class EntitySystemAnalyzer: DiagnosticAnalyzer
{
    public override ImmutableArray<DiagnosticDescriptor> SupportedDiagnostics => ImmutableArray.Create(EntitySystemAnalyzerRule.Rule,EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule);

    public override void Initialize(AnalysisContext context)
    {
        if (!AnalyzerGlobalSetting.EnableAnalyzer)
        {
            return;
        }

        context.ConfigureGeneratedCodeAnalysis(GeneratedCodeAnalysisFlags.None);
        context.EnableConcurrentExecution();
        context.RegisterSymbolAction(this.Analyzer, SymbolKind.NamedType);
        context.RegisterSymbolAction(this.AnalyzeIsSystemMethodValid,SymbolKind.NamedType);
    }

    private static ImmutableArray<ETSystemData> SupportedETSystemDatas => ImmutableArray.Create(
        new ETSystemData(Definition.EntitySystemOfAttribute, Definition.EntitySystemAttribute, Definition.EntityType,Definition.EntitySystemAttributeMetaName,
            new SystemMethodData(Definition.IAwakeInterface, Definition.AwakeMethod),
            new SystemMethodData(Definition.IUpdateInterface, Definition.UpdateMethod),
            new SystemMethodData(Definition.IDestroyInterface, Definition.DestroyMethod),
            new SystemMethodData(Definition.IAddComponentInterface, Definition.AddComponentMethod),
            new SystemMethodData(Definition.IDeserializeInterface, Definition.DeserializeMethod),
            new SystemMethodData(Definition.IGetComponentInterface, Definition.GetComponentMethod),
            new SystemMethodData(Definition.ILoadInterface, Definition.LoadMethod),
            new SystemMethodData(Definition.ILateUpdateInterface, Definition.LateUpdateMethod),
            new SystemMethodData(Definition.ISerializeInterface, Definition.SerializeMethod),
            new SystemMethodData(Definition.ILSRollbackInterface,Definition.LSRollbackMethod)),
        new ETSystemData(Definition.LSEntitySystemOfAttribute,Definition.LSEntitySystemAttribute,Definition.LSEntityType,Definition.LSEntitySystemAttributeMetaName,
            new SystemMethodData(Definition.IAwakeInterface, Definition.AwakeMethod),
            new SystemMethodData(Definition.ILSUpdateInterface, Definition.LSUpdateMethod),
            new SystemMethodData(Definition.IDestroyInterface, Definition.DestroyMethod),
            new SystemMethodData(Definition.IAddComponentInterface, Definition.AddComponentMethod),
            new SystemMethodData(Definition.IDeserializeInterface, Definition.DeserializeMethod),
            new SystemMethodData(Definition.IGetComponentInterface, Definition.GetComponentMethod),
            new SystemMethodData(Definition.ILoadInterface, Definition.LoadMethod),
            new SystemMethodData(Definition.ISerializeInterface, Definition.SerializeMethod),
            new SystemMethodData(Definition.ILSRollbackInterface,Definition.LSRollbackMethod)
        )
    );

    private class ETSystemData
    {
        public string EntityTypeName;
        public string SystemOfAttribute;
        public string SystemAttributeShowName;
        public string SystemAttributeMetaName;
        public SystemMethodData[] SystemMethods;

        public ETSystemData(string systemOfAttribute, string systemAttributeShowName, string entityTypeName, string systemAttributeMetaName, params SystemMethodData[] systemMethods)
        {
            this.SystemOfAttribute = systemOfAttribute;
            this.SystemAttributeShowName = systemAttributeShowName;
            this.EntityTypeName = entityTypeName;
            this.SystemAttributeMetaName = systemAttributeMetaName;
            this.SystemMethods = systemMethods;
        }
    }

    public struct SystemMethodData
    {
        public string InterfaceName;
        public string MethodName;

        public SystemMethodData(string interfaceName, string methodName)
        {
            this.InterfaceName = interfaceName;
            this.MethodName = methodName;
        }
    }

    private void Analyzer(SymbolAnalysisContext context)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }

        ImmutableDictionary<string, string?>.Builder? builder = null;
        foreach (ETSystemData? supportedEtSystemData in SupportedETSystemDatas)
        {
            if (supportedEtSystemData != null)
            {
                this.AnalyzeETSystem(context, supportedEtSystemData, ref builder);
            }
        }

        this.ReportNeedGenerateSystem(context, namedTypeSymbol, ref builder);
    }

    private void AnalyzeETSystem(SymbolAnalysisContext context, ETSystemData etSystemData, ref ImmutableDictionary<string, string?>.Builder? builder)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }

        // 筛选出含有SystemOf标签的类
        AttributeData? attr = namedTypeSymbol.GetFirstAttribute(etSystemData.SystemOfAttribute);
        if (attr == null)
        {
            return;
        }

        

        // 获取所属的实体类symbol
        if (attr.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol)
        {
            return;
        }

        bool ignoreAwake = false;
        if (attr.ConstructorArguments.Length>=2 && attr.ConstructorArguments[1].Value is bool ignore)
        {
            ignoreAwake = ignore;
        }
        

        // 排除非Entity子类
        if (entityTypeSymbol.BaseType?.ToString() != etSystemData.EntityTypeName)
        {
            return;
        }

        foreach (INamedTypeSymbol? interfacetypeSymbol in entityTypeSymbol.AllInterfaces)
        {
            if (ignoreAwake && interfacetypeSymbol.IsInterface(Definition.IAwakeInterface))
            {
                continue;
            }

            
            foreach (SystemMethodData systemMethodData in etSystemData.SystemMethods)
            {
                if (interfacetypeSymbol.IsInterface(systemMethodData.InterfaceName))
                {
                    if (interfacetypeSymbol.IsGenericType)
                    {
                        var typeArgs = ImmutableArray.Create<ITypeSymbol>(entityTypeSymbol).AddRange(interfacetypeSymbol.TypeArguments);
                        if (!namedTypeSymbol.HasMethodWithParams(systemMethodData.MethodName, typeArgs.ToArray()))
                        {
                            StringBuilder str = new();
                            str.Append(entityTypeSymbol);
                            str.Append("/");
                            str.Append(etSystemData.SystemAttributeShowName);
                            foreach (ITypeSymbol? typeArgument in interfacetypeSymbol.TypeArguments)
                            {
                                str.Append("/");
                                str.Append(typeArgument);
                            }

                            AddProperty(ref builder, $"{systemMethodData.MethodName}`{interfacetypeSymbol.TypeArguments.Length}", str.ToString());
                        }
                    }
                    else
                    {
                        if (interfacetypeSymbol.IsInterface(Definition.IGetComponentInterface))
                        {
                            if (!namedTypeSymbol.HasMethodWithParams(systemMethodData.MethodName, entityTypeSymbol.ToString(),"System.Type"))
                            {
                                AddProperty(ref builder, systemMethodData.MethodName, $"{entityTypeSymbol}/{etSystemData.SystemAttributeShowName}/System.Type");
                            }
                        }
                        else if (!namedTypeSymbol.HasMethodWithParams(systemMethodData.MethodName, entityTypeSymbol))
                        {
                            AddProperty(ref builder, systemMethodData.MethodName, $"{entityTypeSymbol}/{etSystemData.SystemAttributeShowName}");
                        }
                    }

                    break;
                }
            }
        }
    }

    

    private void AddProperty(ref ImmutableDictionary<string, string?>.Builder? builder, string methodMetaName, string methodArgs)
    {
        if (builder == null)
        {
            builder = ImmutableDictionary.CreateBuilder<string, string?>();
        }

        if (builder.TryGetValue(Definition.EntitySystemInterfaceSequence, out string? seqValue))
        {
            builder[Definition.EntitySystemInterfaceSequence] = $"{seqValue}/{methodMetaName}";
        }
        else
        {
            builder.Add(Definition.EntitySystemInterfaceSequence, methodMetaName);
        }

        builder.Add(methodMetaName, methodArgs);
    }

    private void ReportNeedGenerateSystem(SymbolAnalysisContext context, INamedTypeSymbol namedTypeSymbol, ref ImmutableDictionary<string, string?>.Builder? builder)
    {
        if (builder == null)
        {
            return;
        }

        foreach (SyntaxReference? reference in namedTypeSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is ClassDeclarationSyntax classDeclarationSyntax)
            {
                Diagnostic diagnostic = Diagnostic.Create(EntitySystemAnalyzerRule.Rule, classDeclarationSyntax.Identifier.GetLocation(),
                    builder.ToImmutable(), namedTypeSymbol.Name);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }
    
    private void AnalyzeIsSystemMethodValid(SymbolAnalysisContext context)
    {
        if (!(context.Symbol is INamedTypeSymbol namedTypeSymbol))
        {
            return;
        }
        
        foreach (ISymbol? symbol in namedTypeSymbol.GetMembers())
        {
            if (symbol is not IMethodSymbol methodSymbol)
            {
                continue;
            }

            foreach (var etSystemData in SupportedETSystemDatas)
            {
                if (!methodSymbol.HasAttribute(etSystemData.SystemAttributeMetaName))
                {
                    continue;
                }

                if (methodSymbol.Parameters.Length==0)
                {
                    continue;
                }
                
                AttributeData? attr = namedTypeSymbol.GetFirstAttribute(etSystemData.SystemOfAttribute);
                if (attr == null || attr.ConstructorArguments[0].Value is not INamedTypeSymbol entityTypeSymbol 
                    || entityTypeSymbol.ToString()!=methodSymbol.Parameters[0].Type.ToString())
                {
                    ReportNeedSystemOfAttr(context,methodSymbol,etSystemData);
                }
            }
        }
    }
    
    private void ReportNeedSystemOfAttr(SymbolAnalysisContext context, IMethodSymbol methodSymbol,ETSystemData etSystemData)
    {
        
        foreach (SyntaxReference? reference in methodSymbol.DeclaringSyntaxReferences)
        {
            if (reference.GetSyntax() is MethodDeclarationSyntax methodDeclarationSyntax)
            {
                Diagnostic diagnostic = Diagnostic.Create(EntitySystemMethodNeedSystemOfAttrAnalyzerRule.Rule, methodDeclarationSyntax.Identifier.GetLocation()
                    ,methodSymbol.Name, etSystemData.SystemAttributeShowName,etSystemData.SystemOfAttribute);
                context.ReportDiagnostic(diagnostic);
            }
        }
    }

}