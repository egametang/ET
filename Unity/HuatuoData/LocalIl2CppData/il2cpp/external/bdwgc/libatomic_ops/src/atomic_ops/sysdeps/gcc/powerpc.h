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
 * for any purpose,  provided the above notices are retained on all copies.
 * Permission to modify the code and to distribute modified code is granted,
 * provided the above notices are retained, and a notice that the code was
 * modified is included with the above copyright notice.
 *
 */

/* Memory model documented at http://www-106.ibm.com/developerworks/    */
/* eserver/articles/archguide.html and (clearer)                        */
/* http://www-106.ibm.com/developerworks/eserver/articles/powerpc.html. */
/* There appears to be no implicit ordering between any kind of         */
/* independent memory references.                                       */
/* Architecture enforces some ordering based on control dependence.     */
/* I don't know if that could help.                                     */
/* Data-dependent loads are always ordered.                             */
/* Based on the above references, eieio is intended for use on          */
/* uncached memory, which we don't support.  It does not order loads    */
/* from cached memory.                                                  */

#include "../all_aligned_atomic_load_store.h"

#include "../test_and_set_t_is_ao_t.h"
        /* There seems to be no byte equivalent of lwarx, so this       */
        /* may really be what we want, at least in the 32-bit case.     */

AO_INLINE void
AO_nop_full(void)
{
  __asm__ __volatile__("sync" : : : "memory");
}
#define AO_HAVE_nop_full

/* lwsync apparently works for everything but a StoreLoad barrier.      */
AO_INLINE void
AO_lwsync(void)
{
#ifdef __NO_LWSYNC__
  __asm__ __volatile__("sync" : : : "memory");
#else
  __asm__ __volatile__("lwsync" : : : "memory");
#endif
}

#define AO_nop_write() AO_lwsync()
#define AO_HAVE_nop_write

#define AO_nop_read() AO_lwsync()
#define AO_HAVE_nop_read

/* We explicitly specify load_acquire, since it is important, and can   */
/* be implemented relatively cheaply.  It could be implemented          */
/* with an ordinary load followed by a lwsync.  But the general wisdom  */
/* seems to be that a data dependent branch followed by an isync is     */
/* cheaper.  And the documentation is fairly explicit that this also    */
/* has acquire semantics.                                               */
/* ppc64 uses ld not lwz */
AO_INLINE AO_t
AO_load_acquire(const volatile AO_t *addr)
{
  AO_t result;
#if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
   __asm__ __volatile__ (
    "ld%U1%X1 %0,%1\n"
    "cmpw %0,%0\n"
    "bne- 1f\n"
    "1: isync\n"
    : "=r" (result)
    : "m"(*addr) : "memory", "cr0");
#else
  /* FIXME: We should get gcc to allocate one of the condition  */
  /* registers.  I always got "impossible constraint" when I    */
  /* tried the "y" constraint.                                  */
  __asm__ __volatile__ (
    "lwz%U1%X1 %0,%1\n"
    "cmpw %0,%0\n"
    "bne- 1f\n"
    "1: isync\n"
    : "=r" (result)
    : "m"(*addr) : "memory", "cc");
#endif
  return result;
}
#define AO_HAVE_load_acquire

/* We explicitly specify store_release, since it relies         */
/* on the fact that lwsync is also a LoadStore barrier.         */
AO_INLINE void
AO_store_release(volatile AO_t *addr, AO_t value)
{
  AO_lwsync();
  *addr = value;
}
#define AO_HAVE_store_release

#ifndef AO_PREFER_GENERALIZED
/* This is similar to the code in the garbage collector.  Deleting      */
/* this and having it synthesized from compare_and_swap would probably  */
/* only cost us a load immediate instruction.                           */
AO_INLINE AO_TS_VAL_t
AO_test_and_set(volatile AO_TS_t *addr) {
#if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
/* Completely untested.  And we should be using smaller objects anyway. */
  unsigned long oldval;
  unsigned long temp = 1; /* locked value */

  __asm__ __volatile__(
               "1:ldarx %0,0,%1\n"   /* load and reserve               */
               "cmpdi %0, 0\n"       /* if load is                     */
               "bne 2f\n"            /*   non-zero, return already set */
               "stdcx. %2,0,%1\n"    /* else store conditional         */
               "bne- 1b\n"           /* retry if lost reservation      */
               "2:\n"                /* oldval is zero if we set       */
              : "=&r"(oldval)
              : "r"(addr), "r"(temp)
              : "memory", "cr0");
#else
  int oldval;
  int temp = 1; /* locked value */

  __asm__ __volatile__(
               "1:lwarx %0,0,%1\n"   /* load and reserve               */
               "cmpwi %0, 0\n"       /* if load is                     */
               "bne 2f\n"            /*   non-zero, return already set */
               "stwcx. %2,0,%1\n"    /* else store conditional         */
               "bne- 1b\n"           /* retry if lost reservation      */
               "2:\n"                /* oldval is zero if we set       */
              : "=&r"(oldval)
              : "r"(addr), "r"(temp)
              : "memory", "cr0");
#endif
  return (AO_TS_VAL_t)oldval;
}
#define AO_HAVE_test_and_set

AO_INLINE AO_TS_VAL_t
AO_test_and_set_acquire(volatile AO_TS_t *addr) {
  AO_TS_VAL_t result = AO_test_and_set(addr);
  AO_lwsync();
  return result;
}
#define AO_HAVE_test_and_set_acquire

AO_INLINE AO_TS_VAL_t
AO_test_and_set_release(volatile AO_TS_t *addr) {
  AO_lwsync();
  return AO_test_and_set(addr);
}
#define AO_HAVE_test_and_set_release

AO_INLINE AO_TS_VAL_t
AO_test_and_set_full(volatile AO_TS_t *addr) {
  AO_TS_VAL_t result;
  AO_lwsync();
  result = AO_test_and_set(addr);
  AO_lwsync();
  return result;
}
#define AO_HAVE_test_and_set_full
#endif /* !AO_PREFER_GENERALIZED */

#ifndef AO_GENERALIZE_ASM_BOOL_CAS

  AO_INLINE int
  AO_compare_and_swap(volatile AO_t *addr, AO_t old, AO_t new_val)
  {
    AO_t oldval;
    int result = 0;
#   if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
      __asm__ __volatile__(
        "1:ldarx %0,0,%2\n"     /* load and reserve             */
        "cmpd %0, %4\n"         /* if load is not equal to      */
        "bne 2f\n"              /*   old, fail                  */
        "stdcx. %3,0,%2\n"      /* else store conditional       */
        "bne- 1b\n"             /* retry if lost reservation    */
        "li %1,1\n"             /* result = 1;                  */
        "2:\n"
        : "=&r"(oldval), "=&r"(result)
        : "r"(addr), "r"(new_val), "r"(old), "1"(result)
        : "memory", "cr0");
#   else
      __asm__ __volatile__(
        "1:lwarx %0,0,%2\n"     /* load and reserve             */
        "cmpw %0, %4\n"         /* if load is not equal to      */
        "bne 2f\n"              /*   old, fail                  */
        "stwcx. %3,0,%2\n"      /* else store conditional       */
        "bne- 1b\n"             /* retry if lost reservation    */
        "li %1,1\n"             /* result = 1;                  */
        "2:\n"
        : "=&r"(oldval), "=&r"(result)
        : "r"(addr), "r"(new_val), "r"(old), "1"(result)
        : "memory", "cr0");
#   endif
    return result;
  }
# define AO_HAVE_compare_and_swap

  AO_INLINE int
  AO_compare_and_swap_acquire(volatile AO_t *addr, AO_t old, AO_t new_val)
  {
    int result = AO_compare_and_swap(addr, old, new_val);
    AO_lwsync();
    return result;
  }
# define AO_HAVE_compare_and_swap_acquire

  AO_INLINE int
  AO_compare_and_swap_release(volatile AO_t *addr, AO_t old, AO_t new_val)
  {
    AO_lwsync();
    return AO_compare_and_swap(addr, old, new_val);
  }
# define AO_HAVE_compare_and_swap_release

  AO_INLINE int
  AO_compare_and_swap_full(volatile AO_t *addr, AO_t old, AO_t new_val)
  {
    int result;
    AO_lwsync();
    result = AO_compare_and_swap(addr, old, new_val);
    AO_lwsync();
    return result;
  }
# define AO_HAVE_compare_and_swap_full

#endif /* !AO_GENERALIZE_ASM_BOOL_CAS */

AO_INLINE AO_t
AO_fetch_compare_and_swap(volatile AO_t *addr, AO_t old_val, AO_t new_val)
{
  AO_t fetched_val;
# if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
    __asm__ __volatile__(
      "1:ldarx %0,0,%1\n"       /* load and reserve             */
      "cmpd %0, %3\n"           /* if load is not equal to      */
      "bne 2f\n"                /*   old_val, fail              */
      "stdcx. %2,0,%1\n"        /* else store conditional       */
      "bne- 1b\n"               /* retry if lost reservation    */
      "2:\n"
      : "=&r"(fetched_val)
      : "r"(addr), "r"(new_val), "r"(old_val)
      : "memory", "cr0");
# else
    __asm__ __volatile__(
      "1:lwarx %0,0,%1\n"       /* load and reserve             */
      "cmpw %0, %3\n"           /* if load is not equal to      */
      "bne 2f\n"                /*   old_val, fail              */
      "stwcx. %2,0,%1\n"        /* else store conditional       */
      "bne- 1b\n"               /* retry if lost reservation    */
      "2:\n"
      : "=&r"(fetched_val)
      : "r"(addr), "r"(new_val), "r"(old_val)
      : "memory", "cr0");
# endif
  return fetched_val;
}
#define AO_HAVE_fetch_compare_and_swap

AO_INLINE AO_t
AO_fetch_compare_and_swap_acquire(volatile AO_t *addr, AO_t old_val,
                                  AO_t new_val)
{
  AO_t result = AO_fetch_compare_and_swap(addr, old_val, new_val);
  AO_lwsync();
  return result;
}
#define AO_HAVE_fetch_compare_and_swap_acquire

AO_INLINE AO_t
AO_fetch_compare_and_swap_release(volatile AO_t *addr, AO_t old_val,
                                  AO_t new_val)
{
  AO_lwsync();
  return AO_fetch_compare_and_swap(addr, old_val, new_val);
}
#define AO_HAVE_fetch_compare_and_swap_release

AO_INLINE AO_t
AO_fetch_compare_and_swap_full(volatile AO_t *addr, AO_t old_val,
                               AO_t new_val)
{
  AO_t result;
  AO_lwsync();
  result = AO_fetch_compare_and_swap(addr, old_val, new_val);
  AO_lwsync();
  return result;
}
#define AO_HAVE_fetch_compare_and_swap_full

#ifndef AO_PREFER_GENERALIZED
AO_INLINE AO_t
AO_fetch_and_add(volatile AO_t *addr, AO_t incr) {
  AO_t oldval;
  AO_t newval;
#if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
  __asm__ __volatile__(
               "1:ldarx %0,0,%2\n"   /* load and reserve                */
               "add %1,%0,%3\n"      /* increment                       */
               "stdcx. %1,0,%2\n"    /* store conditional               */
               "bne- 1b\n"           /* retry if lost reservation       */
              : "=&r"(oldval), "=&r"(newval)
              : "r"(addr), "r"(incr)
              : "memory", "cr0");
#else
  __asm__ __volatile__(
               "1:lwarx %0,0,%2\n"   /* load and reserve                */
               "add %1,%0,%3\n"      /* increment                       */
               "stwcx. %1,0,%2\n"    /* store conditional               */
               "bne- 1b\n"           /* retry if lost reservation       */
              : "=&r"(oldval), "=&r"(newval)
              : "r"(addr), "r"(incr)
              : "memory", "cr0");
#endif
  return oldval;
}
#define AO_HAVE_fetch_and_add

AO_INLINE AO_t
AO_fetch_and_add_acquire(volatile AO_t *addr, AO_t incr) {
  AO_t result = AO_fetch_and_add(addr, incr);
  AO_lwsync();
  return result;
}
#define AO_HAVE_fetch_and_add_acquire

AO_INLINE AO_t
AO_fetch_and_add_release(volatile AO_t *addr, AO_t incr) {
  AO_lwsync();
  return AO_fetch_and_add(addr, incr);
}
#define AO_HAVE_fetch_and_add_release

AO_INLINE AO_t
AO_fetch_and_add_full(volatile AO_t *addr, AO_t incr) {
  AO_t result;
  AO_lwsync();
  result = AO_fetch_and_add(addr, incr);
  AO_lwsync();
  return result;
}
#define AO_HAVE_fetch_and_add_full
#endif /* !AO_PREFER_GENERALIZED */

#if defined(__powerpc64__) || defined(__ppc64__) || defined(__64BIT__)
  /* Empty */
#else
# define AO_T_IS_INT
#endif

/* TODO: Implement double-wide operations if available. */
