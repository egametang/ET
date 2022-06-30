#pragma once

// This API is not type safe. For a type safe version use Baselib_Atomic_TypeSafe.h (C) or Atomic.h (C++)
//
// Atomics closely mimic C11/C++11 implementation, with the following differences:
//
//   *) C API: as Visual Studio C compiler doesn't support _Generic we can't have a single named function operating on different types, or
//      selecting different implementations depending on memory order.
//      This leads to having to explicitly specify type size and ordering in the function name, for example
//      'Baselib_atomic_load_32_acquire' instead of 'Baselib_atomic_load' as one would have available in in C11 or equivalent in C++11.
//

// not type specific
// ----------------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE void Baselib_atomic_thread_fence_relaxed(void);
static FORCE_INLINE void Baselib_atomic_thread_fence_acquire(void);
static FORCE_INLINE void Baselib_atomic_thread_fence_release(void);
static FORCE_INLINE void Baselib_atomic_thread_fence_acq_rel(void);
static FORCE_INLINE void Baselib_atomic_thread_fence_seq_cst(void);

static FORCE_INLINE void Baselib_atomic_load_8_relaxed_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_8_acquire_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_8_seq_cst_v(const void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_8_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_8_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_8_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_fetch_add_8_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_8_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_8_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_8_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_8_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_and_8_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_8_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_8_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_8_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_8_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_or_8_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_8_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_8_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_8_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_8_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_xor_8_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_8_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_8_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_8_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_8_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_exchange_8_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_8_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_8_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_8_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_8_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// 16-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE void Baselib_atomic_load_16_relaxed_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_16_acquire_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_16_seq_cst_v(const void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_16_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_16_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_16_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_fetch_add_16_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_16_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_16_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_16_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_16_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_and_16_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_16_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_16_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_16_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_16_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_or_16_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_16_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_16_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_16_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_16_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_xor_16_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_16_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_16_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_16_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_16_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_exchange_16_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_16_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_16_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_16_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_16_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// 32-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE void Baselib_atomic_load_32_relaxed_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_32_acquire_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_32_seq_cst_v(const void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_32_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_32_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_32_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_fetch_add_32_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_32_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_32_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_32_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_32_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_and_32_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_32_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_32_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_32_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_32_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_or_32_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_32_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_32_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_32_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_32_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_xor_32_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_32_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_32_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_32_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_32_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_exchange_32_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_32_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_32_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_32_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_32_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// 64-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE void Baselib_atomic_load_64_relaxed_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_64_acquire_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_64_seq_cst_v(const void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_64_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_64_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_64_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_fetch_add_64_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_64_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_64_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_64_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_64_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_and_64_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_64_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_64_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_64_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_64_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_or_64_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_64_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_64_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_64_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_64_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_xor_64_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_64_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_64_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_64_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_64_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_exchange_64_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_64_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_64_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_64_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_64_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// 128-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
#if PLATFORM_ARCH_64

// commented out const:
// 128bit loads are guranteed to not change obj but may need a store to confirm atomicity
static FORCE_INLINE void Baselib_atomic_load_128_relaxed_v(/* const */ void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_128_acquire_v(/* const */ void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_128_seq_cst_v(/* const */ void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_128_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_128_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_128_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_exchange_128_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_128_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_128_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_128_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_128_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_128_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_128_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

#endif

// ptr declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE void Baselib_atomic_load_ptr_relaxed_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_ptr_acquire_v(const void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_ptr_seq_cst_v(const void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_ptr_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_ptr_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_ptr_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_fetch_add_ptr_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_ptr_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_ptr_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_ptr_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_add_ptr_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_and_ptr_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_ptr_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_ptr_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_ptr_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_and_ptr_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_or_ptr_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_ptr_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_ptr_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_ptr_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_or_ptr_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_fetch_xor_ptr_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_ptr_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_ptr_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_ptr_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_fetch_xor_ptr_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE void Baselib_atomic_exchange_ptr_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// ptr2x declarations
// ------------------------------------------------------------------------------------------------------------------------------

// commented out const:
// 128bit loads are guranteed to not change obj but may need a store to confirm atomicity
static FORCE_INLINE void Baselib_atomic_load_ptr2x_relaxed_v(/* const */ void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_ptr2x_acquire_v(/* const */ void* obj, void* result);
static FORCE_INLINE void Baselib_atomic_load_ptr2x_seq_cst_v(/* const */ void* obj, void* result);

static FORCE_INLINE void Baselib_atomic_store_ptr2x_relaxed_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_ptr2x_release_v(void* obj, const void* value);
static FORCE_INLINE void Baselib_atomic_store_ptr2x_seq_cst_v(void* obj, const void* value);

static FORCE_INLINE void Baselib_atomic_exchange_ptr2x_relaxed_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr2x_acquire_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr2x_release_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr2x_acq_rel_v(void* obj, const void* value, void* result);
static FORCE_INLINE void Baselib_atomic_exchange_ptr2x_seq_cst_v(void* obj, const void* value, void* result);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr2x_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_relaxed_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_acquire_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_acquire_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_release_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_acq_rel_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_acq_rel_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_seq_cst_relaxed_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_seq_cst_acquire_v(void* obj, void* expected, const void* value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr2x_seq_cst_seq_cst_v(void* obj, void* expected, const void* value);

// Compiler Specific Implementation
// ----------------------------------------------------------------------------------------------------------------------------------------

#if PLATFORM_CUSTOM_ATOMICS
// Platform header does not know where macro header lives and likely needs it.
    #include "../../Include/C/Baselib_Atomic_Macros.h"
    #include "C/Baselib_Atomic_Platform.inl.h"
#elif COMPILER_CLANG || COMPILER_GCC
    #include "Internal/Compiler/Baselib_Atomic_Gcc.h"
#elif COMPILER_MSVC
    #include "Internal/Compiler/Baselib_Atomic_Msvc.h"
#endif
