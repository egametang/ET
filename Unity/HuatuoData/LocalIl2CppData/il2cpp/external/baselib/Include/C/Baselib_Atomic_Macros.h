#pragma once

#include "Baselib_Alignment.h"

//
// order            - relaxed, acquire, release, acq_rel, seq_cst
//
// MACRO_(order, ...)
//
#define Baselib_Atomic_FOR_EACH_MEMORY_ORDER(MACRO_, ...)                           \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(relaxed, __VA_ARGS__))                       \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(acquire, __VA_ARGS__))                       \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(release, __VA_ARGS__))                       \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(acq_rel, __VA_ARGS__))                       \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(seq_cst, __VA_ARGS__))

//
// operation        - load, store, fetch_add, fetch_and, fetch_or, fetch_xor, exchange, compare_exchange_weak, compare_exchange_strong
// order            - relaxed, acquire, release, acq_rel, seq_cst
// order_success    - relaxed, acquire, release, acq_rel, seq_cst
// order_failure    - relaxed, acquire, seq_cst
//
// LOAD_MACRO_(operation, order, ...)
// STORE_MACRO_(operation, order, ...)
// ADD_MACRO_(operation, order, ...)
// AND_MACRO_(operation, order, ...)
// OR_MACRO_(operation, order, ...)
// XOR_MACRO_(operation, order, ...)
// XCHG_MACRO_(operation, order, ...)
// CMP_XCHG_WEAK_MACRO_(operation, order_success, order_failure, ...)
// CMP_XCHG_STRONG_MACRO_(operation, order_success, order_failure, ...)
//
#define Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, ...) \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_MEMORY_ORDER(LOAD_MACRO_, load, __VA_ARGS__)                                   \
    DETAIL__Baselib_Atomic_FOR_EACH_STORE_MEMORY_ORDER(STORE_MACRO_, store, __VA_ARGS__)                                \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(ADD_MACRO_, fetch_add, __VA_ARGS__)                         \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(AND_MACRO_, fetch_and, __VA_ARGS__)                         \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(OR_MACRO_, fetch_or, __VA_ARGS__)                           \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(XOR_MACRO_, fetch_xor, __VA_ARGS__)                         \
    DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(XCHG_MACRO_, exchange, __VA_ARGS__)                         \
    DETAIL__Baselib_Atomic_FOR_EACH_CMP_XCHG_MEMORY_ORDER(CMP_XCHG_WEAK_MACRO_, compare_exchange_weak, __VA_ARGS__)     \
    DETAIL__Baselib_Atomic_FOR_EACH_CMP_XCHG_MEMORY_ORDER(CMP_XCHG_STRONG_MACRO_, compare_exchange_strong, __VA_ARGS__)

//
// LOAD_MACRO_(operation, order, ...)
// STORE_MACRO_(operation, order, ...)
// LOAD_STORE_MACRO_(operation, order, ...)
// CMP_XCHG_MACRO_(operation, order_success, order_failure, ...)
//
#define Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER2(LOAD_MACRO_, STORE_MACRO_, LOAD_STORE_MACRO_, CMP_XCHG_MACRO_, ...) \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(                             \
        LOAD_MACRO_,                                                                \
        STORE_MACRO_,                                                               \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        CMP_XCHG_MACRO_,                                                            \
        CMP_XCHG_MACRO_,                                                            \
        __VA_ARGS__)

//
// operation        - load, store, fetch_add, fetch_and, fetch_or, fetch_xor, exchange, compare_exchange_weak, compare_exchange_strong
// order            - relaxed, acquire, release, acq_rel, seq_cst
// order_success    - relaxed, acquire, release, acq_rel, seq_cst
// order_failure    - relaxed, acquire, seq_cst
// id               - 8, 16, 32, 64
// bits             - 8, 16, 32, 64
// int_type         - int8_t, int16_t, int32_t, int64_t
//
// LOAD_MACRO_(operation, order, id, bits, int_type, ...)
// STORE_MACRO_(operation, order, id, bits, int_type, ...)
// ADD_MACRO_(operation, order, id, bits, int_type, ...)
// AND_MACRO_(operation, order, id, bits, int_type, ...)
// OR_MACRO_(operation, order, id, bits, int_type, ...)
// XOR_MACRO_(operation, order, id, bits, int_type, ...)
// XCHG_MACRO_(operation, order, id, bits, int_type, ...)
// CMP_XCHG_WEAK_MACRO_(operation, order_success, order_failure, id , bits, int_type, ...)
// CMP_XCHG_STRONG_MACRO_(operation, order_success, order_failure, id , bits, int_type, ...)
//
#define Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_INT_TYPE(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, ...)               \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, 8, 8, int8_t __VA_ARGS__)       \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, 16, 16, int16_t, __VA_ARGS__)   \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, 32, 32, int32_t, __VA_ARGS__)   \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, 64, 64, int64_t, __VA_ARGS__)

//
// operation        - load, store, fetch_add, fetch_and, fetch_or, fetch_xor, exchange, compare_exchange_weak, compare_exchange_strong
// order            - relaxed, acquire, release, acq_rel, seq_cst
// order_success    - relaxed, acquire, release, acq_rel, seq_cst
// order_failure    - relaxed, acquire, seq_cst
// id               - 8, 16, 32, 64, ptr
// bits             - 8, 16, 32, 64
// int_type         - int8_t, int16_t, int32_t, int64_t, intptr_t
//
// LOAD_MACRO_(operation, order, id, bits, int_type, ...)
// STORE_MACRO_(operation, order, id, bits, int_type, ...)
// ADD_MACRO_(operation, order, id, bits, int_type, ...)
// AND_MACRO_(operation, order, id, bits, int_type, ...)
// OR_MACRO_(operation, order, id, bits, int_type, ...)
// XOR_MACRO_(operation, order, id, bits, int_type, ...)
// XCHG_MACRO_(operation, order, id, bits, int_type, ...)
// CMP_XCHG_WEAK_MACRO_(operation, order_success, order_failure, id , bits, int_type, ...)
// CMP_XCHG_STRONG_MACRO_(operation, order_success, order_failure, id , bits, int_type, ...)
//
#define Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, ...)         \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_INT_TYPE(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, __VA_ARGS__) \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(LOAD_MACRO_, STORE_MACRO_, ADD_MACRO_, AND_MACRO_, OR_MACRO_, XOR_MACRO_, XCHG_MACRO_, CMP_XCHG_WEAK_MACRO_, CMP_XCHG_STRONG_MACRO_, ptr, DETAIL__Baselib_Atomic_PTR_SIZE, intptr_t, __VA_ARGS__)

//
// LOAD_MACRO_(operation, order, id, bits, int_type, ...)
// STORE_MACRO_(operation, order, id, bits, int_type, ...)
// LOAD_STORE_MACRO_(operation, order, id, bits, int_type, ...)
// CMP_XCHG_MACRO_(operation, order_success, order_failure, id , bits, int_type, ...)
//
#define Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE2(LOAD_MACRO_, STORE_MACRO_, LOAD_STORE_MACRO_, CMP_XCHG_MACRO_, ...) \
    Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE(                        \
        LOAD_MACRO_,                                                                \
        STORE_MACRO_,                                                               \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        LOAD_STORE_MACRO_,                                                          \
        CMP_XCHG_MACRO_,                                                            \
        CMP_XCHG_MACRO_,                                                            \
        __VA_ARGS__)

//
// Implementation details
// ----------------------------------------------------------------------------------
#if PLATFORM_ARCH_64
    #define DETAIL__Baselib_Atomic_PTR_SIZE 64
#else
    #define DETAIL__Baselib_Atomic_PTR_SIZE 32
#endif

#define DETAIL__Baselib_Atomic_EVAL(...)  __VA_ARGS__

#define DETAIL__Baselib_Atomic_FOR_EACH_LOAD_MEMORY_ORDER(MACRO_, OP_, ...)         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, relaxed, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acquire, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, __VA_ARGS__))

#define DETAIL__Baselib_Atomic_FOR_EACH_STORE_MEMORY_ORDER(MACRO_, OP_, ...)        \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, relaxed, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, release, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, __VA_ARGS__))

#define DETAIL__Baselib_Atomic_FOR_EACH_LOAD_STORE_MEMORY_ORDER(MACRO_, OP_, ...)   \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, relaxed, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acquire, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, release, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acq_rel, __VA_ARGS__))                  \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, __VA_ARGS__))

#define DETAIL__Baselib_Atomic_FOR_EACH_CMP_XCHG_MEMORY_ORDER(MACRO_, OP_, ...)     \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, relaxed, relaxed, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acquire, relaxed, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acquire, acquire, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, release, relaxed, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acq_rel, relaxed, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, acq_rel, acquire, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, relaxed, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, acquire, __VA_ARGS__))         \
    DETAIL__Baselib_Atomic_EVAL(MACRO_(OP_, seq_cst, seq_cst, __VA_ARGS__))
