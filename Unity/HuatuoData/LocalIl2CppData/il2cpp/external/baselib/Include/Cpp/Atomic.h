#pragma once

#include "../C/Baselib_Atomic.h"
#include "Internal/TypeTraits.h"

// Note that aligning by type is not possible with the C compatible COMPILER_ALIGN_AS as MSVC's own alignment attribute does not allow evaluation of sizeof
#define ALIGN_ATOMIC(TYPE_)     alignas(sizeof(TYPE_))
#define ALIGNED_ATOMIC(TYPE_)   ALIGN_ATOMIC(TYPE_) TYPE_

// Atomic interface that sticks closely to std::atomic
// Major differences:
// * free functions that operate on types other than baselib::atomic
// * baselib::atomic allows access to its internal value
// * no zero initialization on baselib::atomic
// * no single parameter versions of compare_exchange

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        enum memory_order_relaxed_t { memory_order_relaxed = 0 }; // Equal to std::memory_order_relaxed
        enum memory_order_acquire_t { memory_order_acquire = 2 }; // Equal to std::memory_order_acquire
        enum memory_order_release_t { memory_order_release = 3 }; // Equal to std::memory_order_release
        enum memory_order_acq_rel_t { memory_order_acq_rel = 4 }; // Equal to std::memory_order_acq_rel
        enum memory_order_seq_cst_t { memory_order_seq_cst = 5 }; // Equal to std::memory_order_seq_cst

        namespace detail
        {
            template<typename T, typename ... Rest>
            struct is_any : std::false_type {};

            template<typename T, typename First>
            struct is_any<T, First> : std::is_same<T, First> {};

            template<typename T, typename First, typename ... Rest>
            struct is_any<T, First, Rest...>
                : std::integral_constant<bool, std::is_same<T, First>::value || is_any<T, Rest...>::value>
            {};

    #define TEST_ATOMICS_PREREQUISITES(_TYPE) \
        static_assert(baselib::is_trivially_copyable<_TYPE>::value, "atomic operation operands needs to be trivially copyable"); \
        static_assert(sizeof(_TYPE) <= sizeof(void*) * 2, "atomic operation operands need to be smaller or equal than two pointers");

            template<typename T> static inline T fail();

            template<typename T, typename MemoryOrder, typename ... AllowedMemoryOrders> static inline T fail_prerequisites()
            {
                TEST_ATOMICS_PREREQUISITES(T);
                static_assert(is_any<MemoryOrder, AllowedMemoryOrders...>::value, "the specified memory ordering is invalid for this atomic operation");
                return fail<T>();
            }

            template<typename T, typename MemoryOrderSuccess, typename MemoryOrderFailure> static inline T fail_prerequisites_cmpxchg()
            {
                TEST_ATOMICS_PREREQUISITES(T);
                static_assert(
                    // fail: relaxed, success: relaxed/acquire/release/seq_cst
                    (std::is_same<MemoryOrderFailure, baselib::memory_order_relaxed_t>::value &&
                        is_any<MemoryOrderSuccess, baselib::memory_order_relaxed_t, baselib::memory_order_acquire_t, baselib::memory_order_release_t, baselib::memory_order_seq_cst_t>::value) ||
                    // fail: acquire, success acquire/release/seq_cst
                    (std::is_same<MemoryOrderFailure, baselib::memory_order_relaxed_t>::value &&
                        is_any<MemoryOrderSuccess, baselib::memory_order_acquire_t, baselib::memory_order_release_t, baselib::memory_order_seq_cst_t>::value) ||
                    // fail: seq_cst, success: seq_cst
                    (std::is_same<MemoryOrderSuccess, baselib::memory_order_seq_cst_t>::value && std::is_same<MemoryOrderFailure, baselib::memory_order_seq_cst_t>::value),
                    "the specified combination of memory ordering is invalid for compare exchange operations");
                return fail<T>();
            }

            template<typename T, typename MemoryOrder> static inline T fail_prerequisites_alu()
            {
                static_assert(std::is_integral<T>::value, "operands of arithmetic atomic operations need to be integral");
                return fail_prerequisites<T, MemoryOrder,
                                          baselib::memory_order_relaxed_t,
                                          baselib::memory_order_acquire_t,
                                          baselib::memory_order_release_t,
                                          baselib::memory_order_acq_rel_t,
                                          baselib::memory_order_seq_cst_t>();
            }
        }

        // MACRO generated impl
        // re-directs to Baselib_atomic_ API
        // ----------------------------------------------------------------------------------------------------------------------------------
    #define detail_THREAD_FENCE(order, ...)                                                                                             \
        static FORCE_INLINE void atomic_thread_fence(memory_order_##order##_t order)                                                    \
        {                                                                                                                               \
            return Baselib_atomic_thread_fence_##order();                                                                               \
        }

    #define detail_LOAD(op, order, id, bits, ...)                                                                                       \
        template<typename T, typename std::enable_if<baselib::is_trivial_of_size<T, bits/8>::value, int>::type = 0>                     \
        static FORCE_INLINE T atomic_load_explicit(const T& obj, memory_order_##order##_t order)                                        \
        {                                                                                                                               \
            T ret;                                                                                                                      \
            Baselib_atomic_load_##id##_##order##_v(&obj, &ret);                                                                         \
            return ret;                                                                                                                 \
        }

    #define detail_LOAD128(op, order, id, bits, ...)                                                                                    \
        template<typename T, typename std::enable_if<baselib::is_trivial_of_size<T, bits/8>::value, int>::type = 0>                     \
        static FORCE_INLINE T atomic_load_explicit(const T& obj, memory_order_##order##_t order)                                        \
        {                                                                                                                               \
            T ret;                                                                                                                      \
            Baselib_atomic_load_##id##_##order##_v(const_cast<T*>(&obj), &ret);                                                         \
            return ret;                                                                                                                 \
        }

    #define detail_STORE(op, order, id, bits, ...)                                                                                      \
        template<typename T, typename std::enable_if<baselib::is_trivial_of_size<T, bits/8>::value, int>::type = 0>                     \
        static FORCE_INLINE void atomic_store_explicit(T& obj, typename std::common_type<T>::type value, memory_order_##order##_t order)\
        {                                                                                                                               \
            return Baselib_atomic_store_##id##_##order##_v(&obj, &value);                                                               \
        }

    #define detail_LOAD_STORE(op, order, id, bits, ...)                                                                                 \
        template<typename T, typename std::enable_if<baselib::is_trivial_of_size<T, bits/8>::value, int>::type = 0>                     \
        static FORCE_INLINE T atomic_##op##_explicit(T& obj, typename std::common_type<T>::type value, memory_order_##order##_t order)  \
        {                                                                                                                               \
            T ret;                                                                                                                      \
            Baselib_atomic_##op##_##id##_##order##_v(&obj, &value, &ret);                                                               \
            return ret;                                                                                                                 \
        }

    #define detail_ALU(op, order, id, bits, ...)                                                                                        \
        template<typename T, typename std::enable_if<baselib::is_integral_of_size<T, bits/8>::value, int>::type = 0>                    \
        static FORCE_INLINE T atomic_##op##_explicit(T& obj, typename std::common_type<T>::type value, memory_order_##order##_t order)  \
        {                                                                                                                               \
            T ret;                                                                                                                      \
            Baselib_atomic_##op##_##id##_##order##_v(&obj, &value, &ret);                                                               \
            return ret;                                                                                                                 \
        }

    #define detail_CMP_XCHG(op, order1, order2, id, bits, ...)                                                                          \
        template<typename T, typename std::enable_if<baselib::is_trivial_of_size<T, bits/8>::value, int>::type = 0>                     \
        static FORCE_INLINE bool atomic_##op##_explicit(T& obj,                                                                         \
            typename std::common_type<T>::type& expected,                                                                               \
            typename std::common_type<T>::type desired,                                                                                 \
            memory_order_##order1##_t order_success,                                                                                    \
            memory_order_##order2##_t order_failure)                                                                                    \
        {                                                                                                                               \
            return Baselib_atomic_##op##_##id##_##order1##_##order2##_v(&obj, &expected, &desired);                                     \
        }

    #define detail_NOT_SUPPORTED(...)

        Baselib_Atomic_FOR_EACH_MEMORY_ORDER(
            detail_THREAD_FENCE
        )
        Baselib_Atomic_FOR_EACH_ATOMIC_OP_MEMORY_ORDER_AND_INT_TYPE(
            detail_LOAD,    // load
            detail_STORE,   // store
            detail_ALU,     // add
            detail_ALU,     // and
            detail_ALU,     // or
            detail_ALU,     // xor
            detail_LOAD_STORE, // exchange
            detail_CMP_XCHG, // compare_exchange_weak
            detail_CMP_XCHG // compare_exchange_strong
        )

    #if PLATFORM_ARCH_64
        // 128bit atomics
        Baselib_Atomic_FOR_EACH_ATOMIC_OP_AND_MEMORY_ORDER(
            detail_LOAD128,         // load
            detail_STORE,           // store
            detail_NOT_SUPPORTED,   // add
            detail_NOT_SUPPORTED,   // and
            detail_NOT_SUPPORTED,   // or
            detail_NOT_SUPPORTED,   // xor
            detail_LOAD_STORE,      // exchange
            detail_CMP_XCHG,        // compare_exchange_weak
            detail_CMP_XCHG,        // compare_exchange_strong
            128, 128)
    #endif

    #undef detail_THREAD_FENCE
    #undef detail_LOAD
    #undef detail_STORE
    #undef detail_LOAD_STORE
    #undef detail_ALU
    #undef detail_CMP_XCHG
    #undef detail_NOT_SUPPORTED

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_fetch_sub_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return atomic_fetch_add_explicit(obj, 0 - value, order);
        }

        // API documentation and default fallback for non-matching types
        // ----------------------------------------------------------------------------------------------------------------------
        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_load_explicit(const T& obj, MemoryOrder order)
        {
            return detail::fail_prerequisites<T, MemoryOrder, baselib::memory_order_relaxed_t, baselib::memory_order_acquire_t, baselib::memory_order_seq_cst_t>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE void atomic_store_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            detail::fail_prerequisites<T, MemoryOrder, baselib::memory_order_relaxed_t, baselib::memory_order_release_t, baselib::memory_order_seq_cst_t>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_fetch_add_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return detail::fail_prerequisites_alu<T, MemoryOrder>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_fetch_and_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return detail::fail_prerequisites_alu<T, MemoryOrder>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_fetch_or_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return detail::fail_prerequisites_alu<T, MemoryOrder>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_fetch_xor_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return detail::fail_prerequisites_alu<T, MemoryOrder>();
        }

        template<typename T, typename MemoryOrder>
        static FORCE_INLINE T atomic_exchange_explicit(T& obj, typename std::common_type<T>::type value, MemoryOrder order)
        {
            return detail::fail_prerequisites<T, MemoryOrder>();
        }

        template<typename T, typename MemoryOrderSuccess, typename MemoryOrderFailure>
        static FORCE_INLINE bool atomic_compare_exchange_weak_explicit(T& obj,
            typename std::common_type<T>::type& expected,
            typename std::common_type<T>::type desired,
            MemoryOrderSuccess order_success,
            MemoryOrderFailure order_failure)
        {
            detail::fail_prerequisites_cmpxchg<T, MemoryOrderSuccess, MemoryOrderFailure>();
            return false;
        }

        template<typename T, typename MemoryOrderSuccess, typename MemoryOrderFailure>
        static FORCE_INLINE bool atomic_compare_exchange_strong_explicit(T& obj,
            typename std::common_type<T>::type& expected,
            typename std::common_type<T>::type desired,
            MemoryOrderSuccess order_success,
            MemoryOrderFailure order_failure)
        {
            detail::fail_prerequisites_cmpxchg<T, MemoryOrderSuccess, MemoryOrderFailure>();
            return false;
        }

        // default memory order (memory_order_seq_cst)
        // ----------------------------------------------------------------------------------------------------------------------
        template<typename T>
        static FORCE_INLINE T atomic_load(const T& obj)
        {
            return atomic_load_explicit(obj, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE void atomic_store(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_store_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_fetch_add(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_fetch_add_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_fetch_sub(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_fetch_sub_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_fetch_and(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_fetch_and_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_fetch_or(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_fetch_or_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_fetch_xor(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_fetch_xor_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE T atomic_exchange(T& obj, typename std::common_type<T>::type value)
        {
            return atomic_exchange_explicit(obj, value, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE bool atomic_compare_exchange_weak(T& obj,
            typename std::common_type<T>::type& expected,
            typename std::common_type<T>::type desired)
        {
            return atomic_compare_exchange_weak_explicit(obj, expected, desired, memory_order_seq_cst, memory_order_seq_cst);
        }

        template<typename T>
        static FORCE_INLINE bool atomic_compare_exchange_strong(T& obj,
            typename std::common_type<T>::type& expected,
            typename std::common_type<T>::type desired)
        {
            return atomic_compare_exchange_strong_explicit(obj, expected, desired, memory_order_seq_cst, memory_order_seq_cst);
        }

        template<typename T>
        struct atomic_common
        {
            using value_type = T;

            TEST_ATOMICS_PREREQUISITES(T);

            ALIGNED_ATOMIC(T) obj;

            FORCE_INLINE atomic_common() = default;

            // Initializes atomic with a given value. Initialization is not atomic!
            FORCE_INLINE atomic_common(T value)
            {
                obj = value;
            }

            FORCE_INLINE operator T() const { return atomic_load_explicit(obj, memory_order_seq_cst); }
            FORCE_INLINE T operator=(T value) { atomic_store_explicit(obj, value, memory_order_seq_cst); return value; }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T load(TMemoryOrder order = memory_order_seq_cst) const
            {
                return atomic_load_explicit(obj, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE void store(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_store_explicit(obj, value, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T exchange(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_exchange_explicit(obj, value, order);
            }

            template<typename TMemoryOrderSuccess, typename TMemoryOrderFailure>
            FORCE_INLINE bool compare_exchange_weak(T& expected, T desired, TMemoryOrderSuccess order_success, TMemoryOrderFailure order_failure)
            {
                return atomic_compare_exchange_weak_explicit(obj, expected, desired, order_success, order_failure);
            }

            FORCE_INLINE bool compare_exchange_weak(T& expected, T desired)
            {
                return atomic_compare_exchange_weak_explicit(obj, expected, desired, memory_order_seq_cst, memory_order_seq_cst);
            }

            template<typename TMemoryOrderSuccess, typename TMemoryOrderFailure>
            FORCE_INLINE bool compare_exchange_strong(T& expected, T desired, TMemoryOrderSuccess order_success, TMemoryOrderFailure order_failure)
            {
                return atomic_compare_exchange_strong_explicit(obj, expected, desired, order_success, order_failure);
            }

            FORCE_INLINE bool compare_exchange_strong(T& expected, T desired)
            {
                return atomic_compare_exchange_strong_explicit(obj, expected, desired, memory_order_seq_cst, memory_order_seq_cst);
            }
        };

        template<typename T, bool IsIntegral>
        struct atomic_base {};

        // Atomic type for integral types.
        template<typename T>
        struct atomic_base<T, true> : atomic_common<T>
        {
            using atomic_common<T>::atomic_common;

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T fetch_add(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_fetch_add_explicit(atomic_common<T>::obj, value, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T fetch_sub(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_fetch_sub_explicit(atomic_common<T>::obj, value, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T fetch_and(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_fetch_and_explicit(atomic_common<T>::obj, value, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T fetch_or(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_fetch_or_explicit(atomic_common<T>::obj, value, order);
            }

            template<typename TMemoryOrder = memory_order_seq_cst_t>
            FORCE_INLINE T fetch_xor(T value, TMemoryOrder order = memory_order_seq_cst)
            {
                return atomic_fetch_xor_explicit(atomic_common<T>::obj, value, order);
            }

            FORCE_INLINE T operator++(int)      { return atomic_fetch_add_explicit(atomic_common<T>::obj, T(1), memory_order_seq_cst); }
            FORCE_INLINE T operator--(int)      { return atomic_fetch_sub_explicit(atomic_common<T>::obj, T(1), memory_order_seq_cst); }
            FORCE_INLINE T operator++()         { return atomic_fetch_add_explicit(atomic_common<T>::obj, T(1), memory_order_seq_cst) + T(1); }
            FORCE_INLINE T operator--()         { return atomic_fetch_sub_explicit(atomic_common<T>::obj, T(1), memory_order_seq_cst) - T(1); }
            FORCE_INLINE T operator+=(T value)  { return atomic_fetch_add_explicit(atomic_common<T>::obj, value, memory_order_seq_cst) + value; }
            FORCE_INLINE T operator-=(T value)  { return atomic_fetch_sub_explicit(atomic_common<T>::obj, value, memory_order_seq_cst) - value; }
            FORCE_INLINE T operator&=(T value)  { return atomic_fetch_and_explicit(atomic_common<T>::obj, value, memory_order_seq_cst) & value; }
            FORCE_INLINE T operator|=(T value)  { return atomic_fetch_or_explicit(atomic_common<T>::obj, value, memory_order_seq_cst) | value; }
            FORCE_INLINE T operator^=(T value)  { return atomic_fetch_xor_explicit(atomic_common<T>::obj, value, memory_order_seq_cst) ^ value; }
        };

        // Atomic type for non-integral types.
        template<typename T>
        struct atomic_base<T, false> : atomic_common<T>
        {
            using atomic_common<T>::atomic_common;
        };

        template<typename T>
        struct atomic : atomic_base<T, std::is_integral<T>::value>
        {
            using atomic_base<T, std::is_integral<T>::value>::atomic_base;
        };

    #undef TEST_ATOMICS_PREREQUISITES
    }
}
