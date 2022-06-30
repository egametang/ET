#pragma once

#include <stddef.h> // ptrdiff_t

#if defined(__aarch64__) && defined(__arm__)
#error We assume both __aarch64__ and __arm__ cannot be defined at tha same time.
#endif

#if defined(__aarch64__) || defined(_M_ARM64)
#define IL2CPP_TARGET_ARM64 1
#define IL2CPP_TARGET_ARMV7 0
#elif defined(__arm__)
#define IL2CPP_TARGET_ARM64 0
#define IL2CPP_TARGET_ARMV7 1
#else
#define IL2CPP_TARGET_ARM64 0
#define IL2CPP_TARGET_ARMV7 0
#endif

#if defined(__arm64e__) && defined(__PTRAUTH_INTRINSICS__)
#define IL2CPP_TARGET_ARM64E 1
#else
#define IL2CPP_TARGET_ARM64E 0
#endif

#if defined(__x86_64__) || defined(_M_X64)
#define IL2CPP_TARGET_X64 1
#define IL2CPP_TARGET_X86 0
#elif defined(__i386__) || defined(_M_IX86)
#define IL2CPP_TARGET_X64 0
#define IL2CPP_TARGET_X86 1
#else
#define IL2CPP_TARGET_X64 0
#define IL2CPP_TARGET_X86 0
#endif

#if defined(EMBEDDED_LINUX)
#define IL2CPP_TARGET_EMBEDDED_LINUX 1
#endif

// Large executables on ARM64 and ARMv7 can cause linker errors.
// Specifically, the arm instruction set limits the range a branch can
// take (e.g. 128MB on ARM64). Normally, the linker will insert branch
// islands to bridge gaps larger than the maximum branch range. However,
// branch islands only work within a section, not across sections. So if
// IL2CPP puts managed code into a specific section of the binary, branch
// isalnds won't work. That means that proejcts with a large executable
// size may fail to link.
//
// Set the define IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND to a value of 1
// work around this issue.
//
// The cost of this define is in correctness of managed stack traces.
// With this define enabled, managed stack traces maybe not be correct
// in some cases, because the stack trace generation code must use
// fuzzy heuristics to detemine if a given instrion pointer is in a
// managed method.

#if IL2CPP_TARGET_EMBEDDED_LINUX && IL2CPP_TARGET_ARMV7
// currently on EmbeddedLinux stack unwinding doesn't work properly when using custom code sections on ARMv7
// as a result processing exceptions from managed code and resolving managed stack traces doesn't work
#ifndef IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND 1
#endif
#endif

#if IL2CPP_TARGET_ARM64 || IL2CPP_TARGET_ARMV7
#ifndef IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND 0
#endif
#endif

#define IL2CPP_BINARY_SECTION_NAME "il2cpp"

#if defined(SN_TARGET_PSP2)
#define IL2CPP_TARGET_PSP2 1
#define _UNICODE 1
#define UNICODE 1
#include "il2cpp-config-psp2.h"
#elif defined(SN_TARGET_ORBIS)
#define IL2CPP_TARGET_PS4 1
#define _UNICODE 1
#define UNICODE 1
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 1
#define IL2CPP_METHOD_ATTR __attribute__((section(IL2CPP_BINARY_SECTION_NAME)))
#elif defined(_MSC_VER)
#define IL2CPP_TARGET_WINDOWS 1

#if IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 0
#else
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS !IL2CPP_MONO_DEBUGGER
#endif

#define IL2CPP_PLATFORM_SUPPORTS_DEBUGGER_PRESENT 1
#if IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
#define IL2CPP_METHOD_ATTR  __declspec(code_seg (IL2CPP_BINARY_SECTION_NAME))
#endif
#if defined(_XBOX_ONE)
#define IL2CPP_TARGET_XBOXONE 1
#define IL2CPP_PLATFORM_SUPPORTS_DEBUGGER_PRESENT 1
#define IL2CPP_ENABLE_PLATFORM_THREAD_AFFINTY 1
#elif defined(WINAPI_FAMILY) && (WINAPI_FAMILY == WINAPI_FAMILY_APP)
#define IL2CPP_TARGET_WINRT 1
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 1
#elif defined(WINAPI_FAMILY) && (WINAPI_FAMILY == WINAPI_FAMILY_GAMES)
#define IL2CPP_TARGET_WINDOWS_GAMES 1
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 1
#define IL2CPP_ENABLE_PLATFORM_THREAD_AFFINTY 1
#elif (IL2CPP_CUSTOM_PLATFORM)
#else
#define IL2CPP_TARGET_WINDOWS_DESKTOP 1
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 1
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 1
// Windows 7 is the min OS we support, so we cannot link newer APIs
#define NTDDI_VERSION    0x06010000
#define _WIN32_WINNT     0x0601
#define WINVER           0x0601
#endif
#define _UNICODE 1
#define UNICODE 1
#define STRICT 1
#elif defined(__APPLE__)
#define IL2CPP_TARGET_DARWIN 1
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 1
#define IL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO 1

#include "TargetConditionals.h"
#if TARGET_OS_IPHONE || TARGET_IPHONE_SIMULATOR || TARGET_OS_TV || TARGET_TVOS_SIMULATOR
#define IL2CPP_TARGET_IOS 1
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 1
#else
#define IL2CPP_TARGET_OSX 1
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 1
#endif

#if IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 0
#else
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS (!(IL2CPP_TARGET_IOS && IL2CPP_TARGET_ARMV7) && !IL2CPP_MONO_DEBUGGER)
#endif

#if IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
#define IL2CPP_METHOD_ATTR __attribute__((section ("__TEXT," IL2CPP_BINARY_SECTION_NAME ",regular,pure_instructions")))
#endif

// because it's android based, __ANDROID__ is *also* defined on Lumin.
// so we need to check for that *before* we check __ANDROID__ to avoid false
// positives.
#elif defined(LUMIN)
#define IL2CPP_ENABLE_PLATFORM_THREAD_RENAME 1
#define IL2CPP_TARGET_LUMIN 1
#define IL2CPP_PLATFORM_OVERRIDES_STD_FILE_HANDLES 1
#define IL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO 1
#define IL2CPP_USE_GENERIC_DEBUG_LOG 0
#define IL2CPP_SUPPORTS_PROCESS 1
#elif defined(__ANDROID__)
#define IL2CPP_TARGET_ANDROID 1
#define IL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO 1
#define IL2CPP_ENABLE_PLATFORM_THREAD_RENAME 1
#if IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 0
#else
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS !IL2CPP_MONO_DEBUGGER
#endif

#define IL2CPP_PLATFORM_DISABLE_LIBC_PINVOKE 1
#if IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
#define IL2CPP_METHOD_ATTR __attribute__((section(IL2CPP_BINARY_SECTION_NAME)))
#endif
#elif defined(__EMSCRIPTEN__)
#define IL2CPP_TARGET_JAVASCRIPT 1
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 1
#elif defined(__linux__)
#define IL2CPP_TARGET_LINUX 1
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 1
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 1

#if IL2CPP_LARGE_EXECUTABLE_ARM_WORKAROUND
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 0
#else
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS !IL2CPP_MONO_DEBUGGER
#endif

#if IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
#define IL2CPP_METHOD_ATTR __attribute__((section(IL2CPP_BINARY_SECTION_NAME)))
#endif
#elif defined(NN_PLATFORM_CTR)
#define IL2CPP_TARGET_N3DS 1
#elif defined(NN_BUILD_TARGET_PLATFORM_NX)
#define IL2CPP_TARGET_SWITCH 1
#include "il2cpp-config-switch.h"
#elif IL2CPP_TARGET_CUSTOM
// defined handled externally
#else
#error please define your target platform
#endif

#if IL2CPP_TARGET_PS5
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 1
#define IL2CPP_METHOD_ATTR __attribute__((section(IL2CPP_BINARY_SECTION_NAME)))
#endif

#ifndef IL2CPP_TARGET_WINDOWS
#define IL2CPP_TARGET_WINDOWS 0
#endif

#ifndef IL2CPP_TARGET_WINDOWS_DESKTOP
#define IL2CPP_TARGET_WINDOWS_DESKTOP 0
#endif

#ifndef IL2CPP_TARGET_WINDOWS_GAMES
#define IL2CPP_TARGET_WINDOWS_GAMES 0
#endif

#ifndef IL2CPP_TARGET_WINRT
#define IL2CPP_TARGET_WINRT 0
#endif

#ifndef IL2CPP_TARGET_XBOXONE
#define IL2CPP_TARGET_XBOXONE 0
#endif

#ifndef IL2CPP_TARGET_DARWIN
#define IL2CPP_TARGET_DARWIN 0
#endif

#ifndef IL2CPP_TARGET_IOS
#define IL2CPP_TARGET_IOS 0
#endif

#ifndef IL2CPP_TARGET_OSX
#define IL2CPP_TARGET_OSX 0
#endif

#ifndef IL2CPP_TARGET_ANDROID
#define IL2CPP_TARGET_ANDROID 0
#endif

#ifndef IL2CPP_TARGET_JAVASCRIPT
#define IL2CPP_TARGET_JAVASCRIPT 0
#endif

#ifndef IL2CPP_TARGET_LINUX
#define IL2CPP_TARGET_LINUX 0
#endif

#ifndef IL2CPP_TARGET_N3DS
#define IL2CPP_TARGET_N3DS 0
#endif

#ifndef IL2CPP_TARGET_PS4
#define IL2CPP_TARGET_PS4 0
#endif

#ifndef IL2CPP_TARGET_PSP2
#define IL2CPP_TARGET_PSP2 0
#endif

#ifndef IL2CPP_TARGET_SWITCH
#define IL2CPP_TARGET_SWITCH 0
#endif

#ifndef IL2CPP_TARGET_LUMIN
#define IL2CPP_TARGET_LUMIN 0
#endif

#ifndef IL2CPP_TARGET_EMBEDDED_LINUX
#define IL2CPP_TARGET_EMBEDDED_LINUX 0
#endif

#ifndef IL2CPP_TARGET_POSIX
#define IL2CPP_TARGET_POSIX (IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_JAVASCRIPT || IL2CPP_TARGET_LINUX || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PSP2 || IL2CPP_TARGET_LUMIN)
#endif

#define RUNTIME_TINY (IL2CPP_TINY && !IL2CPP_MONO_DEBUGGER)
#define IL2CPP_TINY_DEBUGGER (IL2CPP_TINY && IL2CPP_MONO_DEBUGGER)

#define IL2CPP_IL2CPP_TINY_SUPPORT_THREADS IL2CPP_TINY && IL2CPP_TINY_DEBUGGER
#define IL2CPP_IL2CPP_TINY_SUPPORT_SOCKETS IL2CPP_TINY && IL2CPP_TINY_DEBUGGER

#ifndef IL2CPP_SUPPORT_THREADS
#define IL2CPP_SUPPORT_THREADS ((!IL2CPP_TARGET_JAVASCRIPT || IL2CPP_TINY_DEBUGGER) && (!IL2CPP_TINY || IL2CPP_IL2CPP_TINY_SUPPORT_THREADS))
#endif

#ifndef IL2CPP_SUPPORT_SOCKETS
#define IL2CPP_SUPPORT_SOCKETS (!IL2CPP_TINY || IL2CPP_IL2CPP_TINY_SUPPORT_SOCKETS)
#endif

#ifndef IL2CPP_PLATFORM_OVERRIDES_STD_FILE_HANDLES
#define IL2CPP_PLATFORM_OVERRIDES_STD_FILE_HANDLES 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES
#define IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO
#define IL2CPP_PLATFORM_SUPPORTS_TIMEZONEINFO 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS
#define IL2CPP_PLATFORM_SUPPORTS_CUSTOM_SECTIONS 0
#endif

#ifndef IL2CPP_DEBUG
#define IL2CPP_DEBUG 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_CPU_INFO
#define IL2CPP_PLATFORM_SUPPORTS_CPU_INFO 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_DEBUGGER_PRESENT
#define IL2CPP_PLATFORM_SUPPORTS_DEBUGGER_PRESENT 0
#endif

#ifndef IL2CPP_PLATFORM_DISABLE_LIBC_PINVOKE
#define IL2CPP_PLATFORM_DISABLE_LIBC_PINVOKE 0
#endif

#ifndef IL2CPP_PLATFORM_SUPPORTS_BACKTRACE_CALL
#define IL2CPP_PLATFORM_SUPPORTS_BACKTRACE_CALL !IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_ANDROID && !IL2CPP_TARGET_LUMIN && !IL2CPP_TARGET_PS4 && !IL2CPP_TARGET_PS5
#endif //IL2CPP_PLATFORM_SUPPORTS_BACKTRACE_CALL

#ifndef IL2CPP_SUPPORT_SOCKETS_POSIX_API
#define IL2CPP_SUPPORT_SOCKETS_POSIX_API 0
#endif

#define IL2CPP_USE_STD_THREAD 0

#define IL2CPP_THREADS_STD IL2CPP_USE_STD_THREAD
#define IL2CPP_THREADS_PTHREAD (!IL2CPP_THREADS_STD && IL2CPP_TARGET_POSIX)
#define IL2CPP_THREADS_WIN32 (!IL2CPP_THREADS_STD && IL2CPP_TARGET_WINDOWS)
#define IL2CPP_THREADS_N3DS (!IL2CPP_THREADS_STD && IL2CPP_TARGET_N3DS)
#define IL2CPP_THREADS_PS4 (!IL2CPP_THREADS_STD && IL2CPP_TARGET_PS4)
#define IL2CPP_THREADS_PSP2 (!IL2CPP_THREADS_STD && IL2CPP_TARGET_PSP2)
#define IL2CPP_THREADS_SWITCH (!IL2CPP_THREADS_STD && IL2CPP_TARGET_SWITCH)

#define IL2CPP_THREAD_HAS_CPU_SET IL2CPP_TARGET_POSIX && !IL2CPP_THREADS_PS4

// Not supported on TINY because it doesn't support synchronization context
// Not supported on no runtime because it needs to call back into the runtime!
#define IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT (IL2CPP_TARGET_WINDOWS) && !IL2CPP_TINY && !RUNTIME_NONE && !IL2CPP_TARGET_WINDOWS_GAMES

/* Trigger assert if 'ptr' is not aligned to 'alignment'. */
#define ASSERT_ALIGNMENT(ptr, alignment) \
    IL2CPP_ASSERT((((ptrdiff_t) ptr) & (alignment - 1)) == 0 && "Unaligned pointer!")

    #if defined(_MSC_VER)
    #if defined(_M_X64) || defined(_M_ARM64)
        #define IL2CPP_SIZEOF_VOID_P 8
    #elif defined(_M_IX86) || defined(_M_ARM)
        #define IL2CPP_SIZEOF_VOID_P 4
    #else
        #error invalid windows architecture
    #endif
#elif defined(__GNUC__) || defined(__SNC__)
    #if defined(__x86_64__)
        #define IL2CPP_SIZEOF_VOID_P 8
    #elif defined(__i386__)
        #define IL2CPP_SIZEOF_VOID_P 4
    #elif defined(__EMSCRIPTEN__)
        #define IL2CPP_SIZEOF_VOID_P 4
    #elif defined(__arm__)
        #define IL2CPP_SIZEOF_VOID_P 4
    #elif defined(__arm64__) || defined(__aarch64__)
        #define IL2CPP_SIZEOF_VOID_P 8
    #else
        #error invalid windows architecture
    #endif
#else
    #error please define your target architecture size
#endif

#ifndef IL2CPP_USE_GENERIC_CRASH_HELPERS
#define IL2CPP_USE_GENERIC_CRASH_HELPERS (!IL2CPP_TARGET_WINDOWS && !IL2CPP_TARGET_POSIX)
#endif

#ifndef IL2CPP_SUPPORTS_CONSOLE_EXTENSION
#define IL2CPP_SUPPORTS_CONSOLE_EXTENSION IL2CPP_TARGET_ANDROID
#endif

#define IL2CPP_COMPILER_MSVC (IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE)

#if IL2CPP_COMPILER_MSVC
#ifndef STDCALL
#define STDCALL __stdcall
#endif
#ifndef CDECL
#define CDECL __cdecl
#endif
#ifndef FASTCALL
#define FASTCALL __fastcall
#endif
#ifndef THISCALL
#define THISCALL __thiscall
#endif
#else
#define STDCALL
#define CDECL
#define FASTCALL
#define THISCALL
#endif
