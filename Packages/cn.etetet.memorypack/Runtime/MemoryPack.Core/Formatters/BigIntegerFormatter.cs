using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Internal;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class BigIntegerFormatter : MemoryPackFormatter<BigInteger>
{
    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref BigInteger value)
    {
#if !UNITY_2021_2_OR_NEWER
        Span<byte> temp = stackalloc byte[255];
        if (value.TryWriteBytes(temp, out var written))
        {
            writer.WriteUnmanagedSpan(temp.Slice(written));
            return;
        }
        else
#endif
        {
            var byteArray = value.ToByteArray();
            writer.WriteUnmanagedArray(byteArray);
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref BigInteger value)
    {
        if (!reader.TryReadCollectionHeader(out var length))
        {
            value = default;
            return;
        }

        ref var src = ref reader.GetSpanReference(length);
        value = new BigInteger(MemoryMarshal.CreateReadOnlySpan(ref src, length));

        reader.Advance(length);
    }
}

}