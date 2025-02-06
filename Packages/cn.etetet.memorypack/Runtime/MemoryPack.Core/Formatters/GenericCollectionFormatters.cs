using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class GenericCollectionFormatter<TCollection, TElement> : MemoryPackFormatter<TCollection?>
    where TCollection : ICollection<TElement?>, new()
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref TCollection? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement?>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref TCollection? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement?>();

        var collection = new TCollection();
        for (int i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

[Preserve]
public abstract class GenericSetFormatterBase<TSet, TElement> : MemoryPackFormatter<TSet?>
    where TSet : ISet<TElement?>
{
    [Preserve]
    protected abstract TSet CreateSet();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref TSet? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var formatter = writer.GetFormatter<TElement?>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            var v = item;
            formatter.Serialize(ref writer, ref v);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref TSet? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var formatter = reader.GetFormatter<TElement?>();

        var collection = CreateSet();
        for (int i = 0; i < length; i++)
        {
            TElement? v = default;
            formatter.Deserialize(ref reader, ref v);
            collection.Add(v);
        }

        value = collection;
    }
}

[Preserve]
public sealed class GenericSetFormatter<TSet, TElement> : GenericSetFormatterBase<TSet, TElement>
    where TSet : ISet<TElement?>, new()
{
    protected override TSet CreateSet()
    {
        return new();
    }
}

[Preserve]
public abstract class GenericDictionaryFormatterBase<TDictionary, TKey, TValue> : MemoryPackFormatter<TDictionary?>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>
{
    [Preserve]
    protected abstract TDictionary CreateDictionary();

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref TDictionary? value)
    {
        if (value == null)
        {
            writer.WriteNullCollectionHeader();
            return;
        }

        var keyFormatter = writer.GetFormatter<TKey>();
        var valueFormatter = writer.GetFormatter<TValue>();

        writer.WriteCollectionHeader(value.Count);
        foreach (var item in value)
        {
            KeyValuePairFormatter.Serialize(keyFormatter, valueFormatter, ref writer, item!);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref TDictionary? value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        var keyFormatter = reader.GetFormatter<TKey>();
        var valueFormatter = reader.GetFormatter<TValue>();

        var dict = CreateDictionary();
        for (int i = 0; i < length; i++)
        {
            KeyValuePairFormatter.Deserialize(keyFormatter, valueFormatter, ref reader, out var k, out var v);
            dict.Add(k!, v);
        }

        value = dict;
    }
}

[Preserve]
public sealed class GenericDictionaryFormatter<TDictionary, TKey, TValue> : GenericDictionaryFormatterBase<TDictionary, TKey, TValue>
    where TKey : notnull
    where TDictionary : IDictionary<TKey, TValue?>, new()
{
    [Preserve]
    protected override TDictionary CreateDictionary()
    {
        return new();
    }
}

}