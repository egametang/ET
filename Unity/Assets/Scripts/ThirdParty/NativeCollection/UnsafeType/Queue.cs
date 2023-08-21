using System;
using System.Runtime.CompilerServices;

namespace NativeCollection.UnsafeType
{
    public unsafe struct Queue<T> : IDisposable where T : unmanaged
{
    private T* _array;
    private int length;

    private int _head;
    private int _tail;
    private int _version;

    public static Queue<T>* Create(int capacity = 10)
    {
        if (capacity < 0) throw new ArgumentOutOfRangeException("Capacity<0");

        var queue = (Queue<T>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<Queue<T>>());
        queue->_array = (T*)NativeMemoryHelper.Alloc((UIntPtr)capacity, (UIntPtr)Unsafe.SizeOf<T>());
        queue->length = capacity;
        queue->_head = 0;
        queue->_tail = 0;
        queue->Count = 0;
        queue->_version = 0;
        return queue;
    }

    public int Count { get; private set; }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Clear()
    {
        if (Count != 0) Count = 0;
        _head = 0;
        _tail = 0;
        ++_version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Enqueue(in T item)
    {
        if (Count == length)
            Grow(Count + 1);
        _array[_tail] = item;
        MoveNext(ref _tail);
        ++Count;
        ++_version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Dequeue()
    {
        var head = _head;
        var array = _array;
        if (Count == 0)
            ThrowHelper.QueueEmptyException();

        var obj = array[head];
        MoveNext(ref _head);
        --Count;
        ++_version;
        return obj;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryDequeue(out T result)
    {
        var head = _head;
        var array = _array;
        if (Count == 0)
        {
            result = default;
            return false;
        }

        result = array[head];
        MoveNext(ref _head);
        --Count;
        ++_version;
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public T Peek()
    {
        if (Count == 0)
            ThrowHelper.QueueEmptyException();
        return _array[_head];
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public bool TryPeek(out T result)
    {
        if (Count == 0)
        {
            result = default;
            return false;
        }

        result = _array[_head];
        return true;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Grow(int capacity)
    {
        var val1 = 2 * length;
        if ((uint)val1 > 2147483591U)
            val1 = 2147483591;
        var capacity1 = Math.Max(val1, length + 4);
        if (capacity1 < capacity)
            capacity1 = capacity;
        SetCapacity(capacity1);
    }

    private void SetCapacity(int capacity)
    {
        var destinationArray = (T*)NativeMemoryHelper.Alloc((UIntPtr)capacity, (UIntPtr)Unsafe.SizeOf<T>());
        if (Count > 0)
        {
            if (_head < _tail)
            {
                Unsafe.CopyBlockUnaligned(destinationArray, _array + _head, (uint)(Count * Unsafe.SizeOf<T>()));
            }
            else
            {
                Unsafe.CopyBlockUnaligned(destinationArray, _array + _head,
                    (uint)((length - _head) * Unsafe.SizeOf<T>()));
                Unsafe.CopyBlockUnaligned(destinationArray + length - _head, _array,
                    (uint)(_tail * Unsafe.SizeOf<T>()));
            }
        }

        NativeMemoryHelper.Free(_array);
        NativeMemoryHelper.RemoveNativeMemoryByte(length * Unsafe.SizeOf<T>());
        _array = destinationArray;
        _head = 0;
        _tail = Count == capacity ? 0 : Count;
        length = capacity;
        ++_version;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void MoveNext(ref int index)
    {
        var num = index + 1;
        if (num == length)
            num = 0;
        index = num;
    }

    public void Dispose()
    {
        if (_array!=null)
        {
            NativeMemoryHelper.Free(_array);
            NativeMemoryHelper.RemoveNativeMemoryByte(length * Unsafe.SizeOf<T>());
        }
    }
}
}

