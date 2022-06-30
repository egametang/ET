#pragma once

#include <intrin.h>

#ifndef _ARM_BARRIER_ISH
    #define _ARM_BARRIER_ISH 0xB
#endif

#define _InterlockedCompareExchange32(obj, value, exp)      _InterlockedCompareExchange((long*)obj, value, exp)
#define _InterlockedCompareExchange32_nf(obj, value, exp)   _InterlockedCompareExchange_nf((long*)obj, value, exp)
#define _InterlockedCompareExchange32_acq(obj, value, exp)  _InterlockedCompareExchange_acq((long*)obj, value, exp)
#define _InterlockedCompareExchange32_rel(obj, value, exp)  _InterlockedCompareExchange_rel((long*)obj, value, exp)
#define _InterlockedExchange32(obj, value)                  _InterlockedExchange((long*)obj, value)
#define _InterlockedExchange32_nf(obj, value)               _InterlockedExchange_nf((long*)obj, value)
#define _InterlockedExchange32_acq(obj, value)              _InterlockedExchange_acq((long*)obj, value)
#define _InterlockedExchange32_rel(obj, value)              _InterlockedExchange_rel((long*)obj, value)
#define _InterlockedExchangeAdd32(obj, value)               _InterlockedExchangeAdd((long*)obj, value)
#define _InterlockedExchangeAdd32_nf(obj, value)            _InterlockedExchangeAdd_nf((long*)obj, value)
#define _InterlockedExchangeAdd32_acq(obj, value)           _InterlockedExchangeAdd_acq((long*)obj, value)
#define _InterlockedExchangeAdd32_rel(obj, value)           _InterlockedExchangeAdd_rel((long*)obj, value)
#define _InterlockedAnd32(obj, value)                       _InterlockedAnd((long*)obj, value)
#define _InterlockedAnd32_nf(obj, value)                    _InterlockedAnd_nf((long*)obj, value)
#define _InterlockedAnd32_acq(obj, value)                   _InterlockedAnd_acq((long*)obj, value)
#define _InterlockedAnd32_rel(obj, value)                   _InterlockedAnd_rel((long*)obj, value)
#define _InterlockedOr32(obj, value)                        _InterlockedOr((long*)obj, value)
#define _InterlockedOr32_nf(obj, value)                     _InterlockedOr_nf((long*)obj, value)
#define _InterlockedOr32_acq(obj, value)                    _InterlockedOr_acq((long*)obj, value)
#define _InterlockedOr32_rel(obj, value)                    _InterlockedOr_rel((long*)obj, value)
#define _InterlockedXor32(obj, value)                       _InterlockedXor((long*)obj, value)
#define _InterlockedXor32_nf(obj, value)                    _InterlockedXor_nf((long*)obj, value)
#define _InterlockedXor32_acq(obj, value)                   _InterlockedXor_acq((long*)obj, value)
#define _InterlockedXor32_rel(obj, value)                   _InterlockedXor_rel((long*)obj, value)

// Use cmp_xchg on x86 to emulate 64 bit exchange and alu ops
#if defined(_M_IX86)

#undef _InterlockedExchange64
#undef _InterlockedExchangeAdd64
#undef _InterlockedOr64
#undef _InterlockedAnd64
#undef _InterlockedXor64

#define detail_CAS_OP(_name, ...)                                                                       \
static __forceinline __int64 _name(__int64* obj, __int64 value)                                         \
{                                                                                                       \
    __int64 p1, p2 = *obj;                                                                              \
    do { p1 = p2; p2 = _InterlockedCompareExchange64(obj, (__VA_ARGS__), p1); } while (p1 != p2);       \
    return p1;                                                                                          \
}

detail_CAS_OP(_InterlockedExchange64, value);
detail_CAS_OP(_InterlockedExchangeAdd64, p1 + value);
detail_CAS_OP(_InterlockedOr64, p1 | value);
detail_CAS_OP(_InterlockedAnd64, p1 & value);
detail_CAS_OP(_InterlockedXor64, p1 ^ value);
#undef detail_CAS_OP

#endif
