#pragma once

// Detect BASELIB_PLATFORM_X define.
//
// Note that PLATFORM_X defines in Unity code base may refer to one or more platforms defined by BASELIB_PLATFORM_X
// Platforms here are very loosely defined on the set of available system apis.
// They have closest relation with the platform toolchains defined in Bee.

#if defined(_XBOX_ONE)
    #define BASELIB_PLATFORM_XBOXONE 1
#elif defined(__NX__)
    #define BASELIB_PLATFORM_SWITCH 1
#elif defined __ORBIS__
    #define BASELIB_PLATFORM_PS4 1
#elif defined __PROSPERO__
    #define BASELIB_PLATFORM_PS5 1
#elif defined __EMSCRIPTEN__
    #define BASELIB_PLATFORM_EMSCRIPTEN 1
#elif defined __wasi__
    #define BASELIB_PLATFORM_WASI 1
#elif defined(__APPLE__)
    #include <TargetConditionals.h>
    #if TARGET_OS_IOS
        #define BASELIB_PLATFORM_IOS 1
    #elif TARGET_OS_TV
        #define BASELIB_PLATFORM_TVOS 1
    #elif TARGET_OS_OSX || TARGET_OS_MAC
        #define BASELIB_PLATFORM_MACOS 1
    #endif
#elif defined(__NetBSD__)
    #define BASELIB_PLATFORM_NETBSD 1
#elif defined(linux) || defined(__linux__)
    #if defined(LUMIN)
        #define BASELIB_PLATFORM_LUMIN 1
    #elif defined(GGP)
        #define BASELIB_PLATFORM_STADIA 1
    #elif defined(ANDROID) || defined(__ANDROID__)
        #define BASELIB_PLATFORM_ANDROID 1
    #elif defined(EMBEDDED_LINUX)
        #define BASELIB_PLATFORM_EMBEDDED_LINUX 1
    #else
        #define BASELIB_PLATFORM_LINUX 1
    #endif
#elif defined(_WIN32) || defined(__WIN32__)
    #include <winapifamily.h>
    #if (defined(WINAPI_FAMILY_GAMES) && (WINAPI_FAMILY == WINAPI_FAMILY_GAMES))
        #define BASELIB_PLATFORM_WINDOWSGAMES 1
    #elif WINAPI_FAMILY == WINAPI_FAMILY_APP
        #define BASELIB_PLATFORM_WINRT 1
    #else
        #define BASELIB_PLATFORM_WINDOWS 1
    #endif
#endif
