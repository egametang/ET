using System;
using System.Runtime.CompilerServices;

namespace NativeCollection
{
    public unsafe class Stack<T> : INativeCollectionClass where T : unmanaged
    {
        private const int _defaultCapacity = 10;
        private UnsafeType.Stack<T>* _stack;
        private int _capacity;
        public Stack(int initialCapacity = _defaultCapacity)
        {
            _capacity = initialCapacity;
            _stack = UnsafeType.Stack<T>.Create(_capacity);
            IsDisposed = false;
        }
    
        public int Count => _stack->Count;
    
        public void Dispose()
        {
            if (IsDisposed)
            {
                return;
            }
            if (_stack != null)
            {
                _stack->Dispose();
                NativeMemoryHelper.Free(_stack);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<UnsafeType.Stack<T>>());
                IsDisposed = true;
            }
        }
    
        public void Clear()
        {
            _stack->Clear();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in T obj)
        {
            return _stack->Contains(obj);
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Peak()
        {
            return _stack->Peak();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T Pop()
        {
            return _stack->Pop();
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool TryPop(out T result)
        {
            var returnValue = _stack->TryPop(out result);
            return returnValue;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Push(in T obj)
        {
            _stack->Push(obj);
        }
    
        ~Stack()
        {
            Dispose();
        }
    
        public void ReInit()
        {
            if (IsDisposed)
            {
                _stack = UnsafeType.Stack<T>.Create(_capacity);
                IsDisposed = false;
            }
        }
    
        public bool IsDisposed { get; private set; }
    }
}


