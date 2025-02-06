using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Collections.Concurrent;

namespace MemoryPack {

public static class MemoryPackReaderOptionalStatePool
{
    static readonly ConcurrentQueue<MemoryPackReaderOptionalState> queue = new ConcurrentQueue<MemoryPackReaderOptionalState>();

    public static MemoryPackReaderOptionalState Rent(MemoryPackSerializerOptions? options)
    {
        if (!queue.TryDequeue(out var state))
        {
            state = new MemoryPackReaderOptionalState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(MemoryPackReaderOptionalState state)
    {
        state.Reset();
        queue.Enqueue(state);
    }
}

public sealed class MemoryPackReaderOptionalState : IDisposable
{
    readonly Dictionary<uint, object> refToObject;
    public MemoryPackSerializerOptions Options { get; private set; }

    internal MemoryPackReaderOptionalState()
    {
        refToObject = new Dictionary<uint, object>();
        Options = null!;
    }

    internal void Init(MemoryPackSerializerOptions? options)
    {
        Options = options ?? MemoryPackSerializerOptions.Default;
    }

    public object GetObjectReference(uint id)
    {
        if (refToObject.TryGetValue(id, out var value))
        {
            return value;
        }
        MemoryPackSerializationException.ThrowMessage("Object is not found in this reference id:" + id);
        return null!;
    }

    public void AddObjectReference(uint id, object value)
    {
        if (!refToObject.TryAdd(id, value))
        {
            MemoryPackSerializationException.ThrowMessage("Object is already added, id:" + id);
        }
    }

    public void Reset()
    {
        refToObject.Clear();
        Options = null!;
    }

    void IDisposable.Dispose()
    {
        MemoryPackReaderOptionalStatePool.Return(this);
    }
}

}