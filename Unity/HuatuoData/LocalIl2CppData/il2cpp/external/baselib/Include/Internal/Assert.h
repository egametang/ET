#pragma once

#ifndef BASELIB_ENABLE_ASSERTIONS
    #ifdef NDEBUG
        #define BASELIB_ENABLE_ASSERTIONS 0
    #else
        #define BASELIB_ENABLE_ASSERTIONS 1
    #endif
#endif

#include "../C/Baselib_Debug.h"


#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

#if COMPILER_CLANG || COMPILER_GCC
__attribute__((format(printf, 1, 2)))
#endif
BASELIB_API void detail_AssertLog(const char* format, ...);

#define DETAIL__ASSERT_LOG(ASSERT_EXPRESSION_, message, ...)                                                                      \
    PP_EVAL(PP_IF_ELSE(PP_VARG_IS_NONEMPTY(__VA_ARGS__))                                                                          \
        (detail_AssertLog("%s(%d): Assertion failed (%s) - " message "\n", __FILE__, __LINE__, #ASSERT_EXPRESSION_, __VA_ARGS__)) \
        (detail_AssertLog("%s(%d): Assertion failed (%s) - %s\n", __FILE__, __LINE__, #ASSERT_EXPRESSION_, message))              \
    )

#define BaselibAssert(ASSERT_EXPRESSION_, ...)                                                                      \
    do {                                                                                                            \
        if (BASELIB_ENABLE_ASSERTIONS)                                                                                      \
        {                                                                                                           \
            if(!(ASSERT_EXPRESSION_))                                                                               \
            {                                                                                                       \
                PP_EVAL(PP_IF_ELSE(PP_VARG_IS_NONEMPTY(__VA_ARGS__))                                                \
                    (DETAIL__ASSERT_LOG(ASSERT_EXPRESSION_, __VA_ARGS__))                                           \
                    (detail_AssertLog("%s(%d): Assertion failed (%s)\n", __FILE__, __LINE__, #ASSERT_EXPRESSION_))  \
                );                                                                                                  \
                Baselib_Debug_Break();                                                                              \
            }                                                                                                       \
        }                                                                                                           \
    } while(0)

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
