using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

#nullable enable
using System.Collections.Concurrent;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace MemoryPack {

public static class MemoryPackWriterOptionalStatePool
{
    static readonly ConcurrentQueue<MemoryPackWriterOptionalState> queue = new ConcurrentQueue<MemoryPackWriterOptionalState>();

    public static MemoryPackWriterOptionalState Rent(MemoryPackSerializerOptions? options)
    {
        if (!queue.TryDequeue(out var state))
        {
            state = new MemoryPackWriterOptionalState();
        }

        state.Init(options);
        return state;
    }

    internal static void Return(MemoryPackWriterOptionalState state)
    {
        state.Reset();
        queue.Enqueue(state);
    }
}

public sealed class MemoryPackWriterOptionalState : IDisposable
{
    internal static readonly MemoryPackWriterOptionalState NullState = new MemoryPackWriterOptionalState(true);

    uint nextId;
    readonly Dictionary<object, uint> objectToRef;

    public MemoryPackSerializerOptions Options { get; private set; }

    internal MemoryPackWriterOptionalState()
    {
        objectToRef = new Dictionary<object, uint>(ReferenceEqualityComparer.Instance);
        Options = null!;
        nextId = 0;
    }

    MemoryPackWriterOptionalState(bool _)
    {
        objectToRef = null!;
        Options = MemoryPackSerializerOptions.Default;
        nextId = 0;
    }

    internal void Init(MemoryPackSerializerOptions? options)
    {
        Options = options ?? MemoryPackSerializerOptions.Default;
    }

    public void Reset()
    {
        objectToRef.Clear();
        Options = null!;
        nextId = 0;
    }

    public (bool existsReference, uint id) GetOrAddReference(object value)
    {
#if NET7_0_OR_GREATER
        ref var id = ref CollectionsMarshal.GetValueRefOrAddDefault(objectToRef, value, out var exists);
        if (exists)
        {
            return (true, id);
        }
        else
        {
            id = nextId++;
            return (false, id);
        }
#else
        if (objectToRef.TryGetValue(value, out var id))
        {
            return (true, id);
        }
        else
        {
            id = nextId++;
            objectToRef.Add(value, id);
            return (false, id);
        }
#endif
    }

    void IDisposable.Dispose()
    {
        MemoryPackWriterOptionalStatePool.Return(this);
    }

    // ReferenceEqualityComparer is exsits in .NET 6 but NetStandard 2.1 does not.
    sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        ReferenceEqualityComparer() { }

        public static ReferenceEqualityComparer Instance { get; } = new ReferenceEqualityComparer();

        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);

        public int GetHashCode(object obj)
        {
            return RuntimeHelpers.GetHashCode(obj);
        }
    }
}

}