#pragma once

#if PLATFORM_USE_APPLE_LLVM_ATOMIC_CMPXCHG_128_PATCH

//
//  Patch for Apple LLVM version 8.x.x (clang-800.0.38 - clang-900.0.37) intrinsic 128-bit __atomic_compare_exchange implementation (debug, using opt level -O0).
//  Note that this patch is only in effect on tvOS/iOS AArch64 debug builds for Apple LLVM version 8.x.x. Arm32 verified working without patch.
//
//  Problem:
//   For the above builds, the __atomic_compare_exchange asm expasion used SUBS/SBCS to compare the pair of "obj" and "expected" values.
//   SUBS/SBCS does not provide sufficient NZCV flags for comparing two 64-bit values.
//   The result is erraneous comparison of "obj" and "expected". Some examples:
//
//    -- fails (lo != lo && hi == hi)
//    obj.lo = 5;
//    obj.hi = 10;
//    expected.lo = 3;
//    expected.hi = 10;
//
//   -- works (expected.lo < 0)
//    obj.lo = 5;
//    obj.hi = 20;
//    expected.lo = -3;
//    expected.hi = 20;
//
//    -- fails (obj.lo < 0 && hi == hi)
//    obj.lo = -5;
//    obj.hi = 30;
//    expected.lo = 3;
//    expected.hi = 30;
//
//    -- fails (expected.lo < 0 && obj.hi+1 == expected.hi)
//    obj.lo = 5;
//    obj.hi = 3;
//    expected.lo = -3;
//    expected.hi = 2;
//
//  Solution: Inline assembly replacement of __atomic_compare_exchange using the same approach as in release mode
//
//  Note: This patch should be removed in it's entirety once we require Apple LLVM version 9 (clang-900.0.37) or higher for building.
//

#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ld_instr, st_instr, barrier_instr) \
{                                                                       \
    register bool result asm ("w0");                                    \
    asm volatile                                                        \
    (                                                                   \
        "   ldp     x12, x13, [%x4]         ; load expected         \n" \
        "   ldp     x10, x11, [%x5]         ; load value            \n" \
        "   " #ld_instr "  x9, x8, [%x3]    ; load obj              \n" \
        "   eor     x13, x8, x13            ; compare to expected   \n" \
        "   eor     x12, x9, x12                                    \n" \
        "   orr     x12, x12, x13                                   \n" \
        "   cbnz    x12, 0f                 ; not equal = no store  \n" \
        "   " #st_instr "   w12, x10, x11, [%x0] ; try store        \n" \
        "   cbnz    w12, 1f                                         \n" \
        "   orr w0, wzr, #0x1               ; success, result in w0 \n" \
        "   b   2f                                                  \n" \
        "0:                                 ; no store              \n" \
        "   clrex                                                   \n" \
        "1:                                 ; failed store          \n" \
        "   movz    w0, #0                                          \n" \
        "2:                                 ; store expected, fail  \n" \
        "   tbnz    w0, #0, 3f                                      \n" \
        "   stp     x9, x8, [%x1]                                   \n" \
        "3:                                                         \n" \
        "   " #barrier_instr "                                      \n" \
                                                                        \
        : "+r" (obj), "+r" (expected), "=r" (result)                    \
        : "r" (obj), "r" (expected), "r" (value)                        \
        : "x8", "x9", "x10", "x11", "x12", "x13", "cc", "memory");      \
                                                                        \
  return result != 0;                                                   \
}

#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_relaxed_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldxp,  stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_acquire_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_acquire_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_release_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldxp,  stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_acq_rel_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_acq_rel_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_seq_cst_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stlxp, dmb ish)
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_seq_cst_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stlxp, dmb ish)
#define detail_APPLE_LLVM_CMP_XCHG_WEAK_128_seq_cst_seq_cst(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_WEAK_128(obj, expected, value, ldaxp, stlxp, dmb ish)

#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ld_instr, st_instr, barrier_instr) \
{                                                                           \
    register bool result asm ("w0");                                        \
    asm volatile                                                            \
    (                                                                       \
        "   ldp     x10, x11, [%x4]         ; load expected                 \n" \
        "   ldp     x12, x13, [%x5]         ; load value                    \n" \
        "0:                                                                 \n" \
        "   " #ld_instr "  x9, x8, [%x3]    ; load obj (ldxp/ldaxp)         \n" \
        "   eor     x14, x8, x11            ; compare to expected           \n" \
        "   eor     x15, x9, x10                                            \n" \
        "   orr     x14, x15, x14                                           \n" \
        "   cbnz    x14, 1f                 ; not equal = no store          \n" \
        "   " #st_instr "   w14, x12, x13, [%x0] ; try store (stxp/stlxp)   \n" \
        "   cbnz    w14, 0b                 ; retry or store result in w0   \n" \
        "   orr w0, wzr, #0x1                                               \n" \
        "   b   2f                                                          \n" \
        "1:                                 ; no store                      \n" \
        "   movz    w0, #0                                                  \n" \
        "   clrex                                                           \n" \
        "2:                                 ; store expected on fail        \n" \
        "   tbnz    w0, #0, 3f                                              \n" \
        "   stp     x9, x8, [%x1]                                           \n" \
        "3:                                                                 \n" \
        "   " #barrier_instr "                                              \n" \
                                                                            \
        : "+r" (obj), "+r" (expected), "=r" (result)                        \
        : "r" (obj), "r" (expected), "r" (value)                            \
        : "x8", "x9", "x10", "x11", "x12", "x13", "x14", "x15", "cc", "memory"); \
                                                                            \
  return result != 0;                                                       \
}

#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_relaxed_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldxp,  stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_acquire_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_acquire_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_release_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldxp,  stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_acq_rel_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_acq_rel_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stlxp, )
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_seq_cst_relaxed(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stlxp, dmb ish)
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_seq_cst_acquire(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stlxp, dmb ish)
#define detail_APPLE_LLVM_CMP_XCHG_STRONG_128_seq_cst_seq_cst(obj, expected, value) detail_APPLE_LLVM_CMP_XCHG_STRONG_128(obj, expected, value, ldaxp, stlxp, dmb ish)

#define detail_APPLE_LLVM_CMP_XCHG_128_WEAK_APPLE_LLVM_PATCH(order1, order2, int_type, obj, expected, value) \
    if(sizeof(int_type) == 16) \
        detail_APPLE_LLVM_CMP_XCHG_WEAK_128_##order1##_##order2(obj, expected, value);

#define detail_APPLE_LLVM_CMP_XCHG_128_STRONG_APPLE_LLVM_PATCH(order1, order2, int_type, obj, expected, value) \
    if(sizeof(int_type) == 16) \
        detail_APPLE_LLVM_CMP_XCHG_STRONG_128_##order1##_##order2(obj, expected, value);

#else // PLATFORM_USE_APPLE_LLVM_ATOMIC_CMPXCHG_128_PATCH

#define detail_APPLE_LLVM_CMP_XCHG_128_WEAK_APPLE_LLVM_PATCH(...)
#define detail_APPLE_LLVM_CMP_XCHG_128_STRONG_APPLE_LLVM_PATCH(...)

#endif
