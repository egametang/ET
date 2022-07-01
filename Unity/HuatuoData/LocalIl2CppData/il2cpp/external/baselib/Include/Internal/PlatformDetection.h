#pragma once

// Detect PLATFORM_X define.
#if defined(_XBOX_ONE)
    #define PLATFORM_XBOXONE 1
#elif defined(__NX__)
    #define PLATFORM_SWITCH 1
#elif defined __ORBIS__
    #define PLATFORM_PS4 1
#elif defined __PROSPERO__
    #define PLATFORM_PS5 1
#elif defined __EMSCRIPTEN__
    #define PLATFORM_WEBGL 1
#elif defined __wasi__
    #define PLATFORM_WASI 1
#elif defined(__APPLE__)
    #include <TargetConditionals.h>
    #if TARGET_OS_IOS
        #define PLATFORM_IOS 1
    #elif TARGET_OS_TV
        #define PLATFORM_TVOS 1
    #elif TARGET_OS_OSX || TARGET_OS_MAC
        #define PLATFORM_OSX 1
    #endif
#elif defined(__NetBSD__)
    #define PLATFORM_NETBSD 1
#elif defined(linux) || defined(__linux__)
    #if defined(LUMIN)
        #define PLATFORM_LUMIN 1
    #elif defined(GGP)
        #define PLATFORM_STADIA 1
    #elif defined(ANDROID) || defined(__ANDROID__)
        #define PLATFORM_ANDROID 1
    #else
        #define PLATFORM_LINUX 1
    #endif
#elif defined(_WIN32) || defined(__WIN32__)
    #if (defined(WINAPI_FAMILY_GAMES) && (WINAPI_FAMILY == WINAPI_FAMILY_GAMES))
        #define PLATFORM_FAMILY_WINDOWSGAMES 1
    #elif __WRL_NO_DEFAULT_LIB__
        #define PLATFORM_WINRT 1
    #else
        #define PLATFORM_WIN 1
    #endif
#endif
