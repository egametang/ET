using System;
using System.Runtime.CompilerServices;

namespace NativeCollection
{
    public unsafe class Queue<T> : INativeCollectionClass where T : unmanaged
    {
        private UnsafeType.Queue<T>* _queue;
        private const int _defaultCapacity = 10;
        private int _capacity;
        public Queue(int capacity = 10)
        {
            _capacity = capacity;
            _queue = UnsafeType.Queue<T>.Create(_capacity);
            IsDisposed = false;
        }
    
        public int Count => _queue->Count;
    
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            if (_queue != null)
            {
                _queue->Dispose();
                NativeMemoryHelper.Free(_queue);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.Queue<T>>());
                IsDisposed = true;
            }
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Clear()
        {
            _queue->Clear();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Enqueue(in T item)
        {
            _queue->Enqueue(item);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Dequeue()
        {
            return _queue->Dequeue();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryDequeue(out T result)
        {
            var value = _queue->TryDequeue(out result);
            return value;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peek()
        {
            return _queue->Peek();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPeek(out T result)
        {
            var value = _queue->TryPeek(out result);
            return value;
        }
    
        ~Queue()
        {
            Dispose();
        }
    
        public void ReInit()
        {
            if (IsDisposed)
            {
                _queue = UnsafeType.Queue<T>.Create(_capacity);
                IsDisposed = false;
            }
        }
    
        public bool IsDisposed { get; private set; }
    }
}

