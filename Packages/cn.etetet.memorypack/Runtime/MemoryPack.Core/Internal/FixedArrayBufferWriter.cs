using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;
using System.Runtime.CompilerServices;

namespace MemoryPack.Internal {

internal struct FixedArrayBufferWriter : IBufferWriter<byte>
{
    byte[] buffer;
    int written;

    public FixedArrayBufferWriter(byte[] buffer)
    {
        this.buffer = buffer;
        this.written = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Advance(int count)
    {
        this.written += count;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Memory<byte> GetMemory(int sizeHint = 0)
    {
        var memory = buffer.AsMemory(written);
        if (memory.Length >= sizeHint)
        {
            return memory;
        }

        MemoryPackSerializationException.ThrowMessage("Requested invalid sizeHint.");
        return memory;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<byte> GetSpan(int sizeHint = 0)
    {
        var span = buffer.AsSpan(written);
        if (span.Length >= sizeHint)
        {
            return span;
        }

        MemoryPackSerializationException.ThrowMessage("Requested invalid sizeHint.");
        return span;
    }

    public byte[] GetFilledBuffer()
    {
        if (written != buffer.Length)
        {
            MemoryPackSerializationException.ThrowMessage("Not filled buffer.");
        }

        return buffer;
    }
}

}