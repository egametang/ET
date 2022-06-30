#pragma once

enum { Baselib_SystemSemaphore_PlatformSize = sizeof(void*) * 4 }; // 16 on 32bit and 32 on 64bit

#define MAX_PATH PATH_MAX

#ifndef EXPORTED_SYMBOL
    #define EXPORTED_SYMBOL __attribute__((visibility("default")))
#endif
#ifndef IMPORTED_SYMBOL
    #define IMPORTED_SYMBOL
#endif

#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #define PLATFORM_FUTEX_NATIVE_SUPPORT 1
#endif

// From #include <signal.h>
#ifdef __cplusplus
extern "C" {
    extern int raise(int __sig) throw ();
}
#else
extern int raise(int __sig);
#endif

// SIGTRAP from #include <signal.h>
// checked via static assert in platform config.
#define DETAIL_BASELIB_SIGTRAP 5

#define BASELIB_DEBUG_TRAP() raise(DETAIL_BASELIB_SIGTRAP)
