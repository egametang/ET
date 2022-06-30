#pragma once

// On Darwin we can get TLS slot table pointer directly.

#if defined(__arm__)
#include <arm/arch.h>
#endif

#if defined(__i386__) || defined(__x86_64__)

// "Annotating a pointer with address space #256 causes it to be code generated relative to the X86 GS segment register"
// from https://opensource.apple.com/source/clang/clang-137/src/tools/clang/docs/LanguageExtensions.html
#define Baselib_TLS_Darwin_SlotTable() ((void* __attribute__((address_space(256)))*) NULL)

#elif defined(__arm__) && defined(_ARM_ARCH_7)

static COMPILER_FORCEINLINE void** Baselib_TLS_Darwin_SlotTable(void)
{
    return (void**)(__builtin_arm_mrc(15, 0, 13, 0, 3) & (~0x3u));
}

#elif defined(__aarch64__)

static COMPILER_FORCEINLINE void** Baselib_TLS_Darwin_SlotTable(void)
{
    uint64_t tsd;
    __asm__ ("mrs %0, TPIDRRO_EL0" : "=r" (tsd));
    return (void**)(tsd & (~0x7ull));
}

#else

#error Baselib_TLS_Darwin_SlotTable is not implemented on this platform.

#endif

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

BASELIB_FORCEINLINE_API void Baselib_TLS_Set(Baselib_TLS_Handle handle, uintptr_t value)
{
    Baselib_TLS_Darwin_SlotTable()[handle] = (void*)value;
}

BASELIB_FORCEINLINE_API uintptr_t Baselib_TLS_Get(Baselib_TLS_Handle handle)
{
    return (uintptr_t)Baselib_TLS_Darwin_SlotTable()[handle];
}

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
