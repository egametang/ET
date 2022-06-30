#pragma once

// C99 compatible static_assert
// Use static_assert in all C++ code directly.
#ifdef __cplusplus
    #define BASELIB_STATIC_ASSERT(EXPR_, MSG_)      static_assert(EXPR_, MSG_)
#else
    #define BASELIB_STATIC_ASSERT(EXPR_, MSG_)      COMPILER_C_STATIC_ASSERT(EXPR_, MSG_)
#endif
