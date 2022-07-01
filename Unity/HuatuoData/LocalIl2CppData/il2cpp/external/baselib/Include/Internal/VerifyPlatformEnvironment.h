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
#ifndef PLATFORM_WIN
    #define PLATFORM_WIN 0
#endif

#ifndef PLATFORM_OSX
    #define PLATFORM_OSX 0
#endif

#ifndef PLATFORM_LINUX
    #define PLATFORM_LINUX 0
#endif

#ifndef PLATFORM_WINRT
    #define PLATFORM_WINRT 0
#endif

#ifndef PLATFORM_FAMILY_WINDOWSGAMES
    #define PLATFORM_FAMILY_WINDOWSGAMES 0
#endif

#ifndef PLATFORM_WEBGL
    #define PLATFORM_WEBGL 0
#endif

#ifndef PLATFORM_WASI
    #define PLATFORM_WASI 0
#endif

#ifndef PLATFORM_ANDROID
    #define PLATFORM_ANDROID 0
#endif

#ifndef PLATFORM_PS4
    #define PLATFORM_PS4 0
#endif

#ifndef PLATFORM_PS5
    #define PLATFORM_PS5 0
#endif

#ifndef PLATFORM_IOS
    #define PLATFORM_IOS 0
#endif

#ifndef PLATFORM_TVOS
    #define PLATFORM_TVOS 0
#endif

#ifndef PLATFORM_XBOXONE
    #define PLATFORM_XBOXONE 0
#endif

#ifndef PLATFORM_SWITCH
    #define PLATFORM_SWITCH 0
#endif

#ifndef PLATFORM_LUMIN
    #define PLATFORM_LUMIN 0
#endif

#ifndef PLATFORM_STADIA
    #define PLATFORM_STADIA 0
#endif

#ifndef PLATFORM_NETBSD
    #define PLATFORM_NETBSD 0
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
