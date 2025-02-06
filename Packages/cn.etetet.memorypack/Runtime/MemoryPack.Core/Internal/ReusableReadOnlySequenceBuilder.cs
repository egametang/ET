using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Buffers;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace MemoryPack.Internal {

internal static class ReusableReadOnlySequenceBuilderPool
{
    static readonly ConcurrentQueue<ReusableReadOnlySequenceBuilder> queue = new();

    public static ReusableReadOnlySequenceBuilder Rent()
    {
        if (queue.TryDequeue(out var builder))
        {
            return builder;
        }
        return new ReusableReadOnlySequenceBuilder();
    }

    public static void Return(ReusableReadOnlySequenceBuilder builder)
    {
        builder.Reset();
        queue.Enqueue(builder);
    }
}

internal sealed class ReusableReadOnlySequenceBuilder
{
    readonly Stack<Segment> segmentPool;
    readonly List<Segment> list;

    public ReusableReadOnlySequenceBuilder()
    {
        list = new();
        segmentPool = new Stack<Segment>();
    }

    public void Add(ReadOnlyMemory<byte> buffer, bool returnToPool)
    {
        if (!segmentPool.TryPop(out var segment))
        {
            segment = new Segment();
        }

        segment.SetBuffer(buffer, returnToPool);
        list.Add(segment);
    }

    public bool TryGetSingleMemory(out ReadOnlyMemory<byte> memory)
    {
        if (list.Count == 1)
        {
            memory = list[0].Memory;
            return true;
        }
        memory = default;
        return false;
    }

    public ReadOnlySequence<byte> Build()
    {
        if (list.Count == 0)
        {
            return ReadOnlySequence<byte>.Empty;
        }

        if (list.Count == 1)
        {
            return new ReadOnlySequence<byte>(list[0].Memory);
        }

        long running = 0;
#if NET7_0_OR_GREATER
        var span = CollectionsMarshal.AsSpan(list);
        for (int i = 0; i < span.Length; i++)
        {
            var next = i < span.Length - 1 ? span[i + 1] : null;
            span[i].SetRunningIndexAndNext(running, next);
            running += span[i].Memory.Length;
        }
        var firstSegment = span[0];
        var lastSegment = span[span.Length - 1];
#else
        var span = list;
        for (int i = 0; i < span.Count; i++)
        {
            var next = i < span.Count - 1 ? span[i + 1] : null;
            span[i].SetRunningIndexAndNext(running, next);
            running += span[i].Memory.Length;
        }
        var firstSegment = span[0];
        var lastSegment = span[span.Count - 1];
#endif
        return new ReadOnlySequence<byte>(firstSegment, 0, lastSegment, lastSegment.Memory.Length);
    }

    public void Reset()
    {
#if NET7_0_OR_GREATER
        var span = CollectionsMarshal.AsSpan(list);
#else
        var span = list;
#endif
        foreach (var item in span)
        {
            item.Reset();
            segmentPool.Push(item);
        }
        list.Clear();
    }

    class Segment : ReadOnlySequenceSegment<byte>
    {
        bool returnToPool;

        public Segment()
        {
            returnToPool = false;
        }

        public void SetBuffer(ReadOnlyMemory<byte> buffer, bool returnToPool)
        {
            Memory = buffer;
            this.returnToPool = returnToPool;
        }

        public void Reset()
        {
            if (returnToPool)
            {
                if (MemoryMarshal.TryGetArray(Memory, out var segment) && segment.Array != null)
                {
                    ArrayPool<byte>.Shared.Return(segment.Array, clearArray: false);
                }
            }
            Memory = default;
            RunningIndex = 0;
            Next = null;
        }

        public void SetRunningIndexAndNext(long runningIndex, Segment? nextSegment)
        {
            RunningIndex = runningIndex;
            Next = nextSegment;
        }
    }
}

}