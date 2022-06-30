#pragma once

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace Algorithm
        {
        namespace detail
        {
            template<typename T>
            static FORCE_INLINE constexpr T LogicalOrRShiftOp(T value, int shift) { return value | (value >> shift); }
        }
        }
    }
}
