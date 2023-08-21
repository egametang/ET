using System;
using System.Runtime.CompilerServices;

namespace NativeCollection.UnsafeType
{
    public interface IPool : IDisposable
    {
        public void OnReturnToPool();
        public void OnGetFromPool();
    }
    
    public unsafe struct NativePool<T> : IDisposable where T: unmanaged,IPool
    {
        public int MaxSize { get; private set; }
        private Stack<IntPtr>* _stack;
        public static NativePool<T>* Create(int maxPoolSize)
        {
            NativePool<T>* pool = (NativePool<T>*)NativeMemoryHelper.Alloc((UIntPtr)Unsafe.SizeOf<NativePool<T>>());
            pool->_stack = Stack<IntPtr>.Create();
            pool->MaxSize = maxPoolSize;
            return pool;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T* Alloc()
        {
            if (_stack->TryPop(out var itemPtr))
            {
                var item = (T*)itemPtr;
                item->OnGetFromPool();
                return item;
            }
            return null;
        }
    
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Return(T* ptr)
        {
            if (_stack->Count>=MaxSize)
            {
                ptr->Dispose();
                NativeMemoryHelper.Free(ptr);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<T>());
                return;
            }
            ptr->OnReturnToPool();
            _stack->Push(new IntPtr(ptr));
        }
    
        public void Clear()
        {
            while (_stack->TryPop(out var ptr))
            {
                T* item = (T*)ptr;
                item->Dispose();
                NativeMemoryHelper.Free(item);
                NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<T>());
            }
        }
    
        public void Dispose()
        {
            Clear();
            _stack->Dispose();
            NativeMemoryHelper.Free(_stack);
            NativeMemoryHelper.RemoveNativeMemoryByte(Unsafe.SizeOf<Stack<T>>());
        }
    }
}

