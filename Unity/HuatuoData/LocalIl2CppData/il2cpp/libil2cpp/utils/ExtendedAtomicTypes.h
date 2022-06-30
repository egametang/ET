#pragma once

#if defined(__x86_64__) || defined(_M_X64)

#   include <emmintrin.h>

/// atomic_word must be 8 bytes aligned if you want to use it with atomic_* ops.
#   if defined(_MSC_VER)
typedef __int64 atomic_word;
#   else
typedef long long atomic_word;
#   endif

/// atomic_word2 must be 16 bytes aligned if you want to use it with atomic_* ops.
union atomic_word2
{
    __m128i         v;
    struct
    {
        atomic_word lo, hi;
    };
};

#define IL2CPP_ATOMIC_HAS_QUEUE

#elif defined(__x86__) || defined(__i386__) || defined(_M_IX86)

/// atomic_word must be 4 bytes aligned if you want to use it with atomic_* ops.
typedef int atomic_word;

/// atomic_word2 must be 8 bytes aligned if you want to use it with atomic_* ops.
union atomic_word2
{
#   if defined(_MSC_VER)
    __int64 v;
#   else
    long long v;
#   endif
#   if !defined(__SSE2__)
    double d;
#   endif
    struct
    {
        atomic_word lo, hi;
    };
};

#define IL2CPP_ATOMIC_HAS_QUEUE

#elif defined(_M_ARM64) || (defined(__arm64__) || defined(__aarch64__)) && (defined(__clang__) || defined(__GNUC__))

typedef long long atomic_word;
struct alignas(16) atomic_word2
{
    atomic_word lo;
    atomic_word hi;
};

#define IL2CPP_ATOMIC_HAS_QUEUE

#elif defined(_M_ARM) || (defined(__arm__) && (defined(__ARM_ARCH_7__) || defined(__ARM_ARCH_7A__) || defined(__ARM_ARCH_7R__) || defined(__ARM_ARCH_7M__) || defined(__ARM_ARCH_7S__)) && (defined(__clang__) || defined(__GNUC__)))

typedef int atomic_word;
union atomic_word2
{
#   if defined(_MSC_VER)
    __int64 v;
#   else
    long long v;
#   endif
    struct
    {
        atomic_word lo;
        atomic_word hi;
    };
};

#define IL2CPP_ATOMIC_HAS_QUEUE

#elif defined(__EMSCRIPTEN__)

#include <stdint.h>
typedef int32_t atomic_word;
union atomic_word2
{
    int64_t  v;
    struct
    {
        atomic_word lo;
        atomic_word hi;
    };
};

#else

#error There is no atomic_word implementation for this platform.

#endif
