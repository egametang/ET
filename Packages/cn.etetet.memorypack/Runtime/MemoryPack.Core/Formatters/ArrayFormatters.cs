using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using MemoryPack.Formatters;
using MemoryPack.Internal;
using System.Buffers;
using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;

// Array and Array-like type formatters
// T[]
// T[] where T: unmnaged
// Memory
// ReadOnlyMemory
// ArraySegment
// ReadOnlySequence

namespace MemoryPack
{
    public static partial class MemoryPackFormatterProvider
    {
        static readonly Dictionary<Type, Type> ArrayLikeFormatters = new Dictionary<Type, Type>(4)
        {
            // If T[], choose UnmanagedArrayFormatter or DangerousUnmanagedTypeArrayFormatter or ArrayFormatter
            { typeof(ArraySegment<>), typeof(ArraySegmentFormatter<>) },
            { typeof(Memory<>), typeof(MemoryFormatter<>) },
            { typeof(ReadOnlyMemory<>), typeof(ReadOnlyMemoryFormatter<>) },
            { typeof(ReadOnlySequence<>), typeof(ReadOnlySequenceFormatter<>) },
        };
    }
}

namespace MemoryPack.Formatters
{
    [Preserve]
    public sealed class UnmanagedArrayFormatter<T> : MemoryPackFormatter<T[]>
            where T : unmanaged
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref T[]? value)
        {
            writer.WriteUnmanagedArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref T[]? value)
        {
            reader.ReadUnmanagedArray<T>(ref value);
        }
    }

    [Preserve]
    public sealed class DangerousUnmanagedArrayFormatter<T> : MemoryPackFormatter<T[]>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref T[]? value)
        {
            writer.DangerousWriteUnmanagedArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref T[]? value)
        {
            reader.DangerousReadUnmanagedArray<T>(ref value);
        }
    }

    [Preserve]
    public sealed class ArrayFormatter<T> : MemoryPackFormatter<T?[]>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref T?[]? value)
        {
            writer.WriteArray(value);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref T?[]? value)
        {
            reader.ReadArray(ref value);
        }
    }

    [Preserve]
    public sealed class ArraySegmentFormatter<T> : MemoryPackFormatter<ArraySegment<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ArraySegment<T?> value)
        {
            writer.WriteSpan(value.AsMemory().Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ArraySegment<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : (ArraySegment<T?>)array;
        }
    }

    [Preserve]
    public sealed class MemoryFormatter<T> : MemoryPackFormatter<Memory<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Memory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Memory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    [Preserve]
    public sealed class ReadOnlyMemoryFormatter<T> : MemoryPackFormatter<ReadOnlyMemory<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ReadOnlyMemory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ReadOnlyMemory<T?> value)
        {
            value = reader.ReadArray<T>();
        }
    }

    [Preserve]
    public sealed class ReadOnlySequenceFormatter<T> : MemoryPackFormatter<ReadOnlySequence<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ReadOnlySequence<T?> value)
        {
            if (value.IsSingleSegment)
            {
                writer.WriteSpan(value.FirstSpan);
                return;
            }

            writer.WriteCollectionHeader(checked((int)value.Length));
            foreach (var memory in value)
            {
                writer.WriteSpanWithoutLengthHeader(memory.Span);
            }
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ReadOnlySequence<T?> value)
        {
            var array = reader.ReadArray<T>();
            value = (array == null) ? default : new ReadOnlySequence<T?>(array);
        }
    }

    [Preserve]
    public sealed class MemoryPoolFormatter<T> : MemoryPackFormatter<Memory<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref Memory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref Memory<T?> value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = Memory<T?>.Empty;
                return;
            }

            var memory = ArrayPool<T?>.Shared.Rent(length).AsMemory(0, length);
            var span = memory.Span;
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
            value = memory;
        }
    }

    [Preserve]
    public sealed class ReadOnlyMemoryPoolFormatter<T> : MemoryPackFormatter<ReadOnlyMemory<T?>>
    {
        [Preserve]
        public override void Serialize(ref MemoryPackWriter writer, ref ReadOnlyMemory<T?> value)
        {
            writer.WriteSpan(value.Span);
        }

        [Preserve]
        public override void Deserialize(ref MemoryPackReader reader, ref ReadOnlyMemory<T?> value)
        {
            if (!reader.TryReadCollectionHeader(out var length))
            {
                value = null;
                return;
            }

            if (length == 0)
            {
                value = Memory<T?>.Empty;
                return;
            }

            var memory = ArrayPool<T?>.Shared.Rent(length).AsMemory(0, length);
            var span = memory.Span;
            reader.ReadSpanWithoutReadLengthHeader(length, ref span);
            value = memory;
        }
    }
}
