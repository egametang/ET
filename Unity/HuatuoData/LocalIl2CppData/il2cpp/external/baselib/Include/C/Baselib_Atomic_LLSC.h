#pragma once

// In computer science, load-link and store-conditional (LL/SC) are a pair of instructions used in multithreading to achieve synchronization.
// Load-link returns the current value of a memory location, while a subsequent store-conditional to the same memory location will store a new
// value only if no updates have occurred to that location since the load-link. Together, this implements a lock-free atomic read-modify-write operation.
//
// Comparison of LL/SC and compare-and-swap
//  If any updates have occurred, the store-conditional is guaranteed to fail, even if the value read by the load-link has since been restored.
//  As such, an LL/SC pair is stronger than a read followed by a compare-and-swap (CAS), which will not detect updates if the old value has been restored
//  (see ABA problem).
//
// "Load-link/store-conditional", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Load-link/store-conditional&oldid=916413430
//

//
// Baselib_atomic_llsc_break
//
// This is has no functional effect, but can improve performance on some Arm architectures
//
// Example:
//  Baselib_atomic_llsc_32_relaxed_relaxed_v(&obj, &expected, &value, { if (expected == 0) { Baselib_atomic_llsc_break(); break; } );
//
#define Baselib_atomic_llsc_break() detail_Baselib_atomic_llsc_break()

//
// Baselib_atomic_llsc_<int_type>_<load order>_<store_order>_v(obj, expected, value, code)
//
// int_type     - 8, 16, 32, 64, ptr, ptr2x, 128 (128 available only on 64-bit architectures)
// load_order   - relaxed, acquire, seq_cst
// store_order  - relaxed, release, seq_cst
//
// obj          - address to memory to store 'value' into.
//                  Must be cache-line size aligned and sized. Any update of this memory between the LL/SC pair results in unpredictable behaviour.
// expected     - address to memory to load 'obj' into.
//                  Loaded by LL. Any updates of this memory between the LL/SC pair results in unpredictable behaviour.
// value        - address to memory containing value to store into 'obj':
//                  Stored by SC to 'obj' memory on success, otherwise 'code' is repeated.
// code         - code executed between the LL/SC pair.
//
// Notes on Arm optimized clang implementation:
//  Armv7A and Armv8A architectures are enabled by default. Newer architectures will be enabled once tested and verified compliant.
//  Specifically, the configuration of the exclusive access global/local monitors such as ERG (Exclusives Reservation Granule) size may vary on other platforms.
//  See Arm Synchronization Primitives: http://infocenter.arm.com/help/topic/com.arm.doc.dht0008a/DHT0008A_arm_synchronization_primitives.pdf
//  chapter 1.2 "Exclusive accesses" for more detailed information.
//
// Notes on default implementation (platforms/architectures not listed in the Arm clang notes)
//  Atomic load and compare_exchange intrinsics emulates LL/SC capability.
//  The values of 'expected' and 'obj' value to determine if SC should succeed and store 'value'.
//
// Example:
//  struct Data { BASELIB_ALIGN_AS(PLATFORM_CACHE_LINE_SIZE) int32_t obj = 0; } data;
//  int32_t expected = 1, value = 2;
//  Baselib_atomic_llsc_32_relaxed_relaxed_v(&data.obj, &expected, &value, { if (expected == 0) value = 3; } );
//  <-- obj is now 3
//
#define Baselib_atomic_llsc_8_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 8, relaxed, relaxed)
#define Baselib_atomic_llsc_8_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 8, acquire, relaxed)
#define Baselib_atomic_llsc_8_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 8, relaxed, release)
#define Baselib_atomic_llsc_8_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 8, acquire, release)
#define Baselib_atomic_llsc_8_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 8, seq_cst, seq_cst)

#define Baselib_atomic_llsc_16_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 16, relaxed, relaxed)
#define Baselib_atomic_llsc_16_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 16, acquire, relaxed)
#define Baselib_atomic_llsc_16_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 16, relaxed, release)
#define Baselib_atomic_llsc_16_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 16, acquire, release)
#define Baselib_atomic_llsc_16_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 16, seq_cst, seq_cst)

#define Baselib_atomic_llsc_32_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 32, relaxed, relaxed)
#define Baselib_atomic_llsc_32_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 32, acquire, relaxed)
#define Baselib_atomic_llsc_32_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 32, relaxed, release)
#define Baselib_atomic_llsc_32_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 32, acquire, release)
#define Baselib_atomic_llsc_32_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 32, seq_cst, seq_cst)

#define Baselib_atomic_llsc_64_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 64, relaxed, relaxed)
#define Baselib_atomic_llsc_64_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 64, acquire, relaxed)
#define Baselib_atomic_llsc_64_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 64, relaxed, release)
#define Baselib_atomic_llsc_64_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 64, acquire, release)
#define Baselib_atomic_llsc_64_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, 64, seq_cst, seq_cst)

#define Baselib_atomic_llsc_ptr_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, ptr, relaxed, relaxed)
#define Baselib_atomic_llsc_ptr_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, ptr, acquire, relaxed)
#define Baselib_atomic_llsc_ptr_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, ptr, relaxed, release)
#define Baselib_atomic_llsc_ptr_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, ptr, acquire, release)
#define Baselib_atomic_llsc_ptr_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_v(obj, expected, value, code, ptr, seq_cst, seq_cst)

#if PLATFORM_ARCH_64

#define Baselib_atomic_llsc_ptr2x_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, relaxed, relaxed)
#define Baselib_atomic_llsc_ptr2x_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, acquire, relaxed)
#define Baselib_atomic_llsc_ptr2x_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, relaxed, release)
#define Baselib_atomic_llsc_ptr2x_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, acquire, release)
#define Baselib_atomic_llsc_ptr2x_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, seq_cst, seq_cst)

#define Baselib_atomic_llsc_128_relaxed_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, relaxed, relaxed)
#define Baselib_atomic_llsc_128_acquire_relaxed_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, acquire, relaxed)
#define Baselib_atomic_llsc_128_relaxed_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, relaxed, release)
#define Baselib_atomic_llsc_128_acquire_release_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, acquire, release)
#define Baselib_atomic_llsc_128_seq_cst_seq_cst_v(obj, expected, value, code) detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, seq_cst, seq_cst)

#else // PLATFORM_ARCH_64

#define Baselib_atomic_llsc_ptr2x_relaxed_relaxed_v(obj, expected, value, code) Baselib_atomic_llsc_64_relaxed_relaxed_v(obj, expected, value, code)
#define Baselib_atomic_llsc_ptr2x_acquire_relaxed_v(obj, expected, value, code) Baselib_atomic_llsc_64_acquire_relaxed_v(obj, expected, value, code)
#define Baselib_atomic_llsc_ptr2x_relaxed_release_v(obj, expected, value, code) Baselib_atomic_llsc_64_relaxed_release_v(obj, expected, value, code)
#define Baselib_atomic_llsc_ptr2x_acquire_release_v(obj, expected, value, code) Baselib_atomic_llsc_64_acquire_release_v(obj, expected, value, code)
#define Baselib_atomic_llsc_ptr2x_seq_cst_seq_cst_v(obj, expected, value, code) Baselib_atomic_llsc_64_seq_cst_seq_cst_v(obj, expected, value, code)

#endif

// Enable LLSC native support for supported compilers and architectures/profiles
#ifndef PLATFORM_LLSC_NATIVE_SUPPORT
    #if (COMPILER_CLANG) && ((__ARM_ARCH >= 7) && (__ARM_ARCH < 9) && (__ARM_ARCH_PROFILE == 'A'))
        #define PLATFORM_LLSC_NATIVE_SUPPORT 1
    #else
        #define PLATFORM_LLSC_NATIVE_SUPPORT 0
    #endif
#endif

#if PLATFORM_LLSC_NATIVE_SUPPORT
// Arm specific implementation of LLSC macros
#include "Internal/Compiler/Baselib_Atomic_LLSC_Gcc.inl.h"
#else
// Generic implementation of LLSC macros
#include "Baselib_Atomic.h"

// LLSC exlusive state access break implementation (nop)
#define detail_Baselib_atomic_llsc_break()

// LLSC implementation using load/cmp_xcgh
#define detail_Baselib_atomic_llsc_cmpxchg_v(obj, expected, value, code, size, loadbarrier, storebarrier)                   \
    do {                                                                                                                    \
        Baselib_atomic_load_##size##_##loadbarrier##_v(obj, expected);                                                      \
        do {                                                                                                                \
            code;                                                                                                           \
        } while (!Baselib_atomic_compare_exchange_weak_##size##_##storebarrier##_##loadbarrier##_v(obj, expected, value));  \
    } while (false)

#define detail_Baselib_atomic_llsc_relaxed_relaxed_v(obj, expected, value, code, size) detail_Baselib_atomic_llsc_cmpxchg_v( obj, expected, value, code, size, relaxed, relaxed)
#define detail_Baselib_atomic_llsc_acquire_relaxed_v(obj, expected, value, code, size) detail_Baselib_atomic_llsc_cmpxchg_v( obj, expected, value, code, size, acquire, acquire)
#define detail_Baselib_atomic_llsc_relaxed_release_v(obj, expected, value, code, size) detail_Baselib_atomic_llsc_cmpxchg_v( obj, expected, value, code, size, relaxed, release)
#define detail_Baselib_atomic_llsc_acquire_release_v(obj, expected, value, code, size) detail_Baselib_atomic_llsc_cmpxchg_v( obj, expected, value, code, size, acquire, acq_rel)
#define detail_Baselib_atomic_llsc_seq_cst_seq_cst_v(obj, expected, value, code, size) detail_Baselib_atomic_llsc_cmpxchg_v( obj, expected, value, code, size, seq_cst, seq_cst)

#define detail_Baselib_atomic_llsc_v(obj, expected, value, code, size, loadbarrier, storebarrier) \
    detail_Baselib_atomic_llsc_##loadbarrier##_##storebarrier##_v(obj, expected, value, code, size)

#define detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, loadbarrier, storebarrier) \
    detail_Baselib_atomic_llsc_v(obj, expected, value, code, 128, loadbarrier, storebarrier)

#endif // PLATFORM_LLSC_NATIVE_SUPPORT
