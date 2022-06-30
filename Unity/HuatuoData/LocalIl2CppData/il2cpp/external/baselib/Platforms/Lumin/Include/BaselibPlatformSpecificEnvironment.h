#pragma once

enum { Baselib_SystemSemaphore_PlatformSize = 16 }; // size should match sem_t

#ifndef EXPORTED_SYMBOL
    #define EXPORTED_SYMBOL __attribute__ ((visibility ("default")))
#endif
#ifndef IMPORTED_SYMBOL
    #define IMPORTED_SYMBOL
#endif

#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #define PLATFORM_FUTEX_NATIVE_SUPPORT 1
#endif
