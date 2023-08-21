using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using NativeCollection.UnsafeType;

namespace NativeCollection
{
    public unsafe class MultiMap<T, K> : IEnumerable<MultiMapPair<T, K>>, INativeCollectionClass
    where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
{
    private UnsafeType.MultiMap<T, K>* _multiMap;

    private const int _defaultPoolSize = 200;

    private int _poolSize;
    
    public MultiMap(int maxPoolSize = _defaultPoolSize)
    {
        _poolSize = maxPoolSize;
        _multiMap = UnsafeType.MultiMap<T, K>.Create(_poolSize);
        IsDisposed = false;
    }
    
    public Span<K> this[T key] => (*_multiMap)[key];

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T key, K value)
    {
        _multiMap->Add(key,value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T key, K value)
    {
        return _multiMap->Remove(key, value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T key)
    {
        return _multiMap->Remove(key);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _multiMap->Clear();
    }

    public int Count => _multiMap->Count;
    
    IEnumerator<MultiMapPair<T, K>> IEnumerable<MultiMapPair<T, K>>. GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeType.SortedSet<MultiMapPair<T, K>>.Enumerator GetEnumerator()
    {
        return _multiMap->GetEnumerator();
    }

    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        if (_multiMap!=null)
        {
            _multiMap->Dispose();
            NativeMemoryHelper.Free(_multiMap);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.MultiMap<T,K>>());
            IsDisposed = true;
        }
    }

    public void ReInit()
    {
        if (IsDisposed)
        {
            _multiMap = UnsafeType.MultiMap<T, K>.Create(_poolSize);
            IsDisposed = false;
        }
    }

    public bool IsDisposed { get; private set; }
    
    ~MultiMap()
    {
        Dispose();
    }
}
}

