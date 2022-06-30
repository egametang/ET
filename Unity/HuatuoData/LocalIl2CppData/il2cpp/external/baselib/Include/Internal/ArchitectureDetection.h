#pragma once

// Detect 64/32bit if not user defined.
#if !defined(PLATFORM_ARCH_64) && !defined(PLATFORM_ARCH_32)
    #if defined(_AMD64_) || defined(__LP64__) || defined(_WIN64) || defined(_M_ARM64)
        #define PLATFORM_ARCH_64 1
        #define PLATFORM_ARCH_32 0
    #else
        #define PLATFORM_ARCH_64 0
        #define PLATFORM_ARCH_32 1
    #endif
#elif !defined(PLATFORM_ARCH_64)
    #define PLATFORM_ARCH_64 (PLATFORM_ARCH_32 ? 0 : 1)
#elif !defined(PLATFORM_ARCH_32)
    #define PLATFORM_ARCH_32 (PLATFORM_ARCH_64 ? 0 : 1)
#endif

// Cache line size in bytes
#ifndef PLATFORM_CACHE_LINE_SIZE
    #define PLATFORM_CACHE_LINE_SIZE 64
#endif

// Detect endianess if not user defined.
#if !defined(PLATFORM_ARCH_BIG_ENDIAN) && !defined(PLATFORM_ARCH_LITTLE_ENDIAN)
    #if defined(__BIG_ENDIAN__)
        #define PLATFORM_ARCH_BIG_ENDIAN    1
        #define PLATFORM_ARCH_LITTLE_ENDIAN 0
    #else
        #define PLATFORM_ARCH_BIG_ENDIAN    0
        #define PLATFORM_ARCH_LITTLE_ENDIAN 1
    #endif
#elif !defined(PLATFORM_ARCH_BIG_ENDIAN)
    #define PLATFORM_ARCH_BIG_ENDIAN    (PLATFORM_ARCH_LITTLE_ENDIAN ? 0 : 1)
#elif !defined(PLATFORM_ARCH_LITTLE_ENDIAN)
    #define PLATFORM_ARCH_LITTLE_ENDIAN (PLATFORM_ARCH_BIG_ENDIAN ? 0 : 1)
#endif


// Detect SIMD features.

// SSE2
// Naming is inherited from Unity and indicates full SSE2 support.
#ifndef PLATFORM_SUPPORTS_SSE
    #if (defined(_M_IX86_FP) && _M_IX86_FP == 2) || defined(_M_AMD64) || defined(_M_X64) || defined(__SSE2__)
        #define PLATFORM_SUPPORTS_SSE 1
    #else
        #define PLATFORM_SUPPORTS_SSE 0
    #endif
#endif

// NEON
// Indicates general availability. Note that there can be some differences in the exact instructions available.
#ifndef PLATFORM_SUPPORTS_NEON
    #if defined(__ARM_NEON) || defined(__ARM_NEON__) || defined(__ARM_NEON_FP) || \
    (defined(_MSC_VER) && (defined(_M_ARM) || defined(_M_ARM64)))
        #define PLATFORM_SUPPORTS_NEON 1
    #else
        #define PLATFORM_SUPPORTS_NEON 0
    #endif
#endif
