/*
 * Copyright (c) 2003-2011 Hewlett-Packard Development Company, L.P.
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

/* This file contains AO primitives based on VC++ built-in intrinsic    */
/* functions commonly available across 32-bit architectures.            */

/* This file should be included from arch-specific header files.        */
/* Define AO_USE_INTERLOCKED_INTRINSICS if _Interlocked primitives      */
/* (used below) are available as intrinsic ones for a target arch       */
/* (otherwise "Interlocked" functions family is used instead).          */
/* Define AO_ASSUME_WINDOWS98 if CAS is available.                      */

#include <windows.h>
        /* Seems like over-kill, but that's what MSDN recommends.       */
        /* And apparently winbase.h is not always self-contained.       */

#if _MSC_VER < 1310 || !defined(AO_USE_INTERLOCKED_INTRINSICS)

# define _InterlockedIncrement       InterlockedIncrement
# define _InterlockedDecrement       InterlockedDecrement
# define _InterlockedExchangeAdd     InterlockedExchangeAdd
# define _InterlockedCompareExchange InterlockedCompareExchange

# define AO_INTERLOCKED_VOLATILE /**/

#else /* elif _MSC_VER >= 1310 */

# if _MSC_VER >= 1400
#   ifndef _WIN32_WCE
#     include <intrin.h>
#   endif

# else /* elif _MSC_VER < 1400 */
#  ifdef __cplusplus
     extern "C" {
#  endif
   LONG __cdecl _InterlockedIncrement(LONG volatile *);
   LONG __cdecl _InterlockedDecrement(LONG volatile *);
   LONG __cdecl _InterlockedExchangeAdd(LONG volatile *, LONG);
   LONG __cdecl _InterlockedCompareExchange(LONG volatile *,
                                        LONG /* Exchange */, LONG /* Comp */);
#  ifdef __cplusplus
     }
#  endif
# endif /* _MSC_VER < 1400 */

# if !defined(AO_PREFER_GENERALIZED) || !defined(AO_ASSUME_WINDOWS98)
#   pragma intrinsic (_InterlockedIncrement)
#   pragma intrinsic (_InterlockedDecrement)
#   pragma intrinsic (_InterlockedExchangeAdd)
# endif /* !AO_PREFER_GENERALIZED */
# pragma intrinsic (_InterlockedCompareExchange)

# define AO_INTERLOCKED_VOLATILE volatile

#endif /* _MSC_VER >= 1310 */

#if !defined(AO_PREFER_GENERALIZED) || !defined(AO_ASSUME_WINDOWS98)
AO_INLINE AO_t
AO_fetch_and_add_full(volatile AO_t *p, AO_t incr)
{
  return _InterlockedExchangeAdd((LONG AO_INTERLOCKED_VOLATILE *)p,
                                 (LONG)incr);
}
#define AO_HAVE_fetch_and_add_full

AO_INLINE AO_t
AO_fetch_and_add1_full(volatile AO_t *p)
{
  return _InterlockedIncrement((LONG AO_INTERLOCKED_VOLATILE *)p) - 1;
}
#define AO_HAVE_fetch_and_add1_full

AO_INLINE AO_t
AO_fetch_and_sub1_full(volatile AO_t *p)
{
  return _InterlockedDecrement((LONG AO_INTERLOCKED_VOLATILE *)p) + 1;
}
#define AO_HAVE_fetch_and_sub1_full
#endif /* !AO_PREFER_GENERALIZED */

#ifdef AO_ASSUME_WINDOWS98
  AO_INLINE AO_t
  AO_fetch_compare_and_swap_full(volatile AO_t *addr, AO_t old_val,
                                 AO_t new_val)
  {
#   ifdef AO_OLD_STYLE_INTERLOCKED_COMPARE_EXCHANGE
      return (AO_t)_InterlockedCompareExchange(
                                        (PVOID AO_INTERLOCKED_VOLATILE *)addr,
                                        (PVOID)new_val, (PVOID)old_val);
#   else
      return (AO_t)_InterlockedCompareExchange(
                                        (LONG AO_INTERLOCKED_VOLATILE *)addr,
                                        (LONG)new_val, (LONG)old_val);
#   endif
  }
# define AO_HAVE_fetch_compare_and_swap_full
#endif /* AO_ASSUME_WINDOWS98 */
