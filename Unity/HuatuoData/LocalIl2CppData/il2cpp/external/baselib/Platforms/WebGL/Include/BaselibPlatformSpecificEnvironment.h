#pragma once

#ifndef __EMSCRIPTEN_PTHREADS__
/*
namespace detail
{
    struct Semaphore
    {
        uint32_t counter;
    };
}
*/
enum { Baselib_SystemSemaphore_PlatformSize = 4 }; // size should match size of struct above from no thread implementation
#else
enum { Baselib_SystemSemaphore_PlatformSize = 16 }; // sem_t with support for threads
#endif

#ifndef EXPORTED_SYMBOL
    #define EXPORTED_SYMBOL __attribute__((visibility("default")))
#endif
#ifndef IMPORTED_SYMBOL
    #define IMPORTED_SYMBOL
#endif

#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #ifdef __EMSCRIPTEN_PTHREADS__
        #define PLATFORM_FUTEX_NATIVE_SUPPORT 1
    #else
        #define PLATFORM_FUTEX_NATIVE_SUPPORT 0
    #endif
#endif

// Has custom atomics instead of compiler intrinsics.
#define PLATFORM_CUSTOM_ATOMICS 1

// The default, std::max_align_t, has the same value as this, but C's max_align_t is not available.
// See: https://github.com/emscripten-core/emscripten/blob/2bca083cbbd5a4133db61fbd74d04f7feecfa907/tests/core/test_stddef.cpp
#ifndef PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT
    #if __asmjs__
        #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT 8
    #else
        #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT 16
    #endif
#endif

#ifdef __cplusplus
extern "C" {
#endif

// This file is included throughout baselib in many compilation units, avoid pulling inp
// extra includes, so forward-declare the symbol we need from #include <emscripten.h>
void emscripten_debugger(void);

#ifdef __cplusplus
}
#endif

// The debugger statement invokes any available debugging functionality, such as setting a breakpoint.
// If no debugging functionality is available, this statement has no effect.
// Also Using compilers __builtin_debugtrap generates:
// "WARNING:root:disabling asm.js validation due to use of non-supported features: llvm.debugtrap is used"
#define BASELIB_DEBUG_TRAP() emscripten_debugger()
