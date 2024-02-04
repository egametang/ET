using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NativeCollection
{
    public unsafe class SortedSet<T> : ICollection<T>, INativeCollectionClass where T : unmanaged, IEquatable<T>,IComparable<T>
{
    private UnsafeType.SortedSet<T>* _sortedSet;
    private const int _defaultNodePoolBlockSize = 64;
    private int _poolBlockSize;
    public SortedSet(int nodePoolSize = _defaultNodePoolBlockSize)
    {
        _poolBlockSize = nodePoolSize;
        _sortedSet = UnsafeType.SortedSet<T>.Create(_poolBlockSize);
        IsDisposed = false;
    }
    IEnumerator<T> IEnumerable<T>.GetEnumerator()
    {
        return GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public UnsafeType.SortedSet<T>.Enumerator GetEnumerator()
    {
        return new UnsafeType.SortedSet<T>.Enumerator(_sortedSet);
    }

    void ICollection<T>.Add(T item)
    {
        _sortedSet->Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Add(T item)
    {
        return _sortedSet->Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _sortedSet->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        return _sortedSet->Contains(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        _sortedSet->CopyTo(array,arrayIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        return _sortedSet->Remove(item);
    }
    
    public T? Min => _sortedSet->Min;
    
    public T? Max => _sortedSet->Max;

    public int Count => _sortedSet->Count;
    public bool IsReadOnly => false;
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        if (_sortedSet!=null)
        {
            _sortedSet->Dispose();
            NativeMemoryHelper.Free(_sortedSet);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.SortedSet<T>>());
            IsDisposed = true;
        }
    }

    public void ReInit()
    {
        if (IsDisposed)
        {
            _sortedSet = UnsafeType.SortedSet<T>.Create(_poolBlockSize);
            IsDisposed = false;
        }
    }

    public bool IsDisposed { get; private set; }
}
}

