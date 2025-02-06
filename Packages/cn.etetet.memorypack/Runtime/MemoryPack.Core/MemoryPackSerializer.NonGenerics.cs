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
using System.Runtime.InteropServices;

namespace MemoryPack {

public static partial class MemoryPackSerializer
{
    // Serialize

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] Serialize(Type type, object? value, MemoryPackSerializerOptions? options = default)
    {
        var state = threadStaticState;
        if (state == null)
        {
            state = threadStaticState = new SerializerWriterThreadStaticState();
        }
        state.Init(options);

        try
        {
            var writer = new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref state.BufferWriter), state.BufferWriter.DangerousGetFirstBuffer(), state.OptionalState);
            Serialize(type, ref writer, value);
            return state.BufferWriter.ToArrayAndReset();
        }
        finally
        {
            state.Reset();
        }
    }

    public static unsafe void Serialize(Type type, in IBufferWriter<byte> bufferWriter, object? value, MemoryPackSerializerOptions? options = default)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        var state = threadStaticWriterOptionalState;
        if (state == null)
        {
            state = threadStaticWriterOptionalState = new MemoryPackWriterOptionalState();
        }
        state.Init(options);

        try
        {
            var writer = new MemoryPackWriter(ref Unsafe.AsRef(bufferWriter), state);
            Serialize(type, ref writer, value);
        }
        finally
        {
            state.Reset();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Serialize(Type type, ref MemoryPackWriter writer, object? value)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        writer.GetFormatter(type).Serialize(ref writer, ref value);
        writer.Flush();
    }

    public static async ValueTask SerializeAsync(Type type, Stream stream, object? value, MemoryPackSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        var tempWriter = ReusableLinkedArrayBufferWriterPool.Rent();
        try
        {
            SerializeToTempWriter(tempWriter, type, value, options);
            await tempWriter.WriteToAndResetAsync(stream, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            ReusableLinkedArrayBufferWriterPool.Return(tempWriter);
        }
    }

    static void SerializeToTempWriter(ReusableLinkedArrayBufferWriter bufferWriter, Type type, object? value, MemoryPackSerializerOptions? options)
    {
        var state = threadStaticWriterOptionalState;
        if (state == null)
        {
            state = threadStaticWriterOptionalState = new MemoryPackWriterOptionalState();
        }
        state.Init(options);

        var writer = new MemoryPackWriter(ref Unsafe.As<ReusableLinkedArrayBufferWriter, IBufferWriter<byte>>(ref bufferWriter), state);

        try
        {
            Serialize(type, ref writer, value);
        }
        finally
        {
            state.Reset();
        }
    }

    // Deserialize

    public static object? Deserialize(Type type, ReadOnlySpan<byte> buffer, MemoryPackSerializerOptions? options = default)
    {
        object? value = default;
        Deserialize(type, buffer, ref value, options);
        return value;
    }

    public static int Deserialize(Type type, ReadOnlySpan<byte> buffer, ref object? value, MemoryPackSerializerOptions? options = default)
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
            reader.GetFormatter(type).Deserialize(ref reader, ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static object? Deserialize(Type type, in ReadOnlySequence<byte> buffer, MemoryPackSerializerOptions? options = default)
    {
        object? value = default;
        Deserialize(type, buffer, ref value, options);
        return value;
    }

    public static int Deserialize(Type type, in ReadOnlySequence<byte> buffer, ref object? value, MemoryPackSerializerOptions? options = default)
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
            reader.GetFormatter(type).Deserialize(ref reader, ref value);
            return reader.Consumed;
        }
        finally
        {
            reader.Dispose();
            state.Reset();
        }
    }

    public static async ValueTask<object?> DeserializeAsync(Type type, Stream stream, MemoryPackSerializerOptions? options = default, CancellationToken cancellationToken = default)
    {
        if (stream is MemoryStream ms && ms.TryGetBuffer(out ArraySegment<byte> streamBuffer))
        {
            cancellationToken.ThrowIfCancellationRequested();
            object? value = default;
            var bytesRead = Deserialize(type, streamBuffer.AsSpan(checked((int)ms.Position)), ref value, options);

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
                return Deserialize(type, memory.Span, options);
            }
            else
            {
                var seq = builder.Build();
                var result = Deserialize(type, seq, options);
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