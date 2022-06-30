#pragma once

#include <string.h>

/* first setup platform defines*/
#include "os/c-api/il2cpp-config-platforms.h"
#include "os/c-api/il2cpp-config-api-platforms.h"

#include "il2cpp-config-api.h"

#include "il2cpp-sanitizers.h"

#ifdef LIBIL2CPP_EXPORT_CODEGEN_API
# define LIBIL2CPP_CODEGEN_API IL2CPP_EXPORT
#elif LIBIL2CPP_IMPORT_CODEGEN_API
# define LIBIL2CPP_CODEGEN_API IL2CPP_IMPORT
#else
# define LIBIL2CPP_CODEGEN_API
#endif

#if defined(__ARMCC_VERSION)
    #include <wchar.h>
    #include <ctype.h>
    #define INTPTR_MAX 2147483647
#endif

#ifndef IL2CPP_METHOD_ATTR
#define IL2CPP_METHOD_ATTR
#endif

#if defined(_MSC_VER)
#define IL2CPP_CXX_ABI_MSVC 1
#else
#define IL2CPP_CXX_ABI_MSVC 0
#endif

typedef void (STDCALL *SynchronizationContextCallback)(intptr_t arg);
typedef void (STDCALL *CultureInfoChangedCallback)(const Il2CppChar* arg);

#if defined(__cplusplus)
#define IL2CPP_EXTERN_C extern "C"
#define IL2CPP_EXTERN_C_CONST extern "C" const
#define IL2CPP_EXTERN_C_BEGIN extern "C" {
#define IL2CPP_EXTERN_C_END }
#else
#define IL2CPP_EXTERN_C
#define IL2CPP_EXTERN_C_CONST extern const
#define IL2CPP_EXTERN_C_BEGIN
#define IL2CPP_EXTERN_C_END
#endif

#if IL2CPP_COMPILER_MSVC || defined(__ARMCC_VERSION)
#define IL2CPP_NO_INLINE __declspec(noinline)
#define IL2CPP_NO_ALIAS __declspec(noalias)
#define IL2CPP_PARAMETER_RESTRICT __restrict
#define IL2CPP_METHOD_RESTRICT __declspec(restrict)
#define IL2CPP_ASSUME(x) __assume(x)
#else
#define IL2CPP_NO_INLINE __attribute__ ((noinline))
#define IL2CPP_NO_ALIAS
#define IL2CPP_PARAMETER_RESTRICT
#define IL2CPP_METHOD_RESTRICT
#define IL2CPP_ASSUME(x)
#endif

#if IL2CPP_COMPILER_MSVC
#define NOVTABLE __declspec(novtable)
#else
#define NOVTABLE
#endif

#define IL2CPP_ENABLE_MONO_BUG_EMULATION 1

#ifndef ALIGN_OF // Baselib header can also define this - if so use their definition.
#if defined(__GNUC__) || defined(__SNC__) || defined(__clang__)
    #define ALIGN_OF(T) __alignof__(T)
    #define ALIGN_TYPE(val) __attribute__((aligned(val)))
    #define ALIGN_FIELD(val) ALIGN_TYPE(val)
    #define IL2CPP_FORCE_INLINE inline __attribute__ ((always_inline))
    #define IL2CPP_MANAGED_FORCE_INLINE IL2CPP_FORCE_INLINE
#elif defined(_MSC_VER)
    #define ALIGN_OF(T) __alignof(T)
#if _MSC_VER >= 1900 && defined(__cplusplus)
    #define ALIGN_TYPE(val) alignas(val)
#else
    #define ALIGN_TYPE(val) __declspec(align(val))
#endif
    #define ALIGN_FIELD(val) __declspec(align(val))
    #define IL2CPP_FORCE_INLINE __forceinline
    #define IL2CPP_MANAGED_FORCE_INLINE inline
#else
    #define ALIGN_TYPE(size)
    #define ALIGN_FIELD(size)
    #define IL2CPP_FORCE_INLINE inline
    #define IL2CPP_MANAGED_FORCE_INLINE IL2CPP_FORCE_INLINE
#endif
#endif

#define IL2CPP_PAGE_SIZE 4096

// 64-bit types are aligned to 8 bytes on 64-bit platforms
#define IL2CPP_ENABLE_INTERLOCKED_64_REQUIRED_ALIGNMENT (IL2CPP_SIZEOF_VOID_P == 8)

/* Debugging */
#ifndef IL2CPP_DEBUG
#define IL2CPP_DEBUG 0
#endif

#ifndef IL2CPP_DEVELOPMENT
#define IL2CPP_DEVELOPMENT 0
#endif

#define IL2CPP_THREADS_ALL_ACCESS (!IL2CPP_THREADS_STD && IL2CPP_TARGET_XBOXONE)

#if (IL2CPP_SUPPORT_THREADS && (!IL2CPP_THREADS_STD && !IL2CPP_THREADS_PTHREAD && !IL2CPP_THREADS_WIN32 && !IL2CPP_THREADS_XBOXONE && !IL2CPP_THREADS_N3DS && !IL2CPP_THREADS_PS4 && !IL2CPP_THREADS_PSP2 && !IL2CPP_THREADS_SWITCH && !IL2CPP_THREADS_CUSTOM))
#error "No thread implementation defined"
#endif

/* Platform support to cleanup attached threads even when native threads are not exited cleanly */
#define IL2CPP_HAS_NATIVE_THREAD_CLEANUP (IL2CPP_THREADS_PTHREAD || IL2CPP_THREADS_WIN32 || IL2CPP_TARGET_SWITCH)

#define IL2CPP_THREAD_IMPL_HAS_COM_APARTMENTS IL2CPP_TARGET_WINDOWS

#if !defined(IL2CPP_ENABLE_PLATFORM_THREAD_STACKSIZE) && IL2CPP_TARGET_IOS
#define IL2CPP_ENABLE_PLATFORM_THREAD_STACKSIZE 1
#endif

#if IL2CPP_TINY
    #if IL2CPP_TINY_DEBUG_METADATA || IL2CPP_TINY_DEBUGGER
        #define IL2CPP_ENABLE_STACKTRACES 1
    #else
        #define IL2CPP_ENABLE_STACKTRACES 0
    #endif // IL2CPP_TINY_DEBUG_METADATA
#else
    #define IL2CPP_ENABLE_STACKTRACES 1
#endif // IL2CPP_TINY

#ifndef IL2CPP_DISABLE_GC
#if IL2CPP_TINY && IL2CPP_TARGET_JAVASCRIPT
#define IL2CPP_DISABLE_GC 1
#endif
#endif

#ifndef FORCE_PINVOKE_INTERNAL
#if IL2CPP_TINY && IL2CPP_TARGET_IOS
#define FORCE_PINVOKE_INTERNAL 1
#endif
#endif

#ifndef HOST_WASM
#if IL2CPP_TINY_DEBUGGER && IL2CPP_TARGET_JAVASCRIPT
#define HOST_WASM 1
#endif
#endif

/* Platforms which use OS specific implementation to extract stracktrace */
#if !defined(IL2CPP_ENABLE_NATIVE_STACKTRACES)
#define IL2CPP_ENABLE_NATIVE_STACKTRACES (IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_LINUX || IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_IOS || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LUMIN)
#endif

/* Platforms which support native IP emission to crash reporting to enable server-side reconstruction of C# exception stack trace line numbers */
#if !defined(IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION)
#define IL2CPP_ENABLE_NATIVE_INSTRUCTION_POINTER_EMISSION (IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_LINUX || IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_IOS || IL2CPP_TARGET_ANDROID)
#endif

#if IL2CPP_ENABLE_STACKTRACES

/* Platforms which use stacktrace sentries */
#if !defined(IL2CPP_ENABLE_STACKTRACE_SENTRIES)
#define IL2CPP_ENABLE_STACKTRACE_SENTRIES (IL2CPP_TARGET_JAVASCRIPT || IL2CPP_TARGET_N3DS || IL2CPP_TARGET_SWITCH)
#endif

#endif // IL2CPP_ENABLE_STACKTRACES

#if (IL2CPP_ENABLE_STACKTRACES && !IL2CPP_ENABLE_NATIVE_STACKTRACES && !IL2CPP_ENABLE_STACKTRACE_SENTRIES)
#error "If stacktraces are supported, then either native stack traces must be supported, or usage of stacktrace sentries must be enabled!"
#endif

#if (IL2CPP_ENABLE_NATIVE_STACKTRACES + IL2CPP_ENABLE_STACKTRACE_SENTRIES) > 1
#error "Only one type of stacktraces are allowed"
#endif

#define IL2CPP_CAN_USE_MULTIPLE_SYMBOL_MAPS IL2CPP_TARGET_IOS

/* GC defines*/
#define IL2CPP_GC_BOEHM 1
#define IL2CPP_GC_NULL !IL2CPP_GC_BOEHM
#define IL2CPP_ENABLE_DEFERRED_GC   IL2CPP_TARGET_JAVASCRIPT

/* we always need to NULL pointer free memory with our current allocators */
#define NEED_TO_ZERO_PTRFREE 1
#define IL2CPP_HAS_GC_DESCRIPTORS 1

#if defined(_MSC_VER)
    #define IL2CPP_ZERO_LEN_ARRAY 0
#else
    #define IL2CPP_ZERO_LEN_ARRAY 0
#endif

#if defined(_MSC_VER)
    #define IL2CPP_HAS_CXX_CONSTEXPR (_MSC_VER >= 1900)
#else
    #define IL2CPP_HAS_CXX_CONSTEXPR (__has_feature (cxx_constexpr))
#endif

#if IL2CPP_HAS_CXX_CONSTEXPR
    #define COMPILE_TIME_CONST constexpr
#else
    #define COMPILE_TIME_CONST const
#endif

/* clang specific __has_builtin check */
#ifndef __has_builtin
    #define __has_builtin(x) 0 // Compatibility with non-clang compilers.
#endif

#if _MSC_VER
#define IL2CPP_UNREACHABLE __assume(0)
#elif __has_builtin(__builtin_unreachable)
#define IL2CPP_UNREACHABLE __builtin_unreachable()
#else
#define IL2CPP_UNREACHABLE
#endif

typedef uint16_t Il2CppMethodSlot;
static const uint16_t kInvalidIl2CppMethodSlot = 65535;

/* Debug macros */

#if defined(_MSC_VER) && _MSC_VER >= 1900 // UTF-8 literals are only supported in VS2015+
// On MSVC, __FILE__ will expand to ANSI string literal and will fail to compile if there are unicode characters in the path
// So we specify it to be UTF-8 literal explicitly
    #define MAKE_UTF8_LITERAL_HELPER(x) u8 ## x
    #define MAKE_UTF8_LITERAL(x) MAKE_UTF8_LITERAL_HELPER(x)
#else
    #define MAKE_UTF8_LITERAL(x) x
#endif

#define __FILE_UTF8__ MAKE_UTF8_LITERAL(__FILE__)
#define STRINGIZE(L)          #L
#define MAKE_STRING(M, L)     M(L)
#define $Line                   MAKE_STRING( STRINGIZE, __LINE__ )
#define FIXME                   "FIXME: "
#define ICALLMESSAGE(name)      __FILE_UTF8__ "(" $Line ") : FIXME: Missing internal call implementation: " name
#define RUNTIMEMESSAGE(name)    __FILE_UTF8__ "(" $Line ") : FIXME: Missing runtime implementation: " name
#define NOTSUPPORTEDICALLMESSAGE(target, name, reason)  __FILE_UTF8__ "(" $Line ") : Unsupported internal call for " target ":" name " - " reason


// Keeping this for future usage if needed.
//#if defined(_MSC_VER)
//  #define PRAGMA_MESSAGE(value) __pragma(message(value))
//#else
//  #define PRAGMA_MESSAGE(value) _Pragma(STRINGIZE(value))
//#endif
#define PRAGMA_MESSAGE(value)

#if !defined(__EMSCRIPTEN__)

#define IL2CPP_NOT_IMPLEMENTED_ICALL(func) \
    PRAGMA_MESSAGE(ICALLMESSAGE(#func)) \
    IL2CPP_ASSERT(0 && #func)
#define IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(func, reason) \
    PRAGMA_MESSAGE(ICALLMESSAGE(#func))

#define IL2CPP_NOT_IMPLEMENTED(func) \
    PRAGMA_MESSAGE(RUNTIMEMESSAGE(#func)) \
    IL2CPP_ASSERT(0 && #func)
#define IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(func, reason) \
    PRAGMA_MESSAGE(RUNTIMEMESSAGE(#func))

#else

#include <stdio.h>
#include <emscripten/emscripten.h>
// emscripten's assert will throw an exception in js.
// For now, we don't want that, so just printf and move on.
    #define IL2CPP_NOT_IMPLEMENTED_ICALL(func) \
    PRAGMA_MESSAGE(message(ICALLMESSAGE(#func))) \
    emscripten_log(EM_LOG_NO_PATHS | EM_LOG_CONSOLE | EM_LOG_ERROR | EM_LOG_JS_STACK, "Not implemented icall: %s\n", #func);
#define IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(func, reason) \
    PRAGMA_MESSAGE(message(ICALLMESSAGE(#func)))

#define IL2CPP_NOT_IMPLEMENTED(func) \
    PRAGMA_MESSAGE(message(RUNTIMEMESSAGE(#func))) \
    printf("Not implemented: %s\n", #func);
#define IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(func, reason) \
    PRAGMA_MESSAGE(message(RUNTIMEMESSAGE(#func)))

#endif

#define NOT_SUPPORTED_IL2CPP(func, reason) \
    il2cpp::vm::Exception::Raise (il2cpp::vm::Exception::GetNotSupportedException ( NOTSUPPORTEDICALLMESSAGE ("IL2CPP", #func, #reason) ))

#define NOT_SUPPORTED_SRE(func) \
    il2cpp::vm::Exception::Raise (il2cpp::vm::Exception::GetNotSupportedException ( NOTSUPPORTEDICALLMESSAGE ("IL2CPP", #func, "System.Reflection.Emit is not supported.") ))

#define NOT_SUPPORTED_REMOTING(func) \
    il2cpp::vm::Exception::Raise (il2cpp::vm::Exception::GetNotSupportedException ( NOTSUPPORTEDICALLMESSAGE ("IL2CPP", #func, "System.Runtime.Remoting is not supported.") ))

#if IL2CPP_TARGET_JAVASCRIPT
#define NOT_SUPPORTED_WEBGL(func, reason) \
    il2cpp::vm::Exception::Raise (il2cpp::vm::Exception::GetNotSupportedException ( NOTSUPPORTEDICALLMESSAGE ("WebGL", #func, #reason) ))
#else
#define NOT_SUPPORTED_WEBGL(func, reason)
#endif

#if IL2CPP_COMPILER_MSVC
    #define IL2CPP_DIR_SEPARATOR '\\'   /* backslash */
#else
    #define IL2CPP_DIR_SEPARATOR '/'    /* forward slash */
#endif

#ifndef IL2CPP_DISABLE_FULL_MESSAGES
    #define IL2CPP_DISABLE_FULL_MESSAGES    1
#endif

#if IL2CPP_COMPILER_MSVC
    #define IL2CPP_USE_GENERIC_SOCKET_IMPL  0
#else
    #define IL2CPP_USE_GENERIC_SOCKET_IMPL  !(IL2CPP_TARGET_POSIX || IL2CPP_TARGET_SWITCH || IL2CPP_SUPPORT_SOCKETS_POSIX_API)
#endif

#ifndef IL2CPP_USE_GENERIC_SOCKET_BRIDGE
#define IL2CPP_USE_GENERIC_SOCKET_BRIDGE !IL2CPP_TARGET_JAVASCRIPT
#endif

/* set by platforms that require special handling of SIGPIPE signalling during socket sends */
#ifndef IL2CPP_USE_SEND_NOSIGNAL
    #define IL2CPP_USE_SEND_NOSIGNAL 0
#endif

#ifndef IL2CPP_USE_GENERIC_ENVIRONMENT
#define IL2CPP_USE_GENERIC_ENVIRONMENT  (!IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_POSIX)
#endif

#define IL2CPP_USE_GENERIC_COM  (!IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_WINDOWS_GAMES)
#define IL2CPP_USE_GENERIC_COM_SAFEARRAYS   (!IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINDOWS_GAMES)
#define IL2CPP_USE_GENERIC_WINDOWSRUNTIME (!IL2CPP_TARGET_WINDOWS || RUNTIME_NONE || IL2CPP_TINY || IL2CPP_TARGET_WINDOWS_GAMES)

#ifndef IL2CPP_USE_GENERIC_MEMORY_MAPPED_FILE
#define IL2CPP_USE_GENERIC_MEMORY_MAPPED_FILE (IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINDOWS_GAMES || IL2CPP_TARGET_JAVASCRIPT || (!IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_POSIX))
#endif

#ifndef IL2CPP_HAS_CLOSE_EXEC
#define IL2CPP_HAS_CLOSE_EXEC (IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_PS4)
#endif

#ifndef IL2CPP_HAS_DUP
#define IL2CPP_HAS_DUP (IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_PS4)
#endif

#ifndef IL2CPP_USE_GENERIC_FILE
#define IL2CPP_USE_GENERIC_FILE (!IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_DARWIN)
#endif

#ifndef IL2CPP_USE_GENERIC_DEBUG_LOG
#define IL2CPP_USE_GENERIC_DEBUG_LOG !IL2CPP_TARGET_WINDOWS
#endif

#ifndef IL2CPP_USE_GENERIC_PROCESS
#define IL2CPP_USE_GENERIC_PROCESS !IL2CPP_TARGET_LUMIN
#endif

#ifndef IL2CPP_USE_GENERIC_THREAD
#define IL2CPP_USE_GENERIC_THREAD (!IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_DARWIN)
#endif

#define IL2CPP_SIZEOF_STRUCT_WITH_NO_INSTANCE_FIELDS 1
#define IL2CPP_VALIDATE_FIELD_LAYOUT 0

#ifndef IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL
#define IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL ( IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_PSP2 || ( IL2CPP_TARGET_ANDROID && __ANDROID_API__ < 21 ) )
#endif

#if IL2CPP_MONO_DEBUGGER
#define STORE_SEQ_POINT(storage, seqPoint) (storage).currentSequencePoint = seqPoint;
#define STORE_TRY_ID(storage, id) (storage).tryId = id;
#define CHECK_SEQ_POINT(storage, seqPoint) il2cpp_codegen_check_sequence_point(&(storage), seqPoint);
#define CHECK_METHOD_ENTRY_SEQ_POINT(storage, seqPoint) il2cpp_codegen_check_sequence_point_entry(&(storage), seqPoint);
#define CHECK_METHOD_EXIT_SEQ_POINT(name, storage, seqPoint) MethodExitSequencePointChecker name(&(storage), seqPoint);
#define DECLARE_METHOD_THIS(variableName, thisAddress) void* variableName[] = { thisAddress }
#define DECLARE_METHOD_PARAMS(variableName, ...) void* variableName[] = { __VA_ARGS__ }
#define DECLARE_METHOD_LOCALS(variableName, ...) void* variableName[] = { __VA_ARGS__ }
#define DECLARE_METHOD_EXEC_CTX(ctxVariable, method, thisVariable, paramsVariable, localsVariable) Il2CppSequencePointExecutionContext ctxVariable(method, thisVariable, paramsVariable, localsVariable)
#define CHECK_PAUSE_POINT il2cpp_codegen_check_pause_point()
#else
#define STORE_SEQ_POINT(storage, seqPoint)
#define STORE_TRY_ID(storage, id)
#define CHECK_SEQ_POINT(storage, seqPoint)
#define CHECK_METHOD_ENTRY_SEQ_POINT(storage, seqPoint)
#define CHECK_METHOD_EXIT_SEQ_POINT(name, storage, seqPoint)
#define DECLARE_METHOD_THIS(variableName, thisAddress)
#define DECLARE_METHOD_PARAMS(variableName, ...)
#define DECLARE_METHOD_LOCALS(variableName, ...)
#define DECLARE_METHOD_EXEC_CTX(ctxVariable, method, thisVariable, paramsVariable, localsVariable)
#define CHECK_PAUSE_POINT
#endif

#ifdef __cplusplus
template<bool value>
struct Il2CppStaticAssertHelper;

template<>
struct Il2CppStaticAssertHelper<true>
{
};


#define Assert(x) do { (void)(x); IL2CPP_ASSERT(x); } while (false)
#define Il2CppStaticAssert(...) do { Il2CppStaticAssertHelper<(__VA_ARGS__)>(); } while (false)
#endif

static const int32_t kIl2CppInt32Min = INT32_MIN;
static const int32_t kIl2CppInt32Max = INT32_MAX;
static const uint32_t kIl2CppUInt32Max = UINT32_MAX;
static const int64_t kIl2CppInt64Min = INT64_MIN;
static const int64_t kIl2CppInt64Max = INT64_MAX;
static const uint64_t kIl2CppUInt64Max = UINT64_MAX;

#if IL2CPP_SIZEOF_VOID_P == 8
static const intptr_t kIl2CppIntPtrMin = INT64_MIN;
static const intptr_t kIl2CppIntPtrMax = INT64_MAX;
static const uintptr_t kIl2CppUIntPtrMax = UINT64_MAX;
#else
static const intptr_t kIl2CppIntPtrMin = INT32_MIN;
static const intptr_t kIl2CppIntPtrMax = INT32_MAX;
static const uintptr_t kIl2CppUIntPtrMax = UINT32_MAX;
#endif

static const int ipv6AddressSize = 16;
#if !defined(IL2CPP_SUPPORT_IPV6)
#define IL2CPP_SUPPORT_IPV6 !IL2CPP_TARGET_PS4 && !IL2CPP_TARGET_SWITCH
#endif

#define IL2CPP_SUPPORT_IPV6_SUPPORT_QUERY (IL2CPP_SUPPORT_IPV6 && IL2CPP_TARGET_LINUX)

#if !defined(IL2CPP_SUPPORT_SEND_FILE)
#define IL2CPP_SUPPORT_SEND_FILE (!IL2CPP_TARGET_SWITCH)
#endif

#if !defined(IL2CPP_SUPPORT_RECV_MSG)
#define IL2CPP_SUPPORT_RECV_MSG (!IL2CPP_TARGET_SWITCH)
#endif

#if !defined(IL2CPP_SUPPORT_SEND_MSG)
#define IL2CPP_SUPPORT_SEND_MSG (!IL2CPP_TARGET_SWITCH)
#endif

#ifndef IL2CPP_USE_NETWORK_ACCESS_HANDLER
#define IL2CPP_USE_NETWORK_ACCESS_HANDLER 0
#endif

// Android: "There is no support for locales in the C library" https://code.google.com/p/android/issues/detail?id=57313
// PS4/PS2: strtol_d doesn't exist
#if !defined(IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING)
#define IL2CPP_SUPPORT_LOCALE_INDEPENDENT_PARSING (!IL2CPP_TARGET_ANDROID && !IL2CPP_TARGET_PS4 && !IL2CPP_TARGET_PSP2 && !IL2CPP_TARGET_LUMIN)
#endif

#define NO_UNUSED_WARNING(expr) (void)(expr)

typedef int32_t il2cpp_hresult_t;

// Sorted numerically!
#define IL2CPP_S_OK                          ((il2cpp_hresult_t)0)
#define IL2CPP_S_FALSE                       ((il2cpp_hresult_t)1)
#define IL2CPP_E_BOUNDS                      ((il2cpp_hresult_t)0x8000000B)
#define IL2CPP_E_CHANGED_STATE               ((il2cpp_hresult_t)0x8000000C)
#define IL2CPP_E_ILLEGAL_METHOD_CALL         ((il2cpp_hresult_t)0x8000000E)
#define IL2CPP_RO_E_CLOSED                   ((il2cpp_hresult_t)0x80000013)
#define IL2CPP_E_NOTIMPL                     ((il2cpp_hresult_t)0x80004001)
#define IL2CPP_E_NOINTERFACE                 ((il2cpp_hresult_t)0x80004002)
#define IL2CPP_E_POINTER                     ((il2cpp_hresult_t)0x80004003)
#define IL2CPP_E_ABORT                       ((il2cpp_hresult_t)0x80004004)
#define IL2CPP_E_FAIL                        ((il2cpp_hresult_t)0x80004005)
#define IL2CPP_E_UNEXPECTED                  ((il2cpp_hresult_t)0x8000FFFF)
#define IL2CPP_RPC_E_DISCONNECTED            ((il2cpp_hresult_t)0x80010108)
#define IL2CPP_RPC_E_WRONG_THREAD            ((il2cpp_hresult_t)0x8001010E)
#define IL2CPP_DISP_E_PARAMNOTFOUND          ((il2cpp_hresult_t)0x80020004)
#define IL2CPP_REGDB_E_CLASSNOTREG           ((il2cpp_hresult_t)0x80040154)
#define IL2CPP_E_FILE_NOT_FOUND              ((il2cpp_hresult_t)0x80070002)
#define IL2CPP_E_ACCESS_DENIED               ((il2cpp_hresult_t)0x80070005)
#define IL2CPP_E_OUTOFMEMORY                 ((il2cpp_hresult_t)0x8007000E)
#define IL2CPP_E_INVALIDARG                  ((il2cpp_hresult_t)0x80070057)
#define IL2CPP_COR_E_EXCEPTION               ((il2cpp_hresult_t)0x80131500)
#define IL2CPP_COR_E_EXECUTIONENGINE         ((il2cpp_hresult_t)0x80131506)
#define IL2CPP_COR_E_INVALIDOPERATION        ((il2cpp_hresult_t)0x80131509)
#define IL2CPP_COR_E_PLATFORMNOTSUPPORTED    ((il2cpp_hresult_t)0x80131539)
#define IL2CPP_COR_E_OPERATIONCANCELED       ((il2cpp_hresult_t)0x8013153B)
#define IL2CPP_COR_E_OBJECTDISPOSED          ((il2cpp_hresult_t)0x80131622)

#define IL2CPP_HR_SUCCEEDED(hr) (((il2cpp_hresult_t)(hr)) >= 0)
#define IL2CPP_HR_FAILED(hr) (((il2cpp_hresult_t)(hr)) < 0)

#define IL2CPP_LITTLE_ENDIAN 1
#define IL2CPP_BIG_ENDIAN 2
#define IL2CPP_BYTE_ORDER IL2CPP_LITTLE_ENDIAN

#if (defined(_MSC_VER) && _MSC_VER > 1600) || (__has_feature(cxx_override_control))
#define IL2CPP_OVERRIDE override
#define IL2CPP_FINAL final
#else
#define IL2CPP_OVERRIDE
#define IL2CPP_FINAL
#endif

#if (__has_feature(cxx_deleted_functions) || (defined(_MSC_VER) && _MSC_VER >= 1800))
#define IL2CPP_HAS_DELETED_FUNCTIONS 1
#else
#define IL2CPP_HAS_DELETED_FUNCTIONS 0
#endif

#if IL2CPP_TARGET_WINDOWS
static const Il2CppChar kIl2CppNewLine[] = { '\r', '\n', '\0' };
#else
static const Il2CppChar kIl2CppNewLine[] = { '\n', '\0' };
#endif

#define MAXIMUM_NESTED_GENERICS_EXCEPTION_MESSAGE "IL2CPP encountered a managed type which it cannot convert ahead-of-time. The type uses generic or array types which are nested beyond the maximum depth which can be converted.   Consider increasing the --maximum-recursive-generic-depth=%d argument"
#if IL2CPP_COMPILER_MSVC
#define IL2CPP_ATTRIBUTE_WEAK
#else
#define IL2CPP_ATTRIBUTE_WEAK __attribute__((weak))
#endif

#if IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINRT || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PSP2
#define IL2CPP_USE_GENERIC_CPU_INFO 1
#else
#define IL2CPP_USE_GENERIC_CPU_INFO 0
#endif

#if !defined(IL2CPP_CAN_CHECK_EXECUTABLE)
#define IL2CPP_CAN_CHECK_EXECUTABLE IL2CPP_TARGET_WINDOWS || (IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_PS4)
#endif

#if IL2CPP_MONO_DEBUGGER
#define IL2CPP_DEBUG_BREAK() il2cpp::utils::Debugger::UserBreak()
#else
#ifdef _MSC_VER
#define IL2CPP_DEBUG_BREAK() __debugbreak()
#else
#define IL2CPP_DEBUG_BREAK()
#endif
#endif

#if defined(__cplusplus)

template<typename Type, size_t Size>
char(*il2cpp_array_size_helper(Type(&array)[Size]))[Size];

#define IL2CPP_ARRAY_SIZE(array) (sizeof(*il2cpp_array_size_helper(array)))

#endif // __cplusplus

#ifndef IL2CPP_MUTATE_METHOD_POINTERS
#define IL2CPP_MUTATE_METHOD_POINTERS !IL2CPP_TARGET_PS4
#endif

#define IL2CPP_USE_GENERIC_ASSERT !(IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINRT || IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PS5)

#ifndef IL2CPP_USE_SPARSEHASH
#define IL2CPP_USE_SPARSEHASH (IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_IOS)
#endif

#if !IL2CPP_DEBUG
#define IL2CPP_ASSERT(expr) void(0)
#else
#if defined(__cplusplus)
#define IL2CPP_ASSERT(expr) (static_cast<bool>(expr) ? void(0) : il2cpp_assert(#expr, __FILE__, __LINE__))
#else
#define IL2CPP_ASSERT(expr) (expr) ? void(0) : il2cpp_assert(#expr, __FILE__, __LINE__))
#endif
extern void il2cpp_assert(const char* assertion, const char* file, unsigned int line);
#endif

