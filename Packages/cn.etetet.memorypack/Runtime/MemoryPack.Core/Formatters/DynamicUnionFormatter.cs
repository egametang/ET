using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
namespace MemoryPack.Formatters {

public sealed class DynamicUnionFormatter<T> : MemoryPackFormatter<T>
    where T : class
{
    readonly Dictionary<Type, ushort> typeToTag;
    readonly Dictionary<ushort, Type> tagToType;

    public DynamicUnionFormatter(params (ushort Tag, Type Type)[] memoryPackUnions)
    {
        typeToTag = memoryPackUnions.ToDictionary(x => x.Type, x => x.Tag);
        tagToType = memoryPackUnions.ToDictionary(x => x.Tag, x => x.Type);
    }

    public override void Serialize(ref MemoryPackWriter writer, ref T? value)
    {
        if (value == null)
        {
            writer.WriteNullUnionHeader();
            return;
        }

        var type = value.GetType();
        if (typeToTag.TryGetValue(type, out var tag))
        {
            writer.WriteUnionHeader(tag);
            writer.WriteValue(type, value);
        }
        else
        {
            MemoryPackSerializationException.ThrowNotFoundInUnionType(type, typeof(T));
        }
    }

    public override void Deserialize(ref MemoryPackReader reader, ref T? value)
    {
        if (!reader.TryReadUnionHeader(out var tag))
        {
            value = default;
            return;
        }
        
        if (tagToType.TryGetValue(tag, out var type))
        {
            object? v = value;
            reader.ReadValue(type, ref v);
            value = (T?)v;
        }
        else
        {
            MemoryPackSerializationException.ThrowInvalidTag(tag, typeof(T));
        }
    }
}

}