#pragma once

#include <type_traits>

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
// workaround for missing std::is_trivially_copyable
// this can't be put inside compiler env due to __GLIBCXX__ not being set at that point
#if (defined(__GLIBCXX__) && __GLIBCXX__ <= 20150623) || (COMPILER_GCC && __GNUC__ < 5)
        template<typename T> struct is_trivially_copyable : std::has_trivial_copy_constructor<T> {};
#else
        template<typename T> struct is_trivially_copyable : std::is_trivially_copyable<T> {};
#endif

        template<typename T, size_t S> struct is_trivial_of_size : std::integral_constant<bool, is_trivially_copyable<T>::value && (sizeof(T) == S)> {};
        template<typename T, size_t S> struct is_integral_of_size : std::integral_constant<bool, std::is_integral<T>::value && (sizeof(T) == S)> {};

        template<typename T, typename T2> struct is_of_same_signedness : std::integral_constant<bool, std::is_signed<T>::value == std::is_signed<T2>::value> {};
    }
}
