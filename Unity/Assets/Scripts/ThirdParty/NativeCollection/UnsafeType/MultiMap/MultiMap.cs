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

    private FixedSizeMemoryPool* _listMemoryPool;

    private NativeStackPool<List<K>>* _listStackPool;

    public static MultiMap<T, K>* Create(int poolBlockSize,int listPoolSize)
    {
        MultiMap<T, K>* multiMap = (MultiMap<T, K>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<MultiMap<T, K>>());
        multiMap->_sortedSet = UnsafeType.SortedSet<MultiMapPair<T, K>>.Create(poolBlockSize);
        multiMap->_listMemoryPool = FixedSizeMemoryPool.Create(poolBlockSize,Unsafe.SizeOf<List<K>>());
        multiMap->_listStackPool = NativeStackPool<List<K>>.Create(listPoolSize);
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
            list = MultiMapPair<T, K>.Create(key,_listMemoryPool,_listStackPool);
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
        list.Dispose(_listMemoryPool,_listStackPool);
        return sortedSetRemove;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        using var enumerator = GetEnumerator();
        do
        {
            if (enumerator.CurrentPointer != null)
            {
                enumerator.CurrentPointer->Item.Dispose(_listMemoryPool,_listStackPool);
            }
        } while (enumerator.MoveNext());

        List<K>* list = _listStackPool->Alloc();
        while (list!=null)
        {
            list->Dispose();
            _listMemoryPool->Free(list);
            list = _listStackPool->Alloc();
        }
        _sortedSet->Clear();
        _listMemoryPool->ReleaseUnUsedSlabs();
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
            Clear();
            _sortedSet->Dispose();
            NativeMemoryHelper.Free(_sortedSet);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.SortedSet<MultiMapPair<T, K>>>());
        }
        
        if (_listStackPool!=null)
        {
            _listStackPool->Dispose();
            NativeMemoryHelper.Free(_listStackPool);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<NativeStackPool<List<K>>>());
        }

        if (_listMemoryPool!=null)
        {
            _listMemoryPool->Dispose();
            _listMemoryPool = null;
        }
    }
}
}

