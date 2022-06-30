#pragma once

// This defines the compiler environment for clang based compilers. Please make sure to define all required features
// (see VerifyCompilerEnvironment.h for reference)

#if defined(__cplusplus) && __cplusplus < 201103L
    #error "Baselib requires C++11 support"
#endif

#define COMPILER_CLANG 1

#define HAS_CLANG_FEATURE(x) (__has_feature(x))

#define COMPILER_SUPPORTS_EXCEPTIONS                 HAS_CLANG_FEATURE(cxx_exceptions)
#define COMPILER_SUPPORTS_RTTI                       HAS_CLANG_FEATURE(cxx_rtti)
#define COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS HAS_CLANG_FEATURE(cxx_generic_lambdas) // Clang >=3.4

#define COMPILER_BUILTIN_EXPECT(X_, Y_)              __builtin_expect((X_), (Y_))

// Tells the compiler to assume that this statement is never reached.
// (reaching it anyways is undefined behavior!)
#define COMPILER_BUILTIN_UNREACHABLE()              __builtin_unreachable()
// Tells the compiler to assume that the given expression is true until the expression is modified.
// (it is undefined behavior if the expression is not true after all)
#define COMPILER_BUILTIN_ASSUME(EXPR_)              __builtin_assume(EXPR_)


#define COMPILER_NOINLINE               __attribute__((unused, noinline)) // unused is needed to avoid warning when a function is not used
#define COMPILER_INLINE                 __attribute__((unused)) inline
#define COMPILER_FORCEINLINE            __attribute__((unused, always_inline, nodebug)) inline
#define COMPILER_EMPTYINLINE            __attribute__((const, always_inline, nodebug)) inline
#define COMPILER_NORETURN               __attribute__((noreturn))

#if __has_extension(attribute_deprecated_with_message)
    #define COMPILER_DEPRECATED(msg)     __attribute__((deprecated(msg)))
    #if __has_extension(enumerator_attributes)
        #define COMPILER_DEPRECATED_ENUM_VALUE(msg) __attribute__((deprecated(msg)))
    #else
        #define COMPILER_DEPRECATED_ENUM_VALUE(msg)
    #endif
#else
    #define COMPILER_DEPRECATED(msg)     __attribute__((deprecated))
    #if __has_extension(enumerator_attributes)
        #define COMPILER_DEPRECATED_ENUM_VALUE(msg) __attribute__((deprecated))
    #else
        #define COMPILER_DEPRECATED_ENUM_VALUE(msg)
    #endif
#endif

#define COMPILER_ALIGN_OF(TYPE_)            __alignof__(TYPE_)
#define COMPILER_ALIGN_AS(ALIGN_)           __attribute__((aligned(ALIGN_)))

#define COMPILER_C_STATIC_ASSERT(EXPR_, MSG_)  _Static_assert(EXPR_, MSG_)

#define COMPILER_ATTRIBUTE_UNUSED           __attribute__((unused))

// Note that this is how the compiler defines a debug break which is not necessarily the standard way on any given platform.
// For a platform friendly implementation, use `BASELIB_DEBUG_TRAP`
#define COMPILER_DEBUG_TRAP()               __builtin_debugtrap()

#define COMPILER_WARN_UNUSED_RESULT         __attribute__((warn_unused_result))

// Warning management
// pragma message on clang does always generate a warning that cannot be disabled, therefore the clang version
// of COMPILER_PRINT_MESSAGE() does nothing
#define COMPILER_PRINT_MESSAGE(MESSAGE_)
#define COMPILER_PRINT_WARNING(MESSAGE_)    _Pragma(PP_STRINGIZE(message(__FILE__ "warning: " MESSAGE_)))

#define COMPILER_WARNING_UNUSED_VARIABLE    PP_STRINGIZE(-Wunused-variable)
#define COMPILER_WARNING_DEPRECATED         PP_STRINGIZE(-Wdeprecated)

#define COMPILER_WARNINGS_PUSH              _Pragma(PP_STRINGIZE(clang diagnostic push))
#define COMPILER_WARNINGS_POP               _Pragma(PP_STRINGIZE(clang diagnostic pop))
#define COMPILER_WARNINGS_DISABLE(Warn)     _Pragma(PP_STRINGIZE(clang diagnostic ignored Warn))
