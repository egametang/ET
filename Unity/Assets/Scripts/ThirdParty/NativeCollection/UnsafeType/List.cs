using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;

namespace NativeCollection.UnsafeType
{
    public unsafe struct List<T> : ICollection<T>, IDisposable, IPool where T : unmanaged, IEquatable<T>
{
    private List<T>* self;

    private int _arrayLength;

    private T* _items;

    private const int _defaultCapacity = 4;

    public static List<T>* Create(int initialCapacity = _defaultCapacity)
    {
        if (initialCapacity < 0) ThrowHelper.ListInitialCapacityException();

        var list = (List<T>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<List<T>>());

        if (initialCapacity < _defaultCapacity)
            initialCapacity = _defaultCapacity; // Simplify doubling logic in Push.

        list->_items = (T*)NativeMemoryHelper.Alloc((UIntPtr)initialCapacity, (UIntPtr)Unsafe.SizeOf<T>());
        list->_arrayLength = initialCapacity;
        list->Count = 0;
        list->self = list;
        return list;
    }

    public static List<T>* AllocFromMemoryPool(FixedSizeMemoryPool* memoryPool,int initialCapacity = _defaultCapacity)
    {
        if (initialCapacity < 0) ThrowHelper.ListInitialCapacityException();

        var list = (List<T>*)memoryPool->Alloc();

        if (initialCapacity < _defaultCapacity)
            initialCapacity = _defaultCapacity; // Simplify doubling logic in Push.

        list->_items = (T*)NativeMemoryHelper.Alloc((UIntPtr)initialCapacity, (UIntPtr)Unsafe.SizeOf<T>());
        list->_arrayLength = initialCapacity;
        list->Count = 0;
        list->self = list;
        return list;
    }

    
    public ref T this[int index]
    {
        get
        {
            if (index>=Count)
            {
                ThrowHelper.IndexMustBeLessException();
            }
            return ref *(_items + index);
        }
    }

    public int Capacity
    {
        get => _arrayLength;
        set
        {
            if (value < Count) ThrowHelper.ListSmallCapacity();

            if (value != _arrayLength)
            {
                if (value > 0)
                {
                    var newArray = (T*)NativeMemoryHelper.Alloc((UIntPtr)value, (UIntPtr)Unsafe.SizeOf<T>());
                    if (Count > 0)
                        Unsafe.CopyBlockUnaligned(newArray, _items, (uint)(_arrayLength * Unsafe.SizeOf<T>()));
                    NativeMemoryHelper.Free(_items);
                    NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<T>() * _arrayLength);
                    _items = newArray;
                    _arrayLength = value;
                }
                else
                {
                    ThrowHelper.ListSmallCapacity();
                }
            }
        }
    }

    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Add(T value)
    {
        AddRef(value);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void AddRef(T value)
    {
        var array = _items;
        var size = Count;
        if ((uint)size < (uint)_arrayLength)
        {
            Count = size + 1;
            array[size] = value;
        }
        else
        {
            AddWithResize(value);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void CopyTo(T[] array, int arrayIndex)
    {
        throw new NotImplementedException();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Remove(T item)
    {
        return RemoveRef(item);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool RemoveRef(in T item)
    {
        var index = IndexOf(item);
        //Console.WriteLine($"index: {index}");
        if (index >= 0)
        {
            RemoveAt(index);
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int IndexOf(in T item)
    {
        return new Span<T>(_items, Count).IndexOf(item);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RemoveAt(int index)
    {
        if ((uint)index >= (uint)Count) ThrowHelper.IndexMustBeLessException();
        Count--;
        if (index < Count)
            Unsafe.CopyBlockUnaligned(_items + index, _items + index + 1, (uint)((Count - index) * Unsafe.SizeOf<T>()));
    }


    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        Count = 0;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool Contains(T item)
    {
        return IndexOf(item) >= 0;
    }
    
    public void Insert(int index, T item)
    {
        // Note that insertions at the end are legal.
        if ((uint)index > (uint)Count)
        {
            ThrowHelper.IndexMustBeLessException();
        }
        if (Count == _arrayLength) Grow(Count + 1);
        if (index < Count)
        {
            Unsafe.CopyBlockUnaligned(_items+index+1,_items+index,(uint)(Count-index));
        }
        _items[index] = item;
        Count++;
    }

    public void AddRange(Span<T> collection)
    {
        InsertRange(Count, collection);
    }
    
    public void InsertRange(int index, Span<T> collection)
    {
        
        if ((uint)index > (uint)Count)
        {
            ThrowHelper.ListIndexOutOfRange();
        }
        
        int count = collection.Length;
        if (count > 0)
        {
            if (_arrayLength - Count < count)
            {
                Grow(Count + count);
            }
            if (index < Count)
            {
                Unsafe.CopyBlockUnaligned(_items+index+count,_items+index,(uint)(Count-index));
            }
            
            collection.CopyTo(new Span<T>(_items,index));
            Count += count;
        }
        
    }

    public void RemoveRange(int index, int count)
    {
        if (index < 0)
        {
            ThrowHelper.ListIndexOutOfRange();
        }

        if (count < 0)
        {
            ThrowHelper.ListIndexOutOfRange();
        }

        if (Count - index < count)
            ThrowHelper.ListIndexOutOfRange();

        if (count > 0)
        {
            Count -= count;
            if (index < Count)
            {
                Unsafe.CopyBlockUnaligned(_items+index,_items+index+count,(uint)(Count-index));
            }
        }
    }
    

    public void FillDefaultValue()
    {
        for (int i = 0; i < _arrayLength; i++)
        {
            _items[i] = default;
        }
        Count = _arrayLength;
    }

    public void ResizeWithDefaultValue(int newSize)
    {
        var size = _arrayLength;
        Capacity = newSize;
        for (int i = size; i < _arrayLength; i++)
        {
            _items[i] = default;
        }
    }

    public int Count { get; private set; }

    public bool IsReadOnly => false;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddWithResize(in T item)
    {
        var size = Count;
        Grow(size + 1);
        Count = size + 1;
        _items[size] = item;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int capacity)
    {
        Debug.Assert(_arrayLength < capacity);

        var newcapacity = _arrayLength == 0 ? _defaultCapacity : 2 * Count;

        // Allow the list to grow to maximum possible capacity (~2G elements) before encountering overflow.
        // Note that this check works even when _items.Length overflowed thanks to the (uint) cast
        if ((uint)newcapacity > 0X7FFFFFC7) newcapacity = 0X7FFFFFC7;

        // If the computed capacity is still less than specified, set to the original argument.
        // Capacities exceeding Array.MaxLength will be surfaced as OutOfMemoryException by Array.Resize.
        if (newcapacity < capacity) newcapacity = capacity;

        Capacity = newcapacity;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> WrittenSpan()
    {
        return new Span<T>(_items, Count);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Span<T> TotalSpan()
    {
        return new Span<T>(_items, _arrayLength);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Dispose()
    {
        NativeMemoryHelper.Free(_items);
        NativeMemoryHelper.RemoveNativeMemoryByte(_arrayLength * Unsafe.SizeOf<T>());
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
    public Enumerator GetEnumerator()
    {
        return new Enumerator(self);
    }

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var item in *self) sb.Append($"{item}  ");
        return sb.ToString();
    }

    public struct Enumerator : IEnumerator<T>
    {
        object IEnumerator.Current => Current;

        private int CurrentIndex;

        private T CurrentItem;

        private readonly List<T>* Items;

        internal Enumerator(List<T>* items)
        {
            Items = items;
            CurrentIndex = 0;
            CurrentItem = default;
        }

        private void Initialize()
        {
            CurrentIndex = 0;
            CurrentItem = default;
        }

        public bool MoveNext()
        {
            if (CurrentIndex == Items->Count) return false;

            CurrentItem = Items->_items[CurrentIndex];

            CurrentIndex++;
            return true;
        }

        public void Reset()
        {
            Initialize();
        }

        public T Current => CurrentItem;

        public void Dispose()
        {
        }
    }

    public void OnReturnToPool()
    {
        Clear();
    }

    public void OnGetFromPool()
    {
        
    }
}
}

