#pragma once
#include "il2cpp-config-platforms.h"
#include <stdint.h>

//If we're building for a windows platform, check that we're not building on xp or earlier
#if IL2CPP_TARGET_WINDOWS
#include "il2cpp-config-mono-win.h"
#elif IL2CPP_TARGET_IOS
#include "il2cpp-config-mono-ios.h"
#elif IL2CPP_TARGET_DARWIN
#include "il2cpp-config-mono-osx.h"
#elif IL2CPP_TARGET_LINUX
#include "il2cpp-config-mono-linux.h"
#elif IL2CPP_TARGET_ANDROID
#include "il2cpp-config-mono-android.h"
#elif IL2CPP_TARGET_JAVASCRIPT
#include "il2cpp-config-mono-web.h"
#elif IL2CPP_TARGET_LUMIN
#include "il2cpp-config-mono-lumin.h"
#else
//Uncomment out after all platforms defines moved to header file
//#error "Mono Code Compiled on Unimplemented Platform"
#endif

#if defined(PID_T_IS_INT)
typedef int pid_t;
#endif

//This value will need to be checked each time we update mono
#define MONO_CORLIB_VERSION 1050001000
#define VERSION "5.0.1"

    #if defined(ARCHITECTURE_IS_X86)
        #define HOST_X86 1
        #define TARGET_X86 1
        #define MONO_ARCHITECTURE "x86"
    #elif defined(ARCHITECTURE_IS_AMD64)
        #define HOST_AMD64 1
        #define TARGET_AMD64 1
        #define MONO_ARCHITECTURE "amd64"
    #elif defined(ARCHITECTURE_IS_ARMv7)
        #define HOST_ARM 1
        #define TARGET_ARM 1
        #define MONO_ARCHITECTURE "arm"
        #define HAVE_ARMV7 1
    #elif defined(ARCHITECTURE_IS_AARCH64)
        #define HOST_AARCH64 1
        #define TARGET_AARCH64 1
        #define MONO_ARCHITECTURE "aarch64"
    #elif defined(ARCHITECTURE_IS_ARM64)
        #define HOST_ARM64 1
        #define TARGET_ARM64 1
        #define MONO_ARCHITECTURE "arm64"
    #elif defined(ARCHITECTURE_IS_WEB)
        #define TARGET_WASM 1
        #define MONO_ARCHITECTURE "wasm"
    #else
        #error Architecture not defined for this platform
    #endif

#define GPOINTER_TO_INT(ptr) ((gint)(intptr_t) (ptr))
#define GPOINTER_TO_UINT(ptr) ((guint)(intptr_t) (ptr))
#define GINT_TO_POINTER(v) ((gpointer)(intptr_t) (v))
#define GUINT_TO_POINTER(v) ((gpointer)(uintptr_t) (v))
typedef uintptr_t gsize;
typedef intptr_t gssize;
#define G_GSIZE_FORMAT "PRIdPTR"

#if defined(GPID_IS_VOID_P)
typedef void * GPid;
#elif defined(GPID_IS_INT)
typedef int GPid;
#else
    #error GPID type is not defined for this platform
#endif

#if defined(PLATFORM_IS_LITTLE_ENDIAN)
    #define G_BYTE_ORDER G_LITTLE_ENDIAN
    #define TARGET_BYTE_ORDER G_LITTLE_ENDIAN
#else
    #error Platform Endianness is not defined for this platform
#endif

#if defined(G_DIR_SEPARATOR_IS_BACKSLASH)
    #define G_DIR_SEPARATOR '\\'
    #define G_DIR_SEPARATOR_S "\\"
#elif defined(G_DIR_SEPARATOR_IS_FORWARDSLASH)
    #define G_DIR_SEPARATOR '/'
    #define G_DIR_SEPARATOR_S "/"
#else
    #error G_DIR_SEPARATOR not defined for this platform
#endif

#if defined(G_SEARCHPATH_SEPARATOR_IS_SEMICOLON)
    #define G_SEARCHPATH_SEPARATOR ';'
    #define G_SEARCHPATH_SEPARATOR_S ";"
#elif defined(G_SEARCHPATH_SEPARATOR_IS_COLON)
    #define G_SEARCHPATH_SEPARATOR ':'
    #define G_SEARCHPATH_SEPARATOR_S ":"
#else
    #error G_SEARCHPATH_SEPARATOR not defined for this platform
#endif

#if defined(STDOUT_STDERR_REQUIRES_CAST)
    #define STDOUT_FILENO (int)(intptr_t)stdout
    #define STDERR_FILENO (int)(intptr_t)stderr
#endif

#if defined(HAVE_GNUC_PRETTY_FUNCTION)
    #define G_GNUC_PRETTY_FUNCTION __FUNCTION__
#else
    #define G_GNUC_PRETTY_FUNCTION
#endif

#if defined(HAVE_DEV_RANDOM)
    #define NAME_DEV_RANDOM "/dev/random"
#else
    #define NAME_DEV_RANDOM
#endif

#if defined(HAVE_GNUC_UNUSED)
    #define G_GNUC_UNUSED __attribute__((__unused__))
#else
    #define G_GNUC_UNUSED
#endif

#if defined(HAVE_GNUC_NORETURN)
    #define G_GNUC_NORETURN __attribute__((__noreturn__))
#else
    #define G_GNUC_NORETURN
#endif

#define SIZEOF_VOID_P IL2CPP_SIZEOF_VOID_P
#define SIZEOF_REGISTER IL2CPP_SIZEOF_VOID_P
#define SIZEOF_SIZE_T sizeof(size_t)
#define SIZEOF_LONG sizeof(long)
#define SIZEOF_INT sizeof(int)
#define SIZEOF_LONG_LONG sizeof(long long)
