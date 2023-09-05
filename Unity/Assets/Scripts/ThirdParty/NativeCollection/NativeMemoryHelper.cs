using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace NativeCollection
{
    public static unsafe class NativeMemoryHelper
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(UIntPtr byteCount)
        {
            AddNativeMemoryByte((long)byteCount);
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc(byteCount);
#else
        return Marshal.AllocHGlobal((int)byteCount).ToPointer();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* Alloc(UIntPtr elementCount, UIntPtr elementSize)
        {
            AddNativeMemoryByte((long)((long)elementCount * (long)elementSize));
#if NET6_0_OR_GREATER
            return NativeMemory.Alloc(elementCount, elementSize);
#else
        return Marshal.AllocHGlobal((int)((int)elementCount*(int)elementSize)).ToPointer();
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(UIntPtr byteCount)
        {
            AddNativeMemoryByte((long)byteCount);
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed(byteCount);
#else
        var ptr = Marshal.AllocHGlobal((int)byteCount).ToPointer();
        Unsafe.InitBlockUnaligned(ptr,0,(uint)byteCount);
        return ptr;
#endif
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void* AllocZeroed(UIntPtr elementCount, UIntPtr elementSize)
        {
            AddNativeMemoryByte((long)((long)elementCount * (long)elementSize));
#if NET6_0_OR_GREATER
            return NativeMemory.AllocZeroed(elementCount, elementSize);
#else
        var ptr = Marshal.AllocHGlobal((int)((int)elementCount*(int)elementSize)).ToPointer();
        Unsafe.InitBlockUnaligned(ptr,0,(uint)((uint)elementCount*(uint)elementSize));
        return ptr;
#endif
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Free<T>(T* ptr) where T : unmanaged
        {
#if NET6_0_OR_GREATER
            NativeMemory.Free(ptr);
#else
        Marshal.FreeHGlobal(new IntPtr(ptr));
#endif
        }
        
#if MEMORY_PROFILE
        public static long NativeMemoryBytes;
#endif
        
        public static void AddNativeMemoryByte(long size)
        {
            GC.AddMemoryPressure((long)size);
            //Console.WriteLine($"AddNativeMemoryByte {size}");
#if MEMORY_PROFILE
            NativeMemoryBytes += size;
#endif
        }
        
        public static void RemoveNativeMemoryByte(long size)
        {
            GC.RemoveMemoryPressure(size);
            //Console.WriteLine($"RemoveMemoryPressure {size}");
#if MEMORY_PROFILE
            NativeMemoryBytes -= size;
#endif
        }

        public static long GetNativeMemoryBytes()
        {
#if MEMORY_PROFILE
            return NativeMemoryBytes;
#else
            return 0;
#endif
        }
    }    
}

