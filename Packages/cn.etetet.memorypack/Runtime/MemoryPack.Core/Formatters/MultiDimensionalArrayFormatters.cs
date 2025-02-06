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
using System.Runtime.InteropServices;
using System.Runtime.Serialization;

namespace MemoryPack.Formatters {

[Preserve]
public sealed class TwoDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,]>
{
    // {i-length, j-length, [total-length, values]}

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref T?[,]? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(3);

        var i = value.GetLength(0);
        var j = value.GetLength(1);
        writer.WriteUnmanaged(i, j);

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * i * j;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount + 4);

            Unsafe.WriteUnaligned(ref dest, value.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dest, 4), ref src, (uint)byteCount);
            writer.Advance(byteCount + 4);
        }
        else
#endif
        {

            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T?>();
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref T?[,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != 3)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(3, propertyCount);
        }

        reader.ReadUnmanaged(out int iLength, out int jLength);

        if (!reader.TryReadCollectionHeader(out var length))
        {
            MemoryPackSerializationException.ThrowInvalidCollection();
        }

        if (value != null && value.GetLength(0) == iLength && value.GetLength(1) == jLength && value.Length == length)
        {
            // allow overwrite
        }
        else
        {
            value = new T[iLength, jLength];
        }

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

            reader.Advance(byteCount);
        }
        else
#endif
        {
            var formatter = reader.GetFormatter<T?>();

            var i = 0;
            var j = -1;
            var count = 0;
            while (count++ < length)
            {
                if (j < jLength - 1)
                {
                    j++;
                }
                else
                {
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j]);
            }
        }
    }
}

[Preserve]
public sealed class ThreeDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,,]>
{
    // {i-length, j-length, k-length, [total-length, values]}

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref T?[,,]? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(4);

        var i = value.GetLength(0);
        var j = value.GetLength(1);
        var k = value.GetLength(2);
        writer.WriteUnmanaged(i, j, k);

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * i * j * k;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount + 4);

            Unsafe.WriteUnaligned(ref dest, value.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dest, 4), ref src, (uint)byteCount);
            writer.Advance(byteCount + 4);
        }
        else
#endif
        {
            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T?>();
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref T?[,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != 4)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(4, propertyCount);
        }

        reader.ReadUnmanaged(out int iLength, out int jLength, out int kLength);

        if (!reader.TryReadCollectionHeader(out var length))
        {
            MemoryPackSerializationException.ThrowInvalidCollection();
        }

        if (value != null && value.GetLength(0) == iLength && value.GetLength(1) == jLength && value.GetLength(2) == kLength && value.Length == length)
        {
            // allow overwrite
        }
        else
        {
            value = new T[iLength, jLength, kLength];
        }

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength * kLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

            reader.Advance(byteCount);
        }
        else
#endif
        {
            var formatter = reader.GetFormatter<T?>();

            var i = 0;
            var j = 0;
            var k = -1;
            var count = 0;
            while (count++ < length)
            {
                if (k < kLength - 1)
                {
                    k++;
                }
                else if (j < jLength - 1)
                {
                    k = 0;
                    j++;
                }
                else
                {
                    k = 0;
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j, k]);
            }
        }
    }
}

[Preserve]
public sealed class FourDimensionalArrayFormatter<T> : MemoryPackFormatter<T?[,,,]>
{
    // {i-length, j-length, k-length, l-length, [total-length, values]}

    [Preserve]
    public override void Serialize(ref MemoryPackWriter writer, ref T?[,,,]? value)
    {
        if (value == null)
        {
            writer.WriteNullObjectHeader();
            return;
        }

        writer.WriteObjectHeader(5);

        var i = value.GetLength(0);
        var j = value.GetLength(1);
        var k = value.GetLength(2);
        var l = value.GetLength(3);
        writer.WriteUnmanaged(i, j, k, l);

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * i * j * k * l;
            ref var src = ref MemoryMarshal.GetArrayDataReference(value);
            ref var dest = ref writer.GetSpanReference(byteCount + 4);

            Unsafe.WriteUnaligned(ref dest, value.Length);
            Unsafe.CopyBlockUnaligned(ref Unsafe.Add(ref dest, 4), ref src, (uint)byteCount);
            writer.Advance(byteCount + 4);
        }
        else
#endif
        {
            writer.WriteCollectionHeader(value.Length);
            var formatter = writer.GetFormatter<T?>();
            foreach (var item in value)
            {
                var v = item;
                formatter.Serialize(ref writer, ref v);
            }
        }
    }

    [Preserve]
    public override void Deserialize(ref MemoryPackReader reader, ref T?[,,,]? value)
    {
        if (!reader.TryReadObjectHeader(out var propertyCount))
        {
            value = null;
            return;
        }

        if (propertyCount != 5)
        {
            MemoryPackSerializationException.ThrowInvalidPropertyCount(5, propertyCount);
        }

        reader.ReadUnmanaged(out int iLength, out int jLength, out int kLength, out int lLength);

        if (!reader.TryReadCollectionHeader(out var length))
        {
            MemoryPackSerializationException.ThrowInvalidCollection();
        }

        if (value != null && value.GetLength(0) == iLength && value.GetLength(1) == jLength && value.GetLength(2) == kLength && value.GetLength(3) == lLength && value.Length == length)
        {
            // allow overwrite
        }
        else
        {
            value = new T[iLength, jLength, kLength, lLength];
        }

#if NET7_0_OR_GREATER
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T?>())
        {
            var byteCount = Unsafe.SizeOf<T>() * iLength * jLength * kLength * lLength;
            ref var dest = ref MemoryMarshal.GetArrayDataReference(value);
            ref var src = ref reader.GetSpanReference(byteCount);
            Unsafe.CopyBlockUnaligned(ref dest, ref src, (uint)byteCount);

            reader.Advance(byteCount);
        }
        else
#endif
        {
            var formatter = reader.GetFormatter<T?>();

            var i = 0;
            var j = 0;
            var k = 0;
            var l = -1;
            var count = 0;
            while (count++ < length)
            {
                if (l < lLength - 1)
                {
                    l++;
                }
                else if (k < kLength - 1)
                {
                    l = 0;
                    k++;
                }
                else if (j < jLength - 1)
                {
                    l = 0;
                    k = 0;
                    j++;
                }
                else
                {
                    l = 0;
                    k = 0;
                    j = 0;
                    i++;
                }

                formatter.Deserialize(ref reader, ref value[i, j, k, l]);
            }
        }
    }
}

}