using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NativeCollection.UnsafeType
{
    public unsafe struct Map<T,K> :IEnumerable<MapPair<T, K>>, IDisposable where T : unmanaged, IEquatable<T>, IComparable<T> where K : unmanaged, IEquatable<K>
{
    private UnsafeType.SortedSet<MapPair<T, K>>* _sortedSet;
    
    public static Map<T, K>* Create(int poolBlockSize)
    {
        Map<T, K>* map = (Map<T, K>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<Map<T, K>>());
        map->_sortedSet = UnsafeType.SortedSet<MapPair<T, K>>.Create(poolBlockSize);
        return map;
    }
    
    public K this[T key] {
        get
        {
            var pair = new MapPair<T, K>(key);
            var node = _sortedSet->FindNode(pair);
            if (node!=null)
            {
                return node->Item.Value;
            }
            return default;
        }
        set
        {
            var pair = new MapPair<T, K>(key,value);
            var node = _sortedSet->FindNode(pair);
            if (node!=null)
            {
                node->Item._value = value;
            }
            else
            {
                _sortedSet->Add(pair);
            }
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T key, K value)
    {
        var mapPair = new MapPair<T, K>(key,value);
        _sortedSet->Add(mapPair);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _sortedSet->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool ContainsKey(T key)
    {
        return _sortedSet->Contains(new MapPair<T, K>(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T key)
    {
        return _sortedSet->Remove(new MapPair<T, K>(key));
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryGetValue(T key, out K value)
    {
        var node = _sortedSet->FindNode(new MapPair<T, K>(key));
        if (node==null)
        {
            value = default;
            return false;
        }
        value = node->Item.Value;
        return true;
    }
    
    public int Count => _sortedSet->Count;
    
    IEnumerator<MapPair<T, K>> IEnumerable<MapPair<T, K>>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeType.SortedSet<MapPair<T, K>>.Enumerator GetEnumerator()
    {
        return new UnsafeType.SortedSet<MapPair<T, K>>.Enumerator(_sortedSet);
    }

    public void Dispose()
    {
        if (_sortedSet != null)
        {
            _sortedSet->Dispose();
            NativeMemoryHelper.Free(_sortedSet);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.SortedSet<MapPair<T, K>>>());
        }
    }
}
}

