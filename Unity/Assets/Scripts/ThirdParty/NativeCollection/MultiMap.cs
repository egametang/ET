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

    private const int _defaultPoolBlockSize = 64;

    private const int _defaultListPoolSize = 200;

    private int _poolBlockSize;

    private int _listPoolSize;
    
    public MultiMap(int listPoolSize = _defaultListPoolSize,int nodePoolBlockSize = _defaultPoolBlockSize)
    {
        _poolBlockSize = nodePoolBlockSize;
        _listPoolSize = listPoolSize;
        _multiMap = UnsafeType.MultiMap<T, K>.Create(_poolBlockSize,_listPoolSize);
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
            _multiMap = UnsafeType.MultiMap<T, K>.Create(_poolBlockSize,_listPoolSize);
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

