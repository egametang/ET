#pragma once

#ifdef _MSC_VER
#define IL2CPP_DISABLE_OPTIMIZATIONS __pragma(optimize("", off))
#define IL2CPP_ENABLE_OPTIMIZATIONS __pragma(optimize("", on))
#elif IL2CPP_TARGET_LINUX
#define IL2CPP_DISABLE_OPTIMIZATIONS
#define IL2CPP_ENABLE_OPTIMIZATIONS
#else
#define IL2CPP_DISABLE_OPTIMIZATIONS __attribute__ ((optnone))
#define IL2CPP_ENABLE_OPTIMIZATIONS
#endif


#if IL2CPP_ENABLE_WRITE_BARRIERS
void Il2CppCodeGenWriteBarrier(void** targetAddress, void* object);
void Il2CppCodeGenWriteBarrierForType(const Il2CppType* type, void** targetAddress, void* object);
void Il2CppCodeGenWriteBarrierForClass(Il2CppClass* klass, void** targetAddress, void* object);
#else
inline void Il2CppCodeGenWriteBarrier(void** targetAddress, void* object) {}
inline void Il2CppCodeGenWriteBarrierForType(const Il2CppType* type, void** targetAddress, void* object) {}
inline void Il2CppCodeGenWriteBarrierForClass(Il2CppClass* klass, void** targetAddress, void* object) {}
#endif

inline void* il2cpp_codegen_memcpy(void* dest, const void* src, size_t count)
{
    return memcpy(dest, src, count);
}

inline void* il2cpp_codegen_memcpy(intptr_t dest, const void* src, size_t count)
{
    return memcpy((void*)dest, src, count);
}

inline void* il2cpp_codegen_memcpy(uintptr_t dest, const void* src, size_t count)
{
    return memcpy((void*)dest, src, count);
}

inline void* il2cpp_codegen_memcpy(void* dest, intptr_t src, size_t count)
{
    return memcpy(dest, (void*)src, count);
}

inline void* il2cpp_codegen_memcpy(intptr_t dest, intptr_t src, size_t count)
{
    return memcpy((void*)dest, (void*)src, count);
}

inline void* il2cpp_codegen_memcpy(uintptr_t dest, intptr_t src, size_t count)
{
    return memcpy((void*)dest, (void*)src, count);
}

inline void* il2cpp_codegen_memcpy(void* dest, uintptr_t src, size_t count)
{
    return memcpy(dest, (void*)src, count);
}

inline void* il2cpp_codegen_memcpy(intptr_t dest, uintptr_t src, size_t count)
{
    return memcpy((void*)dest, (void*)src, count);
}

inline void* il2cpp_codegen_memcpy(uintptr_t dest, uintptr_t src, size_t count)
{
    return memcpy((void*)dest, (void*)src, count);
}

inline void il2cpp_codegen_memset(void* ptr, int value, size_t num)
{
    memset(ptr, value, num);
}

inline void il2cpp_codegen_memset(intptr_t ptr, int value, size_t num)
{
    memset((void*)ptr, value, num);
}

inline void il2cpp_codegen_memset(uintptr_t ptr, int value, size_t num)
{
    memset((void*)ptr, value, num);
}

inline void il2cpp_codegen_initobj(void* value, size_t size)
{
    memset(value, 0, size);
}

inline void il2cpp_codegen_initobj(intptr_t value, size_t size)
{
    memset((void*)value, 0, size);
}

inline void il2cpp_codegen_initobj(uintptr_t value, size_t size)
{
    memset((void*)value, 0, size);
}
