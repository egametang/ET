using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class LazyFormatter<T> : MemoryPackFormatter<Lazy<T?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref Lazy<T?>? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(1);
        writer.WriteValue(value.Value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref Lazy<T?>? value)
    {
        if (!reader.TryReadObjectHeader(out var count))
        {
            value = null;
            return;
        }

        if (count != 1) MemoryPackSerializationException.ThrowInvalidPropertyCount(1, count);

        var v = reader.ReadValue<T>();
        value = new Lazy<T?>(v);
    }
}

}