using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace NativeCollection
{
    public unsafe class List<T>: ICollection<T>, INativeCollectionClass where T:unmanaged, IEquatable<T>
{
    private UnsafeType.List<T>* _list;
    private const int _defaultCapacity = 10;
    private int _capacity;
    public List(int capacity = _defaultCapacity)
    {
        _capacity = capacity;
        _list = UnsafeType.List<T>.Create(_capacity);
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
    public UnsafeType.List<T>.Enumerator GetEnumerator()
    {
        return new UnsafeType.List<T>.Enumerator(_list);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T item)
    {
        _list->Add(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        _list->Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        return _list->Contains(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        _list->CopyTo(array,arrayIndex);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        return _list->Remove(item);
    }
    
    public int Capacity
    {
        get => _list->Capacity;
        set => _list->Capacity = value;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(in T item)
    {
        return _list->IndexOf(item);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        _list->RemoveAt(index);
    }

    public ref T this[int index] => ref (*_list)[index];

    public int Count => _list->Count;
    public bool IsReadOnly => false;
    
    public void Dispose()
    {
        if (IsDisposed)
        {
            return;
        }
        if (_list!=null)
        {
            _list->Dispose();
            NativeMemoryHelper.Free(_list);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.List<T>>());
            IsDisposed = true;
        }
    }
    
    public void ReInit()
    {
        if (IsDisposed)
        {
            _list = UnsafeType.List<T>.Create(_capacity);
            IsDisposed = false;
        }
    }

    public bool IsDisposed { get; private set; }

    ~List()
    {
        Dispose();
    }
}
}


