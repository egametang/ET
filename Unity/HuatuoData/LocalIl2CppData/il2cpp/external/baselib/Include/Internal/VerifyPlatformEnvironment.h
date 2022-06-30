#pragma once

// This header verifies that all required platform defines have been provided by the
// BaselibPlatformEnvironment and defines all non-defined optional macros to 0. Please make
// sure to verify the proper definition of newly added platform defines here.

#ifndef EXPORTED_SYMBOL
    #error "BaselibPlatformSpecificEnvironment is expected to define EXPORTED_SYMBOL."
#endif
#ifndef IMPORTED_SYMBOL
    #error "BaselibPlatformSpecificEnvironment is expected to define IMPORTED_SYMBOL."
#endif

#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "BaselibPlatformSpecificEnvironment is expected to define PLATFORM_FUTEX_NATIVE_SUPPORT to 0 or 1."
#endif

// define all other platforms to 0
#ifndef BASELIB_PLATFORM_WINDOWS
    #define BASELIB_PLATFORM_WINDOWS 0
#endif

#ifndef BASELIB_PLATFORM_MACOS
    #define BASELIB_PLATFORM_MACOS 0
#endif

#ifndef BASELIB_PLATFORM_LINUX
    #define BASELIB_PLATFORM_LINUX 0
#endif

#ifndef BASELIB_PLATFORM_EMBEDDED_LINUX
    #define BASELIB_PLATFORM_EMBEDDED_LINUX 0
#endif

#ifndef BASELIB_PLATFORM_WINRT
    #define BASELIB_PLATFORM_WINRT 0
#endif

#ifndef BASELIB_PLATFORM_FAMILY_WINDOWSGAMES
    #define BASELIB_PLATFORM_FAMILY_WINDOWSGAMES 0
#endif

#ifndef BASELIB_PLATFORM_EMSCRIPTEN
    #define BASELIB_PLATFORM_EMSCRIPTEN 0
#endif

#ifndef BASELIB_PLATFORM_WASI
    #define BASELIB_PLATFORM_WASI 0
#endif

#ifndef BASELIB_PLATFORM_ANDROID
    #define BASELIB_PLATFORM_ANDROID 0
#endif

#ifndef BASELIB_PLATFORM_PS4
    #define BASELIB_PLATFORM_PS4 0
#endif

#ifndef BASELIB_PLATFORM_PS5
    #define BASELIB_PLATFORM_PS5 0
#endif

#ifndef BASELIB_PLATFORM_IOS
    #define BASELIB_PLATFORM_IOS 0
#endif

#ifndef BASELIB_PLATFORM_TVOS
    #define BASELIB_PLATFORM_TVOS 0
#endif

#ifndef BASELIB_PLATFORM_XBOXONE
    #define BASELIB_PLATFORM_XBOXONE 0
#endif

#ifndef BASELIB_PLATFORM_SWITCH
    #define BASELIB_PLATFORM_SWITCH 0
#endif

#ifndef BASELIB_PLATFORM_LUMIN
    #define BASELIB_PLATFORM_LUMIN 0
#endif

#ifndef BASELIB_PLATFORM_STADIA
    #define BASELIB_PLATFORM_STADIA 0
#endif

#ifndef BASELIB_PLATFORM_NETBSD
    #define BASELIB_PLATFORM_NETBSD 0
#endif

// Define all other compilers with 0

#ifndef COMPILER_MSVC
    #define COMPILER_MSVC 0
#endif

#ifndef COMPILER_GCC
    #define COMPILER_GCC 0
#endif

#ifndef COMPILER_CLANG
    #define COMPILER_CLANG 0
#endif


// Make sure no platform is defined twice.
// Note that having no known platform defined is accepted.
#if BASELIB_PLATFORM_WINDOWS + \
    BASELIB_PLATFORM_MACOS + \
    BASELIB_PLATFORM_LINUX + \
    BASELIB_PLATFORM_EMBEDDED_LINUX + \
    BASELIB_PLATFORM_WINRT + \
    BASELIB_PLATFORM_FAMILY_WINDOWSGAMES + \
    BASELIB_PLATFORM_EMSCRIPTEN + \
    BASELIB_PLATFORM_WASI + \
    BASELIB_PLATFORM_ANDROID + \
    BASELIB_PLATFORM_PS4 + \
    BASELIB_PLATFORM_PS5 + \
    BASELIB_PLATFORM_IOS + \
    BASELIB_PLATFORM_TVOS + \
    BASELIB_PLATFORM_XBOXONE + \
    BASELIB_PLATFORM_SWITCH + \
    BASELIB_PLATFORM_LUMIN + \
    BASELIB_PLATFORM_STADIA + \
    BASELIB_PLATFORM_NETBSD \
    > 1
    #error "Only a single BASELIB_PLATFORM_X is allowed to be set to 1"
#endif
