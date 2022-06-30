#pragma once

#include "il2cpp-config-platforms.h"
#include <stdint.h>

#include "Baselib.h"
#include "C/Baselib_Atomic_TypeSafe.h"

inline void UnityPalFullMemoryBarrier()
{
    Baselib_atomic_thread_fence_seq_cst();
}

inline int32_t UnityPalAdd(int32_t* location1, int32_t value)
{
    return Baselib_atomic_fetch_add_32_seq_cst(location1, value) + value;
}

#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
inline int64_t UnityPalAdd64(int64_t* location1, int64_t value)
{
    return Baselib_atomic_fetch_add_64_seq_cst(location1, value) + value;
}

#endif

inline int32_t UnityPalIncrement(int32_t* value)
{
    return Baselib_atomic_fetch_add_32_seq_cst(value, 1) + 1;
}

#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
inline int64_t UnityPalIncrement64(int64_t* value)
{
    return Baselib_atomic_fetch_add_64_seq_cst(value, 1) + 1;
}

#endif

inline int32_t UnityPalDecrement(int32_t* value)
{
    return Baselib_atomic_fetch_add_32_seq_cst(value, -1) - 1;
}

#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
inline int64_t UnityPalDecrement64(int64_t* value)
{
    return Baselib_atomic_fetch_add_64_seq_cst(value, -1) - 1;
}

#endif

inline int32_t UnityPalCompareExchange(int32_t* dest, int32_t exchange, int32_t comparand)
{
    Baselib_atomic_compare_exchange_strong_32_seq_cst_seq_cst(dest, &comparand, exchange);
    return comparand;
}

inline int64_t UnityPalCompareExchange64(int64_t* dest, int64_t exchange, int64_t comparand)
{
    Baselib_atomic_compare_exchange_strong_64_seq_cst_seq_cst(dest, &comparand, exchange);
    return comparand;
}

inline void* UnityPalCompareExchangePointer(void* volatile* dest, void* exchange, void* comparand)
{
    Baselib_atomic_compare_exchange_strong_ptr_seq_cst_seq_cst((intptr_t*)dest, (intptr_t*)&comparand, (intptr_t)exchange);
    return comparand;
}

inline int32_t UnityPalExchange(int32_t* dest, int32_t exchange)
{
    return Baselib_atomic_exchange_32_seq_cst(dest, exchange);
}

#if IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT
inline int64_t UnityPalExchange64(int64_t* dest, int64_t exchange)
{
    return Baselib_atomic_exchange_64_seq_cst(dest, exchange);
}

#endif

inline void* UnityPalExchangePointer(void* volatile* dest, void* exchange)
{
    return (void*)Baselib_atomic_exchange_ptr_seq_cst((intptr_t*)dest, (intptr_t)exchange);
}

inline int64_t UnityPalRead64(int64_t* addr)
{
    return Baselib_atomic_fetch_add_64_seq_cst(addr, 0);
}

inline intptr_t UnityPalReadPtrVal(intptr_t* addr)
{
    return Baselib_atomic_fetch_add_ptr_seq_cst(addr, 0);
}

inline int32_t UnityPalLoadRelaxed(const int32_t* addr)
{
    return Baselib_atomic_load_32_relaxed(addr);
}
