using System.Collections.Generic;
using System.Text;
using ET.Analyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace ET.Generator;

[Generator(LanguageNames.CSharp)]
public class ETEntitySerializeFormatterGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications((() => ETEntitySerializeFormatterSyntaxContextReceiver.Create()));
    }

    public void Execute(GeneratorExecutionContext context)
    {
        
        if (context.SyntaxContextReceiver is not ETEntitySerializeFormatterSyntaxContextReceiver receiver || receiver.entities.Count==0)
        {
            return;
        }
        
        int count = receiver.entities.Count;
        string typeHashCodeMapDeclaration = GenerateTypeHashCodeMapDeclaration(receiver);
        string serializeContent = GenerateSerializeContent(receiver);
        string deserializeContent = GenerateDeserializeContent(receiver);
        string genericTypeParam = context.Compilation.AssemblyName == AnalyzeAssembly.DotNetModel? "<TBufferWriter>" : "";
        string scopedCode = context.Compilation.AssemblyName == AnalyzeAssembly.DotNetModel? "scoped" : "";
        string code = $$"""
#nullable enable
#pragma warning disable CS0108 // hides inherited member
#pragma warning disable CS0162 // Unreachable code
#pragma warning disable CS0164 // This label has not been referenced
#pragma warning disable CS0219 // Variable assigned but never used
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8601 // Possible null reference assignment
#pragma warning disable CS8602
#pragma warning disable CS8604 // Possible null reference argument for parameter
#pragma warning disable CS8619
#pragma warning disable CS8620
#pragma warning disable CS8631 // The type cannot be used as type parameter in the generic type or method
#pragma warning disable CS8765 // Nullability of type of parameter
#pragma warning disable CS9074 // The 'scoped' modifier of parameter doesn't match overridden or implemented member
#pragma warning disable CA1050 // Declare types in namespaces.

using System;
using MemoryPack;

[global::MemoryPack.Internal.Preserve]
public class ETEntitySerializeFormatter : MemoryPackFormatter<global::{{Definition.EntityType}}>
{
    static readonly System.Collections.Generic.Dictionary<Type, long> __typeToTag = new({{count}})
    {
{{typeHashCodeMapDeclaration}}
    };
    
    [global::MemoryPack.Internal.Preserve]
    public override void Serialize{{genericTypeParam}}(ref MemoryPackWriter{{genericTypeParam}} writer,{{scopedCode}} ref global::{{Definition.EntityType}}? value)
    {

        if (value == null)
        {
            writer.WriteNullUnionHeader();
            return;
        }

        if (__typeToTag.TryGetValue(value.GetType(), out var tag))
        {
            writer.WriteValue<byte>(global::MemoryPack.MemoryPackCode.WideTag);
            writer.WriteValue<long>(tag);
            switch (tag)
            {
{{serializeContent}}               
                default:
                    break;
            }
        }
        else
        {
            MemoryPackSerializationException.ThrowNotFoundInUnionType(value.GetType(), typeof(global::{{Definition.EntityType}}));
        }
    }
    
    [global::MemoryPack.Internal.Preserve]
    public override void Deserialize(ref MemoryPackReader reader,{{scopedCode}} ref global::{{Definition.EntityType}}? value)
    {

        bool isNull = reader.ReadValue<byte>() == global::MemoryPack.MemoryPackCode.NullObject;
        if (isNull)
        {
            value = default;
            return;
        }
        
        var tag = reader.ReadValue<long>();

        switch (tag)
        {
{{deserializeContent}}
            default:
                //MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(global::IForExternalUnion));
                break;
        }
    }
}
namespace ET
{
    public static partial class EntitySerializeRegister
    {
        static partial void Register()
        {
            if (!global::MemoryPack.MemoryPackFormatterProvider.IsRegistered<global::{{Definition.EntityType}}>())
            {
                global::MemoryPack.MemoryPackFormatterProvider.Register(new ETEntitySerializeFormatter());
            }
        }
    }
}
""";
        context.AddSource($"ETEntitySerializeFormatterGenerator.g.cs",code);
    }

    private string GenerateTypeHashCodeMapDeclaration(ETEntitySerializeFormatterSyntaxContextReceiver receiver)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entityName in receiver.entities)
        {
            sb.AppendLine($$"""        { typeof(global::{{entityName}}), {{entityName.GetLongHashCode()}} },""");
        }
        return sb.ToString();
    }

    private string GenerateSerializeContent(ETEntitySerializeFormatterSyntaxContextReceiver receiver)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entityName in receiver.entities)
        {
            sb.AppendLine($$"""                case {{entityName.GetLongHashCode()}}: writer.WritePackable(System.Runtime.CompilerServices.Unsafe.As<global::{{Definition.EntityType}}?, global::{{entityName}}>(ref value)); break;""");
        }
        return sb.ToString();
    }

    private string GenerateDeserializeContent(ETEntitySerializeFormatterSyntaxContextReceiver receiver)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var entityName in receiver.entities)
        {
            sb.AppendLine($$"""
            case {{entityName.GetLongHashCode()}}:
                    if(value is global::{{entityName}})
                    {
                        reader.ReadPackable(ref System.Runtime.CompilerServices.Unsafe.As<global::{{Definition.EntityType}}?, global::{{entityName}}>(ref value));
                    }else{
                        value = (global::{{entityName}})reader.ReadPackable<global::{{entityName}}>();
                    }
                    break;
""");
        }
        return sb.ToString();
    }
    
    class ETEntitySerializeFormatterSyntaxContextReceiver : ISyntaxContextReceiver
    {
        internal static ISyntaxContextReceiver Create()
        {
            return new ETEntitySerializeFormatterSyntaxContextReceiver();
        }
        
        public HashSet<string> entities = new HashSet<string>();
        
        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (!AnalyzerHelper.IsAssemblyNeedAnalyze(context.SemanticModel.Compilation.AssemblyName,AnalyzeAssembly.AllLogicModel))
            {
                return;
            }

            if (context.Node is not ClassDeclarationSyntax classDeclarationSyntax)
            {
                return;
            }
            
            var classTypeSymbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
            if (classTypeSymbol==null)
            {
                return;
            }

            var baseType = classTypeSymbol.BaseType?.ToString();
            
            // 筛选出实体类
            if (baseType!= Definition.EntityType && baseType != Definition.LSEntityType)
            {
                return;
            }

            if (!classTypeSymbol.HasAttribute("MemoryPack.MemoryPackableAttribute"))
            {
                return;
            }
            
            entities.Add(classTypeSymbol.ToString());
        }
    }
}