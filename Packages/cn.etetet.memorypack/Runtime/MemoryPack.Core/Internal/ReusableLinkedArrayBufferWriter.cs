using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;
using System.Collections;
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace MemoryPack.Internal {

#if NET7_0_OR_GREATER
using static GC;
using static MemoryMarshal;
#else
using static MemoryPack.Internal.MemoryMarshalEx;
#endif

// internal but used by generator code

public static class ReusableLinkedArrayBufferWriterPool
{
    static readonly ConcurrentQueue<ReusableLinkedArrayBufferWriter> queue = new ConcurrentQueue<ReusableLinkedArrayBufferWriter>();

    public static ReusableLinkedArrayBufferWriter Rent()
    {
        if (queue.TryDequeue(out var writer))
        {
            return writer;
        }
        return new ReusableLinkedArrayBufferWriter(useFirstBuffer: false, pinned: false); // does not cache firstBuffer
    }

    public static void Return(ReusableLinkedArrayBufferWriter writer)
    {
        writer.Reset();
        queue.Enqueue(writer);
    }
}

// This class has large buffer so should cache [ThreadStatic] or Pool.
public sealed class ReusableLinkedArrayBufferWriter : IBufferWriter<byte>
{
    const int InitialBufferSize = 262144; // 256K(32768, 65536, 131072, 262144)
    static readonly byte[] noUseFirstBufferSentinel = new byte[0];

    List<BufferSegment> buffers; // add freezed buffer.

    byte[] firstBuffer; // cache firstBuffer to avoid call ArrayPoo.Rent/Return
    int firstBufferWritten;

    BufferSegment current;
    int nextBufferSize;

    int totalWritten;

    public int TotalWritten => totalWritten;
    bool UseFirstBuffer => firstBuffer != noUseFirstBufferSentinel;

    public ReusableLinkedArrayBufferWriter(bool useFirstBuffer, bool pinned)
    {
        this.buffers = new List<BufferSegment>();
        this.firstBuffer = useFirstBuffer
            ? AllocateUninitializedArray<byte>(InitialBufferSize, pinned)
            : noUseFirstBufferSentinel;
        this.firstBufferWritten = 0;
        this.current = default;
        this.nextBufferSize = InitialBufferSize;
        this.totalWritten = 0;
    }

    public byte[] DangerousGetFirstBuffer() => firstBuffer;

    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        // MemoryPack don't use GetMemory.
        throw new NotSupportedException();
    }

    public Span<byte> GetSpan(int sizeHint = 0)
    {
        if (current.IsNull)
        {
            // use firstBuffer
            var free = firstBuffer.Length - firstBufferWritten;
            if (free != 0 && sizeHint <= free)
            {
                return firstBuffer.AsSpan(firstBufferWritten);
            }
        }
        else
        {
            var buffer = current.FreeBuffer;
            if (buffer.Length > sizeHint)
            {
                return buffer;
            }
        }

        BufferSegment next;
        if (sizeHint <= nextBufferSize)
        {
            next = new BufferSegment(nextBufferSize);
            nextBufferSize = MathEx.NewArrayCapacity(nextBufferSize);
        }
        else
        {
            next = new BufferSegment(sizeHint);
        }

        if (current.WrittenCount != 0)
        {
            buffers.Add(current);
        }
        current = next;
        return next.FreeBuffer;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        if (current.IsNull)
        {
            firstBufferWritten += count;
        }
        else
        {
            current.Advance(count);
        }
        totalWritten += count;
    }

    public byte[] ToArrayAndReset()
    {
        if (totalWritten == 0) return Array.Empty<byte>();

        var result = AllocateUninitializedArray<byte>(totalWritten);
        var dest = result.AsSpan();

        if (UseFirstBuffer)
        {
            firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(dest);
            dest = dest.Slice(firstBufferWritten);
        }

        if (buffers.Count > 0)
        {
#if NET7_0_OR_GREATER
            foreach (ref var item in CollectionsMarshal.AsSpan(buffers))
#else
            foreach (var item in buffers)
#endif
            {
                item.WrittenBuffer.CopyTo(dest);
                dest = dest.Slice(item.WrittenCount);
                item.Clear(); // reset buffer-segment in this loop to avoid iterate twice for Reset
            }
        }

        if (!current.IsNull)
        {
            current.WrittenBuffer.CopyTo(dest);
            current.Clear();
        }

        ResetCore();
        return result;
    }

    public void WriteToAndReset(ref MemoryPackWriter writer)
#if NET7_0_OR_GREATER
        
#else
        
#endif
    {
        if (totalWritten == 0) return;

        if (UseFirstBuffer)
        {
            ref var spanRef = ref writer.GetSpanReference(firstBufferWritten);
            firstBuffer.AsSpan(0, firstBufferWritten).CopyTo(MemoryMarshal.CreateSpan(ref spanRef, firstBufferWritten));
            writer.Advance(firstBufferWritten);
        }

        if (buffers.Count > 0)
        {
#if NET7_0_OR_GREATER
            foreach (ref var item in CollectionsMarshal.AsSpan(buffers))
#else
            foreach (var item in buffers)
#endif
            {
                ref var spanRef = ref writer.GetSpanReference(item.WrittenCount);
                item.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, item.WrittenCount));
                writer.Advance(item.WrittenCount);
                item.Clear(); // reset
            }
        }

        if (!current.IsNull)
        {
            ref var spanRef = ref writer.GetSpanReference(current.WrittenCount);
            current.WrittenBuffer.CopyTo(MemoryMarshal.CreateSpan(ref spanRef, current.WrittenCount));
            writer.Advance(current.WrittenCount);
            current.Clear();
        }

        ResetCore();
    }

    public async ValueTask WriteToAndResetAsync(Stream stream, CancellationToken cancellationToken)
    {
        if (totalWritten == 0) return;

        if (UseFirstBuffer)
        {
            await stream.WriteAsync(firstBuffer.AsMemory(0, firstBufferWritten), cancellationToken).ConfigureAwait(false);
        }

        if (buffers.Count > 0)
        {
            foreach (var item in buffers)
            {
                await stream.WriteAsync(item.WrittenMemory, cancellationToken).ConfigureAwait(false);
                item.Clear(); // reset
            }
        }

        if (!current.IsNull)
        {
            await stream.WriteAsync(current.WrittenMemory, cancellationToken).ConfigureAwait(false);
            current.Clear();
        }

        ResetCore();
    }

    public Enumerator GetEnumerator()
    {
        return new Enumerator(this);
    }

    // reset without list's BufferSegment element
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    void ResetCore()
    {
        firstBufferWritten = 0;
        buffers.Clear();
        totalWritten = 0;
        current = default;
        nextBufferSize = InitialBufferSize;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset()
    {
        if (totalWritten == 0) return;
#if NET7_0_OR_GREATER
        foreach (ref var item in CollectionsMarshal.AsSpan(buffers))
#else
        foreach (var item in buffers)
#endif
        {
            item.Clear();
        }
        current.Clear();
        ResetCore();
    }

    public struct Enumerator : IEnumerator<Memory<byte>>
    {
        ReusableLinkedArrayBufferWriter parent;
        State state;
        Memory<byte> current;
        List<BufferSegment>.Enumerator buffersEnumerator;

        public Enumerator(ReusableLinkedArrayBufferWriter parent)
        {
            this.parent = parent;
            this.state = default;
            this.current = default;
            this.buffersEnumerator = default;
        }

        public Memory<byte> Current => current;

        object IEnumerator.Current => throw new NotSupportedException();

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            if (state == State.FirstBuffer)
            {
                state = State.BuffersInit;

                if (parent.UseFirstBuffer)
                {
                    current = parent.firstBuffer.AsMemory(0, parent.firstBufferWritten);
                    return true;
                }
            }

            if (state == State.BuffersInit)
            {
                state = State.BuffersIterate;

                buffersEnumerator = parent.buffers.GetEnumerator();
            }

            if (state == State.BuffersIterate)
            {
                if (buffersEnumerator.MoveNext())
                {
                    current = buffersEnumerator.Current.WrittenMemory;
                    return true;
                }

                buffersEnumerator.Dispose();
                state = State.Current;
            }

            if (state == State.Current)
            {
                state = State.End;

                current = parent.current.WrittenMemory;
                return true;
            }

            return false;
        }

        public void Reset()
        {
            throw new NotSupportedException();
        }

        enum State
        {
            FirstBuffer,
            BuffersInit,
            BuffersIterate,
            Current,
            End
        }
    }
}

internal struct BufferSegment
{
    byte[] buffer;
    int written;

    public bool IsNull => buffer == null;

    public int WrittenCount => written;
    public Span<byte> WrittenBuffer => buffer.AsSpan(0, written);
    public Memory<byte> WrittenMemory => buffer.AsMemory(0, written);
    public Span<byte> FreeBuffer => buffer.AsSpan(written);

    public BufferSegment(int size)
    {
        buffer = ArrayPool<byte>.Shared.Rent(size);
        written = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        written += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (buffer != null)
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
        buffer = null!;
        written = 0;
    }
}

}