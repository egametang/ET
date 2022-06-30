#pragma once

enum { Baselib_SystemSemaphore_PlatformSize = 1 }; // unused but 1 to simplify things

// Don't include sdkddkver.h, because we might accidentally use API not available on older system
// Include APIs from Windows 7 if versions are not defined yet
#ifndef NTDDI_VERSION
    #define NTDDI_VERSION    0x06010000
#endif
#ifndef WINVER
    #define WINVER           0x0601
#endif
#ifndef _WIN32_WINNT
    #define _WIN32_WINNT     0x0601
#endif

// These affect <windows.h> behavior; define them here so that no matter how/if later windows.h is included, it will get consistent result.
#ifndef WIN32_LEAN_AND_MEAN
    #define WIN32_LEAN_AND_MEAN
#endif
#ifndef NOMINMAX
    #define NOMINMAX 1
#endif

#if !defined(_AMD64_) && PLATFORM_ARCH_64 && defined(_M_X64)
    #define _AMD64_
#endif
#if PLATFORM_ARCH_64 && !defined(_ARM64_) && defined(_M_ARM64)
    #define _ARM64_
#endif
#if !defined(_X86_) && PLATFORM_ARCH_32 && defined(_M_IX86)
    #define _X86_
#endif
#if PLATFORM_ARCH_32 && !defined(_ARM_) && defined(_M_ARM)
    #define _ARM_
#endif

#ifndef EXPORTED_SYMBOL
    #define EXPORTED_SYMBOL __declspec(dllexport)
#endif
#ifndef IMPORTED_SYMBOL
    #define IMPORTED_SYMBOL __declspec(dllimport)
#endif

// Requires Windows 8
#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #define PLATFORM_FUTEX_NATIVE_SUPPORT 0
#endif

// Malloc is specified to have 16 byte alignment on 64bit platforms.
// see https://docs.microsoft.com/en-us/cpp/c-runtime-library/reference/malloc?view=vs-2019
#ifndef PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT
    #if PLATFORM_ARCH_64
        #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT 16
    #else
        #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT 8
    #endif
#endif
