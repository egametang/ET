#pragma once

// Arm exlusive state access break implementation
#define detail_Baselib_atomic_llsc_break() __builtin_arm_clrex()

// Arm exlusive LLSC implementation using intrinsics.
#define detail_Baselib_atomic_llsc_arm_ts(obj, expected, value, code, ll_instr, sc_instr, load_barrier, store_barrier) \
    do {                                                                                                               \
        do {                                                                                                           \
            *expected = __builtin_arm_##ll_instr(obj);                                                                 \
            load_barrier;                                                                                              \
            code;                                                                                                      \
        } while (__builtin_arm_##sc_instr(*value, obj));                                                               \
        store_barrier;                                                                                                 \
    } while (false)

#define detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ll_instr, sc_instr, loadbarrier, storebarrier) \
    detail_Baselib_atomic_llsc_arm_ts((int_type*)((void*)obj),                                                                \
                                        (int_type*)((void*)expected),                                                         \
                                        (int_type*)((void*)value),                                                            \
                                        code, ll_instr, sc_instr, loadbarrier, storebarrier)

#define detail_Baselib_atomic_llsc_relaxed_relaxed_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, strex, ,)
#if PLATFORM_ARCH_64
#define detail_Baselib_atomic_llsc_acquire_relaxed_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldaex, strex, ,)
#define detail_Baselib_atomic_llsc_relaxed_release_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, stlex, ,)
#define detail_Baselib_atomic_llsc_acquire_release_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldaex, stlex, ,)
#define detail_Baselib_atomic_llsc_seq_cst_seq_cst_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldaex, stlex, , __builtin_arm_dmb(11) )
#else
#define detail_Baselib_atomic_llsc_acquire_relaxed_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, strex, __builtin_arm_dmb(11), )
#define detail_Baselib_atomic_llsc_relaxed_release_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, strex, ,__builtin_arm_dmb(11) )
#define detail_Baselib_atomic_llsc_acquire_release_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, strex, __builtin_arm_dmb(11) , __builtin_arm_dmb(11) )
#define detail_Baselib_atomic_llsc_seq_cst_seq_cst_v(obj, expected, value, code, int_type) detail_Baselib_atomic_llsc_arm_v(obj, expected, value, code, int_type, ldrex, strex, __builtin_arm_dmb(11) , __builtin_arm_dmb(11) )
#endif

#define detail_Baselib_atomic_llsc_v(obj, expected, value, code, size, loadbarrier, storebarrier) \
    detail_Baselib_atomic_llsc_##loadbarrier##_##storebarrier##_v(obj, expected, value, code, int##size##_t)

#define detail_Baselib_atomic_llsc_128_v(obj, expected, value, code, loadbarrier, storebarrier) \
    detail_Baselib_atomic_llsc_##loadbarrier##_##storebarrier##_v(obj, expected, value, code, __int128)
