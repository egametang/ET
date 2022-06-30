#pragma once

// TODO remove GCC intrinsics when https://github.com/WebAssembly/binaryen/issues/2170 is fixed (or switched to upstream backend)

#include <emscripten/threading.h>

#define detail_intrinsic_relaxed __ATOMIC_RELAXED
#define detail_intrinsic_acquire __ATOMIC_ACQUIRE
#define detail_intrinsic_release __ATOMIC_RELEASE
#define detail_intrinsic_acq_rel __ATOMIC_ACQ_REL
#define detail_intrinsic_seq_cst __ATOMIC_SEQ_CST

#define detail_THREAD_FENCE(order, ...)                                                                                       \
static FORCE_INLINE void Baselib_atomic_thread_fence_##order(void)                                                            \
{                                                                                                                             \
    emscripten_atomic_fence();                                                                                                \
}                                                                                                                             \

// When compiling without threads, atomic operations lead to compilation failure in some SDK versions.
#ifdef __EMSCRIPTEN_PTHREADS__

// We can't use emscripten_atomic_XXX_u8, due to a bug as it yields the following error message when linking for wasm:
// bad processUnshifted
// See bug ticket with minimal repro case here: https://github.com/WebAssembly/binaryen/issues/2170

#define detail_LOAD(op, order, id , bits, int_type, ...)                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(const void* obj, void* result)                              \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_load_u##bits((int_type*)obj);                                                  \
    else                                                                                                                      \
         __extension__({ __atomic_load((int_type*)obj, (int_type*)result, detail_intrinsic_##order); });                      \
}

#define detail_LOAD_NOT_CONST(op, order, id , bits, int_type, ...)                                                            \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, void* result)                                    \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_load_u##bits((int_type*)obj);                                                  \
    else                                                                                                                      \
         __extension__({ __atomic_load((int_type*)obj, (int_type*)result, detail_intrinsic_##order); });                      \
}

#define detail_STORE(op, order, id , bits, int_type, ...)                                                                     \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value)                               \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        emscripten_atomic_store_u##bits((int_type*)obj, *(const int_type*)value);                                             \
    else                                                                                                                      \
        __extension__({ __atomic_store((int_type*)obj, (int_type*)value, detail_intrinsic_##order); });                       \
}

#define detail_ADD(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_add_u##bits((int_type*)obj, *(const int_type*)value);                          \
    else                                                                                                                      \
        *(int_type*)result = __extension__({ __atomic_##op((int_type*)obj, *(int_type*)value, detail_intrinsic_##order); });  \
}

#define detail_AND(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_and_u##bits((int_type*)obj, *(const int_type*)value);                          \
    else                                                                                                                      \
        *(int_type*)result = __extension__({ __atomic_##op((int_type*)obj, *(int_type*)value, detail_intrinsic_##order); });  \
}

#define detail_OR(op, order, id , bits, int_type, ...)                                                                        \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_or_u##bits((int_type*)obj, *(const int_type*)value);                           \
    else                                                                                                                      \
        *(int_type*)result = __extension__({ __atomic_##op((int_type*)obj, *(int_type*)value, detail_intrinsic_##order); });  \
}

#define detail_XOR(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_xor_u##bits((int_type*)obj, *(const int_type*)value);                          \
    else                                                                                                                      \
        *(int_type*)result = __extension__({ __atomic_##op((int_type*)obj, *(int_type*)value, detail_intrinsic_##order); });  \
}

#define detail_XCHG(op, order, id , bits, int_type, ...)                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
        *(int_type*)result = emscripten_atomic_exchange_u##bits(obj, *(int_type*)value);                                      \
    else                                                                                                                      \
        __extension__({ __atomic_exchange((int_type*)obj, (int_type*)value, (int_type*)result, detail_intrinsic_##order); }); \
}

#define detail_CMP_XCHG_STRONG(op, order1, order2, id , bits, int_type, ...)                                                  \
static FORCE_INLINE bool Baselib_atomic_##op##_##id##_##order1##_##order2##_v(void* obj, void* expected, const void* value)   \
{                                                                                                                             \
    if (bits > 8)                                                                                                             \
    {                                                                                                                         \
        int_type prev = emscripten_atomic_cas_u##bits(obj, *(int_type*)expected, *(int_type*)value);                          \
        if (prev == *(int_type*)expected)                                                                                     \
            return true;                                                                                                      \
        *(int_type*)expected = prev;                                                                                          \
        return false;                                                                                                         \
    }                                                                                                                         \
    else                                                                                                                      \
    {                                                                                                                         \
        return __extension__({ __atomic_compare_exchange(                                                                     \
        (int_type*)obj,                                                                                                       \
        (int_type*)expected,                                                                                                  \
        (int_type*)value,                                                                                                     \
        1,                                                                                                                    \
        detail_intrinsic_##order1,                                                                                            \
        detail_intrinsic_##order2);                                                                                           \
        });                                                                                                                   \
    }                                                                                                                         \
}

#else

// TODO remove emulation as soon as switch to newer version of emscripten sdk

#define detail_LOAD(op, order, id , bits, int_type, ...)                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(const void* obj, void* result)                              \
{                                                                                                                             \
    *(int_type*)result = *(const int_type*)obj;                                                                               \
}

#define detail_LOAD_NOT_CONST(op, order, id , bits, int_type, ...)                                                            \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, void* result)                                    \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
}

#define detail_STORE(op, order, id , bits, int_type, ...)                                                                     \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value)                               \
{                                                                                                                             \
    *(int_type*)obj = *(const int_type*)value;                                                                                \
}

#define detail_ADD(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
    *(int_type*)obj += *(const int_type*)value;                                                                               \
}

#define detail_AND(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
    *(int_type*)obj &= *(const int_type*)value;                                                                               \
}

#define detail_OR(op, order, id , bits, int_type, ...)                                                                        \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
    *(int_type*)obj |= *(const int_type*)value;                                                                               \
}

#define detail_XOR(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
    *(int_type*)obj ^= *(const int_type*)value;                                                                               \
}

#define detail_XCHG(op, order, id , bits, int_type, ...)                                                                      \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order##_v(void* obj, const void* value, void* result)                 \
{                                                                                                                             \
    *(int_type*)result = *(int_type*)obj;                                                                                     \
    *(int_type*)obj = *(const int_type*)value;                                                                                \
}

// We have no idea why manual atomic CAS doesn't work and this intrinsic works.
#define detail_CMP_XCHG_STRONG(op, order1, order2, id , bits, int_type, ...)                                                  \
static FORCE_INLINE bool Baselib_atomic_##op##_##id##_##order1##_##order2##_v(void* obj, void* expected, const void* value)   \
{                                                                                                                             \
    return __extension__({ __atomic_compare_exchange(                                                                         \
        (int_type*)obj,                                                                                                       \
        (int_type*)expected,                                                                                                  \
        (int_type*)value,                                                                                                     \
        1,                                                                                                                    \
        detail_intrinsic_##order1,                                                                                            \
        detail_intrinsic_##order2);                                                                                           \
        });                                                                                                                   \
}

#endif

// AsmJs/WASM do not distinguish between strong/weak compare/exchange!
#define detail_CMP_XCHG_WEAK detail_CMP_XCHG_STRONG

#define detail_NOT_SUPPORTED(...)

Baselib_Atomic_FOR_EACH_MEMORY_ORDER(
    detail_THREAD_FENCE
)

Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE(
    detail_LOAD,            // load
    detail_STORE,           // store
    detail_ADD,             // add
    detail_AND,             // and
    detail_OR,              // or
    detail_XOR,             // xor
    detail_XCHG,            // exchange
    detail_CMP_XCHG_WEAK,   // compare_exchange_weak
    detail_CMP_XCHG_STRONG  // compare_exchange_strong
)

Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(
    detail_LOAD_NOT_CONST,      // load
    detail_STORE,               // store
    detail_NOT_SUPPORTED,       // add
    detail_NOT_SUPPORTED,       // and
    detail_NOT_SUPPORTED,       // or
    detail_NOT_SUPPORTED,       // xor
    detail_XCHG,                // exchange
    detail_CMP_XCHG_WEAK,       // compare_exchange_weak
    detail_CMP_XCHG_STRONG,     // compare_exchange_strong
    ptr2x, 64, int64_t          // type information
)

#undef detail_intrinsic_relaxed
#undef detail_intrinsic_acquire
#undef detail_intrinsic_release
#undef detail_intrinsic_acq_rel
#undef detail_intrinsic_seq_cst

#undef detail_THREAD_FENCE
#undef detail_LOAD
#undef detail_STORE
#undef detail_ADD
#undef detail_AND
#undef detail_OR
#undef detail_XOR
#undef detail_XCHG
#undef detail_CMP_XCHG_WEAK
#undef detail_CMP_XCHG_STRONG
