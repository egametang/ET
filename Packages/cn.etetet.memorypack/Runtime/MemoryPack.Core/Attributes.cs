using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack {

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackableAttribute : Attribute
{
    public GenerateType GenerateType { get; }
    public SerializeLayout SerializeLayout { get; }

    // ctor parameter is parsed in MemoryPackGenerator.Parser TypeMeta for detect which ctor used in MemoryPack.Generator.
    // if modify ctor, be careful.

    /// <summary>
    /// [generateType, (VersionTolerant or CircularReference) ? SerializeLayout.Explicit : SerializeLayout.Sequential]
    /// </summary>
    /// <param name="generateType"></param>
    public MemoryPackableAttribute(GenerateType generateType = GenerateType.Object)
    {
        this.GenerateType = generateType;
        this.SerializeLayout = (generateType == GenerateType.VersionTolerant || generateType == GenerateType.CircularReference)
            ? SerializeLayout.Explicit
            : SerializeLayout.Sequential;
    }

    /// <summary>
    /// [GenerateType.Object, serializeLayout]
    /// </summary>
    public MemoryPackableAttribute(SerializeLayout serializeLayout)
    {
        this.GenerateType = GenerateType.Object;
        this.SerializeLayout = serializeLayout;
    }

    public MemoryPackableAttribute(GenerateType generateType, SerializeLayout serializeLayout)
    {
        this.GenerateType = generateType;
        this.SerializeLayout = serializeLayout;
    }
}

public enum GenerateType
{
    Object,
    VersionTolerant,
    CircularReference,
    Collection,
    NoGenerate
}

public enum SerializeLayout
{
    Sequential, // default
    Explicit
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = true, Inherited = false)]
public sealed class MemoryPackUnionAttribute : Attribute
{
    public ushort Tag { get; }
    public Type Type { get; }

    public MemoryPackUnionAttribute(ushort tag, Type type)
    {
        this.Tag = tag;
        this.Type = type;
    }
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackUnionFormatterAttribute : Attribute
{
    public Type Type { get; }

    public MemoryPackUnionFormatterAttribute(Type type)
    {
        this.Type = type;
    }
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackAllowSerializeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOrderAttribute : Attribute
{
    public int Order { get; }

    public MemoryPackOrderAttribute(int order)
    {
        this.Order = order;
    }
}

#if !UNITY_2021_2_OR_NEWER

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class MemoryPackCustomFormatterAttribute<T> : Attribute
{
    public abstract IMemoryPackFormatter<T> GetFormatter();
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public abstract class MemoryPackCustomFormatterAttribute<TFormatter, T> : Attribute
    where TFormatter : IMemoryPackFormatter<T>
{
    public abstract TFormatter GetFormatter();
}

#endif

// similar naming as System.Text.Json attribtues
// https://docs.microsoft.com/en-us/dotnet/api/system.text.json.serialization.jsonattribute

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackIgnoreAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackIncludeAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackConstructorAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnSerializingAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnSerializedAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnDeserializingAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class MemoryPackOnDeserializedAttribute : Attribute
{
}

// Others

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Interface, AllowMultiple = false, Inherited = false)]
public sealed class GenerateTypeScriptAttribute : Attribute
{
}

}