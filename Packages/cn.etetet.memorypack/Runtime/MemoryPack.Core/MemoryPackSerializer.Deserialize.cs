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
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack {

public static partial class MemoryPackSerializer
{
    [ThreadStatic]
    static MemoryPackReaderOptionalState? threadStaticReaderOptionalState;

    public static T? Deserialize<T>(ReadOnlySpan<byte> buffer, MemoryPackSerializerOptions? options = default)
    {
        T? value = default;
        Deserialize(buffer, ref value, options);
        return value;
    }

    public static int Deserialize<T>(ReadOnlySpan<byte> buffer, ref T? value, MemoryPackSerializerOptions? options = default)
    {
        if (!RuntimeHelpers.IsReferenceOrContainsReferences<T>())
        {
            if (buffer.Length < Unsafe.SizeOf<T>())
            {
                MemoryPackSerializationException.ThrowInvalidRange(Unsafe.SizeOf<T>(), buffer.Length);
            }
            value = Unsafe.ReadUnaligned<T>(ref MemoryMarshal.GetReference(buffer));
            return Unsafe.SizeOf<T>();
        }

        var state = threadStaticReaderOptionalState;
        if (state == null)
        {
            state = threadStaticReaderOptionalState = new MemoryPackReaderOptionalState();
        }
        state.Init(options);

        var reader = new MemoryPackReader(buffer, state);
        try
        {
            reader.ReadValue(ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static T? Deserialize<T>(in ReadOnlySequence<byte> buffer, MemoryPackSerializerOptions? options = default)
    {
        T? value = default;
        Deserialize<T>(buffer, ref value);
        return value;
    }

    public static int Deserialize<T>(in ReadOnlySequence<byte> buffer, ref T? value, MemoryPackSerializerOptions? options = default)
    {
        var state = threadStaticReaderOptionalState;
        if (state == null)
        {
            state = threadStaticReaderOptionalState = new MemoryPackReaderOptionalState();
        }
        state.Init(options);

        var reader = new MemoryPackReader(buffer, state);
        try
        {
            reader.ReadValue(ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static async ValueTask<T?> DeserializeAsync<T>(Stream stream, MemoryPackSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            T? value = default;
            var bytesRead = Deserialize<T>(streamBuffer.AsSpan(checked((int)ms.Position)), ref value, options);

            // Emulate that we had actually "read" from the stream.
            ms.Seek(bytesRead, SeekOrigin.Current);

            return value;
        }

        var builder = ReusableReadOnlySequenceBuilderPool.Rent();
        try
        {
            var buffer = ArrayPool<byte>.Shared.Rent(65536); // initial 64K
            var offset = 0;
            do
            {
                if (offset == buffer.Length)
                {
                    builder.Add(buffer, returnToPool: true);
                    buffer = ArrayPool<byte>.Shared.Rent(MathEx.NewArrayCapacity(buffer.Length));
                    offset = 0;
                }

                int read = 0;
                try
                {
                    read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken).ConfigureAwait(false);
                }
                catch
                {
                    // buffer is not added in builder, so return here.
                    ArrayPool<byte>.Shared.Return(buffer);
                    throw;
                }

                offset += read;

                if (read == 0)
                {
                    builder.Add(buffer.AsMemory(0, offset), returnToPool: true);
                    break;

                }
            } while (true);

            // If single buffer, we can avoid ReadOnlySequence build cost.
            if (builder.TryGetSingleMemory(out var memory))
            {
                return Deserialize<T>(memory.Span, options);
            }
            else
            {
                var seq = builder.Build();
                var result = Deserialize<T>(seq, options);
                return result;
            }
        }
        finally
        {
            builder.Reset();
        }
    }
}

}