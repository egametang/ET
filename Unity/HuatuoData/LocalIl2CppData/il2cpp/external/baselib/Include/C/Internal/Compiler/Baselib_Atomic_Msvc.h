#pragma once

#include "../../../C/Baselib_Atomic.h"
#include "../../../C/Baselib_Atomic_Macros.h"

#include "Baselib_Atomic_MsvcIntrinsics.h"

#define detail_relaxed_relaxed(...) __VA_ARGS__
#define detail_relaxed_acquire(...)
#define detail_relaxed_release(...)
#define detail_relaxed_acq_rel(...)
#define detail_relaxed_seq_cst(...)
#define detail_acquire_relaxed(...)
#define detail_acquire_acquire(...) __VA_ARGS__
#define detail_acquire_release(...)
#define detail_acquire_acq_rel(...)
#define detail_acquire_seq_cst(...)
#define detail_release_relaxed(...)
#define detail_release_acquire(...)
#define detail_release_release(...) __VA_ARGS__
#define detail_release_acq_rel(...)
#define detail_release_seq_cst(...)
#define detail_acq_rel_relaxed(...)
#define detail_acq_rel_acquire(...)
#define detail_acq_rel_release(...)
#define detail_acq_rel_acq_rel(...) __VA_ARGS__
#define detail_acq_rel_seq_cst(...)
#define detail_seq_cst_relaxed(...)
#define detail_seq_cst_acquire(...)
#define detail_seq_cst_release(...)
#define detail_seq_cst_acq_rel(...)
#define detail_seq_cst_seq_cst(...) __VA_ARGS__


#define detail_relaxed(memory_order, ...) detail_relaxed_##memory_order(__VA_ARGS__)
#define detail_acquire(memory_order, ...) detail_acquire_##memory_order(__VA_ARGS__)
#define detail_release(memory_order, ...) detail_release_##memory_order(__VA_ARGS__)
#define detail_acq_rel(memory_order, ...) detail_acq_rel_##memory_order(__VA_ARGS__)
#define detail_seq_cst(memory_order, ...) detail_seq_cst_##memory_order(__VA_ARGS__)

// Intel
// ------------------------------------------------------------------------------------------------------------------------------------------------------
#if defined(_M_IX86) || defined(_M_X64)

#define detail_intrinsic_relaxed
#define detail_intrinsic_acquire
#define detail_intrinsic_release
#define detail_intrinsic_acq_rel
#define detail_intrinsic_seq_cst

#if defined(_M_X64)

#define detail_THREAD_FENCE(order, ...)                                                                                                                 \
static COMPILER_FORCEINLINE void Baselib_atomic_thread_fence_##order()                                                                                  \
{                                                                                                                                                       \
    detail_acquire(order, _ReadWriteBarrier());                                                                                                         \
    detail_release(order, _ReadWriteBarrier());                                                                                                         \
    detail_acq_rel(order, _ReadWriteBarrier());                                                                                                         \
    detail_seq_cst(order, __faststorefence());                                                                                                          \
}

#else // #defined(_M_IX86)

#define detail_THREAD_FENCE(order, ...)                                                                                                                 \
static COMPILER_FORCEINLINE void Baselib_atomic_thread_fence_##order()                                                                                  \
{                                                                                                                                                       \
    detail_acquire(order, _ReadWriteBarrier());                                                                                                         \
    detail_release(order, _ReadWriteBarrier());                                                                                                         \
    detail_acq_rel(order, _ReadWriteBarrier());                                                                                                         \
    detail_seq_cst(order, _ReadWriteBarrier(); __int32 temp = 0; _InterlockedExchange32(&temp, 0); _ReadWriteBarrier());                                \
}

#endif


#define detail_LOAD_BITS_8(obj, result) *(__int8*)result = *(const volatile __int8*)obj
#define detail_LOAD_BITS_16(obj, result) *(__int16*)result = *(const volatile __int16*)obj
#define detail_LOAD_BITS_32(obj, result) *(__int32*)result = *(const volatile __int32*)obj
#if PLATFORM_ARCH_64
    #define detail_LOAD_BITS_64(obj, result) *(__int64*)result = *(const volatile __int64*)obj
#else
// x86 32-bit load/store 64-bit integer.
// - SSE2 enabled yields (identical to __mm_store/load):
// movsd   xmm0, QWORD PTR unsigned __int64 obj
// movsd   QWORD PTR unsigned __int64 result, xmm0
// - No SSE2 enabled yields:
// fld     QWORD PTR unsigned __int64 obj
// fstp    QWORD PTR unsigned __int64 result
// Link comparing various implementations: https://godbolt.org/z/T3zW5M
    #define detail_LOAD_BITS_64(obj, result) *(double*)result = *(const volatile double*)obj
#endif

#define detail_LOAD(op, order, id , bits, int_type, ...)                                                                                                \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(const void* obj, void* result)                                                        \
{                                                                                                                                                       \
    detail_LOAD_BITS_##bits(obj, result);                                                                                                               \
    detail_acquire(order, _ReadWriteBarrier());                                                                                                         \
    detail_seq_cst(order, _ReadWriteBarrier());                                                                                                         \
}

#define detail_LOAD_NOT_CONST(op, order, id , bits, int_type, ...)                                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, void* result)                                                              \
{                                                                                                                                                       \
    detail_LOAD_BITS_##bits(obj, result);                                                                                                               \
    detail_acquire(order, _ReadWriteBarrier());                                                                                                         \
    detail_seq_cst(order, _ReadWriteBarrier());                                                                                                         \
}

#define detail_STORE_BITS_8(obj, value) *(volatile __int8*)obj = *(const __int8*)value
#define detail_STORE_BITS_16(obj, value) *(volatile __int16*)obj = *(const __int16*)value
#define detail_STORE_BITS_32(obj, value) *(volatile __int32*)obj = *(const __int32*)value
#if PLATFORM_ARCH_64
    #define detail_STORE_BITS_64(obj, value) *(volatile __int64*)obj = *(const __int64*)value
#else
    #define detail_STORE_BITS_64(obj, value) *(volatile double*)obj = *(double*)value
#endif

#define detail_STORE(op, order, id , bits, int_type, ...)                                                                                               \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value)                                                         \
{                                                                                                                                                       \
    detail_relaxed(order, detail_STORE_BITS_##bits(obj, value));                                                                                        \
    detail_release(order, detail_STORE_BITS_##bits(obj, value); _ReadWriteBarrier());                                                                   \
    detail_seq_cst(order, _InterlockedExchange##bits((__int##bits*)obj, *(const __int##bits*)value));                                                   \
}

// ARM
// ------------------------------------------------------------------------------------------------------------------------------------------------------
#elif defined(_M_ARM) || defined(_M_ARM64)

#define detail_intrinsic_relaxed _nf
#define detail_intrinsic_acquire _acq
#define detail_intrinsic_release _rel
#define detail_intrinsic_acq_rel
#define detail_intrinsic_seq_cst

#define detail_THREAD_FENCE(order, ...)                                                                                                                 \
static COMPILER_FORCEINLINE void Baselib_atomic_thread_fence_##order()                                                                                  \
{                                                                                                                                                       \
    detail_acquire(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_release(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_acq_rel(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_seq_cst(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
}

#define detail_LOAD(op, order, id , bits, int_type, ...)                                                                                                \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(const void* obj, void* result)                                                        \
{                                                                                                                                                       \
    *(__int##bits*)result = __iso_volatile_load##bits((const __int##bits*)obj);                                                                         \
    detail_acquire(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_seq_cst(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
}

#define detail_LOAD_NOT_CONST(op, order, id , bits, int_type, ...)                                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, void* result)                                                              \
{                                                                                                                                                       \
    *(__int##bits*)result = __iso_volatile_load##bits((const __int##bits*)obj);                                                                         \
    detail_acquire(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_seq_cst(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
}

#define detail_STORE(op, order, id , bits, int_type, ...)                                                                                               \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value)                                                         \
{                                                                                                                                                       \
    detail_release(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    detail_seq_cst(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
    __iso_volatile_store##bits((__int##bits*) obj, *(const __int##bits*)value);                                                                         \
    detail_seq_cst(order, __dmb(_ARM_BARRIER_ISH));                                                                                                     \
}

#endif

// Common
// ------------------------------------------------------------------------------------------------------------------------------------------------------

#define detail_intrinsic_exchange   _InterlockedExchange
#define detail_intrinsic_fetch_add  _InterlockedExchangeAdd
#define detail_intrinsic_fetch_and  _InterlockedAnd
#define detail_intrinsic_fetch_or   _InterlockedOr
#define detail_intrinsic_fetch_xor  _InterlockedXor

#define detail_LOAD_STORE(op, order, id , bits, int_type, ...)                                                                                          \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                                           \
{                                                                                                                                                       \
    *(__int##bits##*)result = PP_CONCAT(detail_intrinsic_##op, bits, detail_intrinsic_##order)((__int##bits##*)obj, *(const __int##bits##*)value);      \
}

#define detail_CMP_XCHG(op, order1, order2, id , bits, int_type, ...)                                                                                   \
static FORCE_INLINE bool Baselib_atomic_##op##_##id##_##order1##_##order2##_v(void* obj, void* expected, const void* value)                             \
{                                                                                                                                                       \
    __int##bits cmp =  *(__int##bits##*)expected;                                                                                                       \
    __int##bits result = PP_CONCAT(_InterlockedCompareExchange, bits, detail_intrinsic_##order1)((__int##bits##*)obj, *(__int##bits##*)value, cmp);     \
    return result == cmp ? true : (*(__int##bits##*)expected = result, false);                                                                          \
}

#define detail_NOT_SUPPORTED(...)

// Setup implementation
// ------------------------------------------------------------------------------------------------------------------------------------------------------

Baselib_Atomic_FOR_EACH_MEMORY_ORDER(
    detail_THREAD_FENCE
)

Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE(
    detail_LOAD,        // load
    detail_STORE,       // store
    detail_LOAD_STORE,  // add
    detail_LOAD_STORE,  // and
    detail_LOAD_STORE,  // or
    detail_LOAD_STORE,  // xor
    detail_LOAD_STORE,  // exchange
    detail_CMP_XCHG,    // compare_exchange_weak
    detail_CMP_XCHG     // compare_exchange_strong
)

#if PLATFORM_ARCH_64

// 128-bit implementation
// There are more efficient ways of doing load, store and exchange on Arm64. Unfortunately MSVC doesn't provide intrinsics for those. The specific
// instructions needed to perform atomic load, store and exchange are also not available on MSVC.
// Hence we fallback to cmpxchg for all atomic ops.
// ------------------------------------------------------------------------------------------------------------------------------------------------------
#define detail_LOAD128(op, order, id, ...)                                                                                                              \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, void* result)                                                              \
{                                                                                                                                                       \
    Baselib_atomic_compare_exchange_weak_128_##order##_##order##_v((void*)obj, result, result);                                                         \
}

#define detail_STORE128(op, order, id, ...)                                                                                                             \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value)                                                         \
{                                                                                                                                                       \
    uint64_t comparand[2] = { ((volatile uint64_t*)obj)[0], ((volatile uint64_t*)obj)[1] };                                                             \
    while(!Baselib_atomic_compare_exchange_weak_128_##order##_relaxed_v(obj, comparand, value))                                                         \
        ;                                                                                                                                               \
}

#define detail_XCHG128(op, order, id, ...)                                                                                                              \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                                           \
{                                                                                                                                                       \
    ((uint64_t*)result)[0] = ((volatile uint64_t*)obj)[0];                                                                                              \
    ((uint64_t*)result)[1] = ((volatile uint64_t*)obj)[1];                                                                                              \
    while(!Baselib_atomic_compare_exchange_weak_128_##order##_relaxed_v(obj, result, value))                                                            \
        ;                                                                                                                                               \
}

#define detail_CMP_XCHG128(op, order1, order2, id, ...)                                                                                                 \
static FORCE_INLINE bool Baselib_atomic_##op##_##id##_##order1##_##order2##_v(void* obj, void* expected, const void* value)                             \
{                                                                                                                                                       \
    return PP_CONCAT(_InterlockedCompareExchange128, detail_intrinsic_##order1)(                                                                        \
        (__int64*)obj,                                                                                                                                  \
        ((const __int64*)value)[1],                                                                                                                     \
        ((const __int64*)value)[0],                                                                                                                     \
        (__int64*)expected                                                                                                                              \
    ) == 1;                                                                                                                                             \
}

Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(
    detail_LOAD128,         // load
    detail_STORE128,        // store
    detail_NOT_SUPPORTED,   // add
    detail_NOT_SUPPORTED,   // and
    detail_NOT_SUPPORTED,   // or
    detail_NOT_SUPPORTED,   // xor
    detail_XCHG128,         // exchange
    detail_CMP_XCHG128,     // compare_exchange_weak
    detail_CMP_XCHG128,     // compare_exchange_strong
    128
)

Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(
    detail_LOAD128,         // load
    detail_STORE128,        // store
    detail_NOT_SUPPORTED,   // add
    detail_NOT_SUPPORTED,   // and
    detail_NOT_SUPPORTED,   // or
    detail_NOT_SUPPORTED,   // xor
    detail_XCHG128,         // exchange
    detail_CMP_XCHG128,     // compare_exchange_weak
    detail_CMP_XCHG128,     // compare_exchange_strong
    ptr2x
)

#undef detail_LOAD128
#undef detail_STORE128
#undef detail_XCHG128
#undef detail_CMP_XCHG128

#else

Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(
    detail_LOAD_NOT_CONST,  // load
    detail_STORE,           // store
    detail_NOT_SUPPORTED,   // add
    detail_NOT_SUPPORTED,   // and
    detail_NOT_SUPPORTED,   // or
    detail_NOT_SUPPORTED,   // xor
    detail_LOAD_STORE,      // exchange
    detail_CMP_XCHG,        // compare_exchange_weak
    detail_CMP_XCHG,        // compare_exchange_strong
    ptr2x, 64, int64_t
)

#endif

#undef detail_THREAD_FENCE
#undef detail_LOAD
#undef detail_LOAD_NOT_CONST
#undef detail_STORE
#undef detail_LOAD_STORE
#undef detail_CMP_XCHG
#undef detail_NOT_SUPPORTED

#undef detail_LOAD_BITS_8
#undef detail_LOAD_BITS_16
#undef detail_LOAD_BITS_32
#undef detail_LOAD_BITS_64
#undef detail_STORE_BITS_8
#undef detail_STORE_BITS_16
#undef detail_STORE_BITS_32
#undef detail_STORE_BITS_64

#undef detail_intrinsic_exchange
#undef detail_intrinsic_fetch_add
#undef detail_intrinsic_fetch_and
#undef detail_intrinsic_fetch_or
#undef detail_intrinsic_fetch_xor

#undef detail_relaxed_relaxed
#undef detail_relaxed_acquire
#undef detail_relaxed_release
#undef detail_relaxed_acq_rel
#undef detail_relaxed_seq_cst
#undef detail_acquire_relaxed
#undef detail_acquire_acquire
#undef detail_acquire_release
#undef detail_acquire_acq_rel
#undef detail_acquire_seq_cst
#undef detail_release_relaxed
#undef detail_release_acquire
#undef detail_release_release
#undef detail_release_acq_rel
#undef detail_release_seq_cst
#undef detail_acq_rel_relaxed
#undef detail_acq_rel_acquire
#undef detail_acq_rel_release
#undef detail_acq_rel_acq_rel
#undef detail_acq_rel_seq_cst
#undef detail_seq_cst_relaxed
#undef detail_seq_cst_acquire
#undef detail_seq_cst_release
#undef detail_seq_cst_acq_rel
#undef detail_seq_cst_seq_cst

#undef detail_relaxed
#undef detail_acquire
#undef detail_release
#undef detail_acq_rel
#undef detail_seq_cst
