#pragma once

#if _MSC_VER < 1900
    #error "Baselib requires C++11 support, i.e. MSVC 2015 or newer"
#endif

#define COMPILER_MSVC 1

#ifdef _CPPUNWIND
    #define COMPILER_SUPPORTS_EXCEPTIONS                _CPPUNWIND
#else
    #define COMPILER_SUPPORTS_EXCEPTIONS                0
#endif

#ifdef _CPPRTTI
    #define COMPILER_SUPPORTS_RTTI                      _CPPRTTI
#else
    #define COMPILER_SUPPORTS_RTTI                      0
#endif

#define COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS    1 // _MSC_VER >= 1900

#define COMPILER_BUILTIN_EXPECT(X_, Y_)             (X_)

// Tells the compiler to assume that this statement is never reached.
// (reaching it anyways is undefined behavior!)
#define COMPILER_BUILTIN_UNREACHABLE()              __assume(false)
// Tells the compiler to assume that the given expression is true until the expression is modified.
// (it is undefined behavior if the expression is not true after all)
#define COMPILER_BUILTIN_ASSUME(EXPR_)              __assume(EXPR_)

#define HAS_CLANG_FEATURE(x) 0

// Warning management
#define COMPILER_PRINT_MESSAGE(MESSAGE_)    __pragma(message(__FILE__ "(" PP_STRINGIZE(__LINE__) ") : info: " MESSAGE_))
#define COMPILER_PRINT_WARNING(MESSAGE_)    __pragma(message(__FILE__ "(" PP_STRINGIZE(__LINE__) ") : warning: " MESSAGE_))

#define COMPILER_WARNING_UNUSED_VARIABLE    4101
#define COMPILER_WARNING_DEPRECATED         4995 4996

#define COMPILER_WARNINGS_PUSH              __pragma(warning(push))
#define COMPILER_WARNINGS_POP               __pragma(warning(pop))
#define COMPILER_WARNINGS_DISABLE(Warn)     __pragma(warning(disable : Warn))

#define COMPILER_NOINLINE                   __declspec(noinline)
#define COMPILER_INLINE                     inline
#define COMPILER_FORCEINLINE                __forceinline
#define COMPILER_EMPTYINLINE                __forceinline
#define COMPILER_NORETURN                   __declspec(noreturn)

#define COMPILER_DEPRECATED(msg)            __declspec(deprecated(msg))
#define COMPILER_DEPRECATED_ENUM_VALUE(msg) /* no equivalent for this in MSVC */

#define COMPILER_ALIGN_OF(TYPE_)            __alignof(TYPE_)
#define COMPILER_ALIGN_AS(ALIGN_)           __declspec(align(ALIGN_))

#define COMPILER_C_STATIC_ASSERT(EXPR_, MSG_)  typedef char __static_assert_t[(EXPR_) != 0]

#define COMPILER_ATTRIBUTE_UNUSED           __pragma(warning(suppress:4100))

#define COMPILER_DEBUG_TRAP()               __debugbreak()

// Note that this is best effort, as "/analyze" compiler flag required to make warning appear
#define COMPILER_WARN_UNUSED_RESULT         _Check_return_

#if !defined(alloca)
    #define alloca _alloca
#endif
