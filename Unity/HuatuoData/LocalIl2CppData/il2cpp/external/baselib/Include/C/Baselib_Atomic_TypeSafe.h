#pragma once

#include "Baselib_Atomic.h"

// TypeSafe version of baselib atomics "C" API

// 8-bit declarations
// ----------------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE int8_t Baselib_atomic_load_8_relaxed(const int8_t* obj);
static FORCE_INLINE int8_t Baselib_atomic_load_8_acquire(const int8_t* obj);
static FORCE_INLINE int8_t Baselib_atomic_load_8_seq_cst(const int8_t* obj);

static FORCE_INLINE void Baselib_atomic_store_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE void Baselib_atomic_store_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE void Baselib_atomic_store_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE int8_t Baselib_atomic_fetch_add_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_add_8_acquire(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_add_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_add_8_acq_rel(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_add_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE int8_t Baselib_atomic_fetch_and_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_and_8_acquire(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_and_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_and_8_acq_rel(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_and_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE int8_t Baselib_atomic_fetch_or_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_or_8_acquire(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_or_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_or_8_acq_rel(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_or_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE int8_t Baselib_atomic_fetch_xor_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_xor_8_acquire(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_xor_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_xor_8_acq_rel(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_fetch_xor_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE int8_t Baselib_atomic_exchange_8_relaxed(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_exchange_8_acquire(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_exchange_8_release(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_exchange_8_acq_rel(int8_t* obj, int8_t value);
static FORCE_INLINE int8_t Baselib_atomic_exchange_8_seq_cst(int8_t* obj, int8_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_relaxed_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acquire_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acquire_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_release_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acq_rel_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_acq_rel_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_8_seq_cst_seq_cst(int8_t* obj, int8_t* expected, int8_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_relaxed_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acquire_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acquire_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_release_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acq_rel_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_acq_rel_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_relaxed(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_acquire(int8_t* obj, int8_t* expected, int8_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_8_seq_cst_seq_cst(int8_t* obj, int8_t* expected, int8_t value);

// 16-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE int16_t Baselib_atomic_load_16_relaxed(const int16_t* obj);
static FORCE_INLINE int16_t Baselib_atomic_load_16_acquire(const int16_t* obj);
static FORCE_INLINE int16_t Baselib_atomic_load_16_seq_cst(const int16_t* obj);

static FORCE_INLINE void Baselib_atomic_store_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE void Baselib_atomic_store_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE void Baselib_atomic_store_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE int16_t Baselib_atomic_fetch_add_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_add_16_acquire(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_add_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_add_16_acq_rel(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_add_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE int16_t Baselib_atomic_fetch_and_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_and_16_acquire(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_and_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_and_16_acq_rel(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_and_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE int16_t Baselib_atomic_fetch_or_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_or_16_acquire(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_or_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_or_16_acq_rel(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_or_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE int16_t Baselib_atomic_fetch_xor_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_xor_16_acquire(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_xor_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_xor_16_acq_rel(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_fetch_xor_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE int16_t Baselib_atomic_exchange_16_relaxed(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_exchange_16_acquire(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_exchange_16_release(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_exchange_16_acq_rel(int16_t* obj, int16_t value);
static FORCE_INLINE int16_t Baselib_atomic_exchange_16_seq_cst(int16_t* obj, int16_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_relaxed_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acquire_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acquire_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_release_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acq_rel_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_acq_rel_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_16_seq_cst_seq_cst(int16_t* obj, int16_t* expected, int16_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_relaxed_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acquire_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acquire_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_release_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acq_rel_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_acq_rel_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_relaxed(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_acquire(int16_t* obj, int16_t* expected, int16_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_16_seq_cst_seq_cst(int16_t* obj, int16_t* expected, int16_t value);

// 32-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE int32_t Baselib_atomic_load_32_relaxed(const int32_t* obj);
static FORCE_INLINE int32_t Baselib_atomic_load_32_acquire(const int32_t* obj);
static FORCE_INLINE int32_t Baselib_atomic_load_32_seq_cst(const int32_t* obj);

static FORCE_INLINE void Baselib_atomic_store_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE void Baselib_atomic_store_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE void Baselib_atomic_store_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE int32_t Baselib_atomic_fetch_add_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_add_32_acquire(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_add_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_add_32_acq_rel(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_add_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE int32_t Baselib_atomic_fetch_and_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_and_32_acquire(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_and_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_and_32_acq_rel(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_and_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE int32_t Baselib_atomic_fetch_or_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_or_32_acquire(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_or_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_or_32_acq_rel(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_or_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE int32_t Baselib_atomic_fetch_xor_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_xor_32_acquire(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_xor_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_xor_32_acq_rel(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_fetch_xor_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE int32_t Baselib_atomic_exchange_32_relaxed(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_exchange_32_acquire(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_exchange_32_release(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_exchange_32_acq_rel(int32_t* obj, int32_t value);
static FORCE_INLINE int32_t Baselib_atomic_exchange_32_seq_cst(int32_t* obj, int32_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acquire_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_release_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acq_rel_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_acq_rel_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_32_seq_cst_seq_cst(int32_t* obj, int32_t* expected, int32_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_relaxed_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acquire_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acquire_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_release_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acq_rel_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_acq_rel_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_relaxed(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_acquire(int32_t* obj, int32_t* expected, int32_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_32_seq_cst_seq_cst(int32_t* obj, int32_t* expected, int32_t value);

// 64-bit declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE int64_t Baselib_atomic_load_64_relaxed(const int64_t* obj);
static FORCE_INLINE int64_t Baselib_atomic_load_64_acquire(const int64_t* obj);
static FORCE_INLINE int64_t Baselib_atomic_load_64_seq_cst(const int64_t* obj);

static FORCE_INLINE void Baselib_atomic_store_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE void Baselib_atomic_store_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE void Baselib_atomic_store_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE int64_t Baselib_atomic_fetch_add_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_add_64_acquire(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_add_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_add_64_acq_rel(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_add_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE int64_t Baselib_atomic_fetch_and_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_and_64_acquire(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_and_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_and_64_acq_rel(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_and_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE int64_t Baselib_atomic_fetch_or_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_or_64_acquire(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_or_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_or_64_acq_rel(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_or_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE int64_t Baselib_atomic_fetch_xor_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_xor_64_acquire(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_xor_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_xor_64_acq_rel(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_fetch_xor_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE int64_t Baselib_atomic_exchange_64_relaxed(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_exchange_64_acquire(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_exchange_64_release(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_exchange_64_acq_rel(int64_t* obj, int64_t value);
static FORCE_INLINE int64_t Baselib_atomic_exchange_64_seq_cst(int64_t* obj, int64_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_relaxed_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acquire_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acquire_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_release_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acq_rel_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_acq_rel_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_64_seq_cst_seq_cst(int64_t* obj, int64_t* expected, int64_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_relaxed_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acquire_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acquire_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_release_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acq_rel_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_acq_rel_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_relaxed(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_acquire(int64_t* obj, int64_t* expected, int64_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_64_seq_cst_seq_cst(int64_t* obj, int64_t* expected, int64_t value);

// ptr declarations
// ------------------------------------------------------------------------------------------------------------------------------
static FORCE_INLINE intptr_t Baselib_atomic_load_ptr_relaxed(const intptr_t* obj);
static FORCE_INLINE intptr_t Baselib_atomic_load_ptr_acquire(const intptr_t* obj);
static FORCE_INLINE intptr_t Baselib_atomic_load_ptr_seq_cst(const intptr_t* obj);

static FORCE_INLINE void Baselib_atomic_store_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE void Baselib_atomic_store_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE void Baselib_atomic_store_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE intptr_t Baselib_atomic_fetch_add_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_add_ptr_acquire(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_add_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_add_ptr_acq_rel(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_add_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE intptr_t Baselib_atomic_fetch_and_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_and_ptr_acquire(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_and_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_and_ptr_acq_rel(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_and_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE intptr_t Baselib_atomic_fetch_or_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_or_ptr_acquire(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_or_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_or_ptr_acq_rel(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_or_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE intptr_t Baselib_atomic_fetch_xor_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_xor_ptr_acquire(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_xor_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_xor_ptr_acq_rel(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_fetch_xor_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE intptr_t Baselib_atomic_exchange_ptr_relaxed(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_exchange_ptr_acquire(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_exchange_ptr_release(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_exchange_ptr_acq_rel(intptr_t* obj, intptr_t value);
static FORCE_INLINE intptr_t Baselib_atomic_exchange_ptr_seq_cst(intptr_t* obj, intptr_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_relaxed_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acquire_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acquire_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_release_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acq_rel_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_acq_rel_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_weak_ptr_seq_cst_seq_cst(intptr_t* obj, intptr_t* expected, intptr_t value);

static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_relaxed_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acquire_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acquire_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_release_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acq_rel_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_acq_rel_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_relaxed(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_acquire(intptr_t* obj, intptr_t* expected, intptr_t value);
static FORCE_INLINE bool Baselib_atomic_compare_exchange_strong_ptr_seq_cst_seq_cst(intptr_t* obj, intptr_t* expected, intptr_t value);

// Macro Implementation
// ------------------------------------------------------------------------------------------------------------------------------

#define detail_LOAD(op, order, id , bits, int_type, ...)                                                                        \
static FORCE_INLINE int_type Baselib_atomic_##op##_##id##_##order(const int_type* obj)                                          \
{                                                                                                                               \
    int_type result;                                                                                                            \
    Baselib_atomic_##op##_##bits##_##order##_v(obj, &result);                                                                       \
    return result;                                                                                                              \
}

#define detail_STORE(op, order, id , bits, int_type, ...)                                                                       \
static FORCE_INLINE void Baselib_atomic_##op##_##id##_##order(int_type* obj, int_type value)                                  \
{                                                                                                                               \
    Baselib_atomic_##op##_##bits##_##order##_v(obj, &value);                                                                        \
}

#define detail_LOAD_STORE(op, order, id , bits, int_type, ...)                                                                  \
static FORCE_INLINE int_type Baselib_atomic_##op##_##id##_##order(int_type* obj, int_type value)                              \
{                                                                                                                               \
    int_type result;                                                                                                            \
    Baselib_atomic_##op##_##bits##_##order##_v(obj, &value, &result);                                                                        \
    return result;                                                                                                              \
}

#define detail_CMP_XCHG(op, order1, order2, id , bits, int_type, ...)                                                           \
static FORCE_INLINE bool Baselib_atomic_##op##_##id##_##order1##_##order2(int_type* obj, int_type* expected, int_type value)  \
{                                                                                                                               \
    return Baselib_atomic_##op##_##bits##_##order1##_##order2##_v(obj, expected, &value);                      \
}

Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_TYPE2(detail_LOAD, detail_STORE, detail_LOAD_STORE, detail_CMP_XCHG);

#undef detail_LOAD
#undef detail_STORE
#undef detail_LOAD_STORE
#undef detail_CMP_XCHG
