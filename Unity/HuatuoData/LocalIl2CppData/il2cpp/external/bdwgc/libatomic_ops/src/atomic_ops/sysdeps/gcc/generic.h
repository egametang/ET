/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 2003-2011 Hewlett-Packard Development Company, L.P.
 *
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose, provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 */

/* The following implementation assumes GCC 4.7 or later.               */
/* For the details, see GNU Manual, chapter 6.52 (Built-in functions    */
/* for memory model aware atomic operations).                           */

/* TODO: Include this file for other targets if gcc 4.7+ */

#ifdef AO_UNIPROCESSOR
  /* If only a single processor (core) is used, AO_UNIPROCESSOR could   */
  /* be defined by the client to avoid unnecessary memory barrier.      */
  AO_INLINE void
  AO_nop_full(void)
  {
    AO_compiler_barrier();
  }
# define AO_HAVE_nop_full

#else
  AO_INLINE void
  AO_nop_read(void)
  {
    __atomic_thread_fence(__ATOMIC_ACQUIRE);
  }
# define AO_HAVE_nop_read

# ifndef AO_HAVE_nop_write
    AO_INLINE void
    AO_nop_write(void)
    {
      __atomic_thread_fence(__ATOMIC_RELEASE);
    }
#   define AO_HAVE_nop_write
# endif

  AO_INLINE void
  AO_nop_full(void)
  {
    /* __sync_synchronize() could be used instead.      */
    __atomic_thread_fence(__ATOMIC_SEQ_CST);
  }
# define AO_HAVE_nop_full
#endif /* !AO_UNIPROCESSOR */

#include "generic-small.h"

#ifndef AO_PREFER_GENERALIZED
# include "generic-arithm.h"

  AO_INLINE AO_TS_VAL_t
  AO_test_and_set(volatile AO_TS_t *addr)
  {
    return (AO_TS_VAL_t)__atomic_test_and_set(addr, __ATOMIC_RELAXED);
  }
# define AO_HAVE_test_and_set

  AO_INLINE AO_TS_VAL_t
  AO_test_and_set_acquire(volatile AO_TS_t *addr)
  {
    return (AO_TS_VAL_t)__atomic_test_and_set(addr, __ATOMIC_ACQUIRE);
  }
# define AO_HAVE_test_and_set_acquire

  AO_INLINE AO_TS_VAL_t
  AO_test_and_set_release(volatile AO_TS_t *addr)
  {
    return (AO_TS_VAL_t)__atomic_test_and_set(addr, __ATOMIC_RELEASE);
  }
# define AO_HAVE_test_and_set_release

  AO_INLINE AO_TS_VAL_t
  AO_test_and_set_full(volatile AO_TS_t *addr)
  {
    return (AO_TS_VAL_t)__atomic_test_and_set(addr, __ATOMIC_SEQ_CST);
  }
# define AO_HAVE_test_and_set_full
#endif /* !AO_PREFER_GENERALIZED */

#ifdef AO_HAVE_DOUBLE_PTR_STORAGE

# ifndef AO_HAVE_double_load
    AO_INLINE AO_double_t
    AO_double_load(const volatile AO_double_t *addr)
    {
      AO_double_t result;

      result.AO_whole = __atomic_load_n(&addr->AO_whole, __ATOMIC_RELAXED);
      return result;
    }
#   define AO_HAVE_double_load
# endif

# ifndef AO_HAVE_double_load_acquire
    AO_INLINE AO_double_t
    AO_double_load_acquire(const volatile AO_double_t *addr)
    {
      AO_double_t result;

      result.AO_whole = __atomic_load_n(&addr->AO_whole, __ATOMIC_ACQUIRE);
      return result;
    }
#   define AO_HAVE_double_load_acquire
# endif

# ifndef AO_HAVE_double_store
    AO_INLINE void
    AO_double_store(volatile AO_double_t *addr, AO_double_t value)
    {
      __atomic_store_n(&addr->AO_whole, value.AO_whole, __ATOMIC_RELAXED);
    }
#   define AO_HAVE_double_store
# endif

# ifndef AO_HAVE_double_store_release
    AO_INLINE void
    AO_double_store_release(volatile AO_double_t *addr, AO_double_t value)
    {
      __atomic_store_n(&addr->AO_whole, value.AO_whole, __ATOMIC_RELEASE);
    }
#   define AO_HAVE_double_store_release
# endif

# ifndef AO_HAVE_double_compare_and_swap
    AO_INLINE int
    AO_double_compare_and_swap(volatile AO_double_t *addr,
                               AO_double_t old_val, AO_double_t new_val)
    {
      return (int)__atomic_compare_exchange_n(&addr->AO_whole,
                                  &old_val.AO_whole /* p_expected */,
                                  new_val.AO_whole /* desired */,
                                  0 /* is_weak: false */,
                                  __ATOMIC_RELAXED /* success */,
                                  __ATOMIC_RELAXED /* failure */);
    }
#   define AO_HAVE_double_compare_and_swap
# endif

  /* TODO: Add double CAS _acquire/release/full primitives. */
#endif /* AO_HAVE_DOUBLE_PTR_STORAGE */
