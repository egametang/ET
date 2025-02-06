using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Formatters {

[Preserve]
public static class KeyValuePairFormatter
{
    // for Dictionary serialization

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize<TKey, TValue>(IMemoryPackFormatter<TKey> keyFormatter, IMemoryPackFormatter<TValue> valueFormatter, ref MemoryPackWriter writer, KeyValuePair<TKey?, TValue?> value)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        value.Deconstruct(out var k, out var v);
        keyFormatter.Serialize(ref writer, ref k);
        valueFormatter.Serialize(ref writer, ref v);
    }

    [Preserve]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Deserialize<TKey, TValue>(IMemoryPackFormatter<TKey> keyFormatter, IMemoryPackFormatter<TValue> valueFormatter, ref MemoryPackReader reader, out TKey? key, out TValue? value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            reader.DangerousReadUnmanaged(out KeyValuePair<TKey?, TValue?> kvp);
            key = kvp.Key;
            value = kvp.Value;
            return;
        }

        key = default;
        value = default;
        keyFormatter.Deserialize(ref reader, ref key);
        valueFormatter.Deserialize(ref reader, ref value);
    }
}

[Preserve]
public sealed class KeyValuePairFormatter<TKey, TValue> : MemoryPackFormatter<KeyValuePair<TKey?, TValue?>>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            writer.DangerousWriteUnmanaged(value);
            return;
        }

        writer.WriteValue(value.Key);
        writer.WriteValue(value.Value);
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref KeyValuePair<TKey?, TValue?> value)
    {
        if (!System.Runtime.CompilerServices.RuntimeHelpers.IsReferenceOrContainsReferences<KeyValuePair<TKey?, TValue?>>())
        {
            reader.DangerousReadUnmanaged(out value);
            return;
        }

        value = new KeyValuePair<TKey?, TValue?>(
            reader.ReadValue<TKey>(),
            reader.ReadValue<TValue>()
        );
    }
}

}