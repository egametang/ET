#pragma once

// Verify that the GCC is correctly defining __cplusplus. This is not the case for
// GCC versions < 4.7, where it is just defined to 1. We only error here in case of linux build
// as we, as we use the commandline --std=xxx to select the featureset there. On armcc and cxppc
// we do not use any >C99 features, so the detection works correctly with __cplusplus==1
#if (__cplusplus == 1) && defined(LINUX)
    #error "This version of GCC is not supported. Please update to a more recent one."
#endif
#if defined(__cplusplus) && __cplusplus < 201103L
    #error "Baselib requires C++11 support"
#endif

#define COMPILER_GCC 1

// __cpp_exceptions is the correct way to check whether exceptions are enabled or not, but is unfortunately not supported
// by GCC versions before 5.0. For Pre 5.0 GCC, we also need to check the __EXCEPTIONS macro
#if defined(__cpp_exceptions) || __EXCEPTIONS == 1
    #define COMPILER_SUPPORTS_EXCEPTIONS             1
#else
    #define COMPILER_SUPPORTS_EXCEPTIONS             0
#endif

// __cpp_rtti is the correct way to check whether RTTI is enabled or not, but is unfortunately not supported
// by GCC versions before 5.0. For Pre 5.0 GCC, we also need to check the __GXX_RTTI macro
#if defined(__cpp_rtti) || __GXX_RTTI == 1
    #define COMPILER_SUPPORTS_RTTI                   1
#else
    #define COMPILER_SUPPORTS_RTTI                   0
#endif

// GCC >=4.9
#if defined(__cpp_generic_lambdas) && (__cpp_generic_lambdas >= 201304)
#define COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS 1
#else
#define COMPILER_SUPPORTS_GENERIC_LAMBDA_EXPRESSIONS 0
#endif

#define COMPILER_BUILTIN_EXPECT(X_, Y_)              __builtin_expect((X_), (Y_))

// Tells the compiler to assume that this statement is never reached.
// (reaching it anyways is undefined behavior!)
#define COMPILER_BUILTIN_UNREACHABLE()              __builtin_unreachable()
// Tells the compiler to assume that the given expression is true until the expression is modified.
// (it is undefined behavior if the expression is not true after all)
#define COMPILER_BUILTIN_ASSUME(EXPR_)             PP_WRAP_CODE(if (!(EXPR_)) COMPILER_BUILTIN_UNREACHABLE())

#define COMPILER_NOINLINE                   __attribute__((unused, noinline)) // unused is needed to avoid warning when a function is not used
#define COMPILER_INLINE                     __attribute__((unused)) inline
#define COMPILER_FORCEINLINE                __attribute__((unused, always_inline)) inline
#define COMPILER_EMPTYINLINE                __attribute__((const, always_inline)) inline
#define COMPILER_NORETURN                   __attribute__((noreturn))

#if (__GNUC__ == 4 && __GNUC_MINOR__ >= 5) || __GNUC__ > 4
    #define COMPILER_DEPRECATED(msg) __attribute__((deprecated(msg)))
#else
    #define COMPILER_DEPRECATED(msg) __attribute__((deprecated))
#endif

// Support for attributes on enumerators is GCC 6
#if __GNUC__ >= 6
    #define COMPILER_DEPRECATED_ENUM_VALUE(msg) __attribute__((deprecated(msg)))
#else
    #define COMPILER_DEPRECATED_ENUM_VALUE(msg)
#endif

#define COMPILER_ALIGN_OF(TYPE_)            __alignof__(TYPE_)
#define COMPILER_ALIGN_AS(ALIGN_)           __attribute__((aligned(ALIGN_)))

#define COMPILER_C_STATIC_ASSERT(EXPR_, MSG_)  _Static_assert(EXPR_, MSG_)

#define COMPILER_ATTRIBUTE_UNUSED           __attribute__((unused))

// Some versions of GCC do provide __builtin_debugtrap, but it seems to be unreliable.
// See https://github.com/scottt/debugbreak/issues/13
#if defined(__i386__) || defined(__x86_64__)
#define COMPILER_DEBUG_TRAP()               __asm__ volatile("int $0x03")
#elif defined(__thumb__)
#define COMPILER_DEBUG_TRAP()               __asm__ volatile(".inst 0xde01")
#elif defined(__arm__) && !defined(__thumb__)
#define COMPILER_DEBUG_TRAP()               __asm__ volatile(".inst 0xe7f001f0")
#elif defined(__aarch64__)
#define COMPILER_DEBUG_TRAP()               __asm__ volatile(".inst 0xd4200000")
#endif

#define COMPILER_WARN_UNUSED_RESULT         __attribute__((warn_unused_result))

#define HAS_CLANG_FEATURE(x) 0

// Warning management
#define COMPILER_PRINT_MESSAGE(MESSAGE_)    _Pragma(PP_STRINGIZE(message(__FILE__ "info: " MESSAGE_)))
#define COMPILER_PRINT_WARNING(MESSAGE_)    _Pragma(PP_STRINGIZE(message(__FILE__ "warning: " MESSAGE_)))

#define COMPILER_WARNING_UNUSED_VARIABLE    PP_STRINGIZE(-Wunused-variable)
#define COMPILER_WARNING_DEPRECATED         PP_STRINGIZE(-Wdeprecated)

#define COMPILER_WARNINGS_PUSH              _Pragma("GCC diagnostic push")
#define COMPILER_WARNINGS_POP               _Pragma("GCC diagnostic pop")
#define COMPILER_WARNINGS_DISABLE(Warn)     _Pragma(PP_STRINGIZE(GCC diagnostic ignored Warn))
