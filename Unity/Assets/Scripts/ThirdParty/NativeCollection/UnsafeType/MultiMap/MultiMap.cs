using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NativeCollection.UnsafeType
{
    public unsafe struct MultiMap<T, K> : IEnumerable<MultiMapPair<T, K>>, IDisposable
    where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
{
    private UnsafeType.SortedSet<MultiMapPair<T, K>>* _sortedSet;

    private NativePool<List<K>>* _listPool;

    public static MultiMap<T, K>* Create(int maxPoolSize)
    {
        MultiMap<T, K>* multiMap = (MultiMap<T, K>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<MultiMap<T, K>>());
        multiMap->_sortedSet = UnsafeType.SortedSet<MultiMapPair<T, K>>.Create(maxPoolSize);
        multiMap->_listPool = NativePool<List<K>>.Create(maxPoolSize);
        return multiMap;
    }

    public Span<K> this[T key] {
        get
        {
            var list = new MultiMapPair<T, K>(key);
            var node = _sortedSet->FindNode(list);
            if (node!=null)
            {
                return node->Item.Value.WrittenSpan();
            }
            return Span<K>.Empty;
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(in T key,in K value)
    {
        var list = new MultiMapPair<T, K>(key);
        var node = _sortedSet->FindNode(list);
        if (node != null)
        {
            list = node->Item;
        }
        else
        {
            list = MultiMapPair<T, K>.Create(key,_listPool);
            _sortedSet->AddRef(list);
        }
        list.Value.AddRef(value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in T key,in K value)
    {
        var list = new MultiMapPair<T, K>(key);
        var node = _sortedSet->FindNode(list);

        if (node == null) return false;
        list = node->Item;
        if (!list.Value.RemoveRef(value)) return false;

        if (list.Value.Count == 0) Remove(key);

        return true;
    }
    
    

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(in T key)
    {
        var list = new MultiMapPair<T, K>(key);
        SortedSet<MultiMapPair<T, K>>.Node* node = _sortedSet->FindNode(list);

        if (node == null) return false;
        list = node->Item;
        var sortedSetRemove = _sortedSet->RemoveRef(list);
        list.Dispose(_listPool);
        return sortedSetRemove;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _sortedSet->Clear();
    }

    public int Count => _sortedSet->Count;
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator<MultiMapPair<T, K>> IEnumerable<MultiMapPair<T, K>>.GetEnumerator()
    {
        return _sortedSet->GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    IEnumerator IEnumerable.GetEnumerator()
    {
        return _sortedSet->GetEnumerator();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeType.SortedSet<MultiMapPair<T, K>>.Enumerator GetEnumerator()
    {
        return new UnsafeType.SortedSet<MultiMapPair<T, K>>.Enumerator(_sortedSet);
    }
    
    public void Dispose()
    {
        if (_sortedSet != null)
        {
            _sortedSet->Dispose();
            NativeMemoryHelper.Free(_sortedSet);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.SortedSet<MultiMapPair<T, K>>>());
        }

        if (_listPool!=null)
        {
            _listPool->Dispose();
            NativeMemoryHelper.Free(_listPool);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<NativePool<List<K>>>());
        }
    }
}
}

