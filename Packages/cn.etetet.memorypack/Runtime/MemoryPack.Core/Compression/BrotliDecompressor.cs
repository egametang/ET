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
using System.Diagnostics;
using System.IO.Compression;

namespace MemoryPack.Compression {

public struct BrotliDecompressor : IDisposable
{
    ReusableReadOnlySequenceBuilder? sequenceBuilder;

    public ReadOnlySequence<byte> Decompress(ReadOnlySpan<byte> compressedSpan)
    {
        return Decompress(compressedSpan, out _);
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySpan<byte> compressedSpan, out int consumed)
    {
        if (sequenceBuilder != null)
        {
            MemoryPackSerializationException.ThrowAlreadyDecompressed();
        }

        sequenceBuilder = ReusableReadOnlySequenceBuilderPool.Rent();
        var decoder = new BrotliDecoder();
        try
        {
            var status = OperationStatus.DestinationTooSmall;
            DecompressCore(ref status, ref decoder, compressedSpan, out consumed);
            if (status == OperationStatus.NeedMoreData)
            {
                MemoryPackSerializationException.ThrowCompressionFailed(status);
            }
        }
        finally
        {
            decoder.Dispose();
        }

        return sequenceBuilder.Build();
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> compressedSequence)
    {
        return Decompress(compressedSequence, out _);
    }

    public ReadOnlySequence<byte> Decompress(ReadOnlySequence<byte> compressedSequence, out int consumed)
    {
        if (sequenceBuilder != null)
        {
            MemoryPackSerializationException.ThrowAlreadyDecompressed();
        }

        sequenceBuilder = ReusableReadOnlySequenceBuilderPool.Rent();
        var decoder = new BrotliDecoder();
        try
        {
            var status = OperationStatus.DestinationTooSmall;
            consumed = 0;
            foreach (var item in compressedSequence)
            {
                DecompressCore(ref status, ref decoder, item.Span, out var bytesConsumed);
                consumed += bytesConsumed;
            }

            if (status == OperationStatus.NeedMoreData)
            {
                MemoryPackSerializationException.ThrowCompressionFailed(status);
            }
        }
        finally
        {
            decoder.Dispose();
        }

        return sequenceBuilder.Build();
    }

    void DecompressCore(ref OperationStatus status, ref BrotliDecoder decoder, ReadOnlySpan<byte> source, out int consumed)
    {
        Debug.Assert(sequenceBuilder != null);
        consumed = 0;

        byte[]? buffer = null;
        status = OperationStatus.DestinationTooSmall;
        var nextCapacity = source.Length;
        while (status == OperationStatus.DestinationTooSmall)
        {
            if (buffer == null)
            {
                nextCapacity = GetDoubleCapacity(nextCapacity);
                buffer = ArrayPool<byte>.Shared.Rent(nextCapacity);
            }

            status = decoder.Decompress(source, buffer, out var bytesConsumed, out var bytesWritten);
            consumed += bytesConsumed;

            if (status == OperationStatus.InvalidData)
            {
                MemoryPackSerializationException.ThrowCompressionFailed(status);
            }

            if (status == OperationStatus.NeedMoreData)
            {
                if (bytesWritten > 0)
                {
                    sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
                }
                if (bytesConsumed > 0)
                {
                    source = source.Slice(bytesConsumed);
                }
                if (source.Length != 0)
                {
                    // not consumed source fully
                    MemoryPackSerializationException.ThrowCompressionFailed();
                }

                // continue for next sequence. 
                return;
            }

            if (bytesConsumed > 0)
            {
                source = source.Slice(bytesConsumed);
            }
            if (bytesWritten > 0)
            {
                sequenceBuilder.Add(buffer.AsMemory(0, bytesWritten), true);
                buffer = null;
            }
        }
    }

    public void Dispose()
    {
        if (sequenceBuilder != null)
        {
            ReusableReadOnlySequenceBuilderPool.Return(sequenceBuilder);
            sequenceBuilder = null;
        }
    }

    int GetDoubleCapacity(int length)
    {
        var newCapacity = unchecked(length * 2);
        if ((uint)newCapacity > int.MaxValue) newCapacity = int.MaxValue;
        return Math.Max(newCapacity, 4096);
    }
}

}