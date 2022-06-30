/*
 * Copyright (c) 1991-1994 by Xerox Corporation.  All rights reserved.
 * Copyright (c) 1996-1999 by Silicon Graphics.  All rights reserved.
 * Copyright (c) 1999-2003 by Hewlett-Packard Company. All rights reserved.
 *
 *
 * THIS MATERIAL IS PROVIDED AS IS, WITH ABSOLUTELY NO WARRANTY EXPRESSED
 * OR IMPLIED.  ANY USE IS AT YOUR OWN RISK.
 *
 * Permission is hereby granted to use or copy this program
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 */

#include "../test_and_set_t_is_ao_t.h"

#include "../standard_ao_double_t.h"

#ifndef AO_UNIPROCESSOR
  AO_INLINE void
  AO_nop_write(void)
  {
    __asm__ __volatile__("dmb st" : : : "memory");
  }
# define AO_HAVE_nop_write
#endif

/* TODO: Adjust version check on fixing double-wide AO support in GCC. */
#if __GNUC__ == 4

  AO_INLINE AO_double_t
  AO_double_load(const volatile AO_double_t *addr)
  {
    AO_double_t result;
    int status;

    /* Note that STXP cannot be discarded because LD[A]XP is not        */
    /* single-copy atomic (unlike LDREXD for 32-bit ARM).               */
    do {
      __asm__ __volatile__("//AO_double_load\n"
      "       ldxp  %0, %1, %3\n"
      "       stxp %w2, %0, %1, %3"
      : "=&r" (result.AO_val1), "=&r" (result.AO_val2), "=&r" (status)
      : "Q" (*addr));
    } while (AO_EXPECT_FALSE(status));
    return result;
  }
# define AO_HAVE_double_load

  AO_INLINE AO_double_t
  AO_double_load_acquire(const volatile AO_double_t *addr)
  {
    AO_double_t result;
    int status;

    do {
      __asm__ __volatile__("//AO_double_load_acquire\n"
      "       ldaxp  %0, %1, %3\n"
      "       stxp %w2, %0, %1, %3"
      : "=&r" (result.AO_val1), "=&r" (result.AO_val2), "=&r" (status)
      : "Q" (*addr));
    } while (AO_EXPECT_FALSE(status));
    return result;
  }
# define AO_HAVE_double_load_acquire

  AO_INLINE void
  AO_double_store(volatile AO_double_t *addr, AO_double_t value)
  {
    AO_double_t old_val;
    int status;

    do {
      __asm__ __volatile__("//AO_double_store\n"
      "       ldxp  %0, %1, %3\n"
      "       stxp %w2, %4, %5, %3"
      : "=&r" (old_val.AO_val1), "=&r" (old_val.AO_val2), "=&r" (status),
        "=Q" (*addr)
      : "r" (value.AO_val1), "r" (value.AO_val2));
      /* Compared to the arm.h implementation, the 'cc' (flags) are not */
      /* clobbered because A64 has no concept of conditional execution. */
    } while (AO_EXPECT_FALSE(status));
  }
# define AO_HAVE_double_store

  AO_INLINE void
  AO_double_store_release(volatile AO_double_t *addr, AO_double_t value)
  {
    AO_double_t old_val;
    int status;

    do {
      __asm__ __volatile__("//AO_double_store_release\n"
      "       ldxp  %0, %1, %3\n"
      "       stlxp %w2, %4, %5, %3"
      : "=&r" (old_val.AO_val1), "=&r" (old_val.AO_val2), "=&r" (status),
        "=Q" (*addr)
      : "r" (value.AO_val1), "r" (value.AO_val2));
    } while (AO_EXPECT_FALSE(status));
  }
# define AO_HAVE_double_store_release

  AO_INLINE int
  AO_double_compare_and_swap(volatile AO_double_t *addr,
                             AO_double_t old_val, AO_double_t new_val)
  {
    AO_double_t tmp;
    int result = 1;

    do {
      __asm__ __volatile__("//AO_double_compare_and_swap\n"
        "       ldxp  %0, %1, %2\n"
        : "=&r" (tmp.AO_val1), "=&r" (tmp.AO_val2)
        : "Q" (*addr));
      if (tmp.AO_val1 != old_val.AO_val1 || tmp.AO_val2 != old_val.AO_val2)
        break;
      __asm__ __volatile__(
        "       stxp %w0, %2, %3, %1\n"
        : "=&r" (result), "=Q" (*addr)
        : "r" (new_val.AO_val1), "r" (new_val.AO_val2));
    } while (AO_EXPECT_FALSE(result));
    return !result;
  }
# define AO_HAVE_double_compare_and_swap

  AO_INLINE int
  AO_double_compare_and_swap_acquire(volatile AO_double_t *addr,
                                     AO_double_t old_val, AO_double_t new_val)
  {
    AO_double_t tmp;
    int result = 1;

    do {
      __asm__ __volatile__("//AO_double_compare_and_swap_acquire\n"
        "       ldaxp  %0, %1, %2\n"
        : "=&r" (tmp.AO_val1), "=&r" (tmp.AO_val2)
        : "Q" (*addr));
      if (tmp.AO_val1 != old_val.AO_val1 || tmp.AO_val2 != old_val.AO_val2)
        break;
      __asm__ __volatile__(
        "       stxp %w0, %2, %3, %1\n"
        : "=&r" (result), "=Q" (*addr)
        : "r" (new_val.AO_val1), "r" (new_val.AO_val2));
    } while (AO_EXPECT_FALSE(result));
    return !result;
  }
# define AO_HAVE_double_compare_and_swap_acquire

  AO_INLINE int
  AO_double_compare_and_swap_release(volatile AO_double_t *addr,
                                     AO_double_t old_val, AO_double_t new_val)
  {
    AO_double_t tmp;
    int result = 1;

    do {
      __asm__ __volatile__("//AO_double_compare_and_swap_release\n"
        "       ldxp  %0, %1, %2\n"
        : "=&r" (tmp.AO_val1), "=&r" (tmp.AO_val2)
        : "Q" (*addr));
      if (tmp.AO_val1 != old_val.AO_val1 || tmp.AO_val2 != old_val.AO_val2)
        break;
      __asm__ __volatile__(
        "       stlxp %w0, %2, %3, %1\n"
        : "=&r" (result), "=Q" (*addr)
        : "r" (new_val.AO_val1), "r" (new_val.AO_val2));
    } while (AO_EXPECT_FALSE(result));
    return !result;
  }
# define AO_HAVE_double_compare_and_swap_release

  AO_INLINE int
  AO_double_compare_and_swap_full(volatile AO_double_t *addr,
                                  AO_double_t old_val, AO_double_t new_val)
  {
    AO_double_t tmp;
    int result = 1;

    do {
      __asm__ __volatile__("//AO_double_compare_and_swap_full\n"
        "       ldaxp  %0, %1, %2\n"
        : "=&r" (tmp.AO_val1), "=&r" (tmp.AO_val2)
        : "Q" (*addr));
      if (tmp.AO_val1 != old_val.AO_val1 || tmp.AO_val2 != old_val.AO_val2)
        break;
      __asm__ __volatile__(
        "       stlxp %w0, %2, %3, %1\n"
        : "=&r" (result), "=Q" (*addr)
        : "r" (new_val.AO_val1), "r" (new_val.AO_val2));
    } while (AO_EXPECT_FALSE(result));
    return !result;
  }
# define AO_HAVE_double_compare_and_swap_full
#endif /* __GNUC__ == 4 */

#include "generic.h"
