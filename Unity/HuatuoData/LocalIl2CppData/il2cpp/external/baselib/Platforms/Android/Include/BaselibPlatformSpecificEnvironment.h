#pragma once

#ifndef EXPORTED_SYMBOL
    #define EXPORTED_SYMBOL __attribute__((visibility("default")))
#endif
#ifndef IMPORTED_SYMBOL
    #define IMPORTED_SYMBOL
#endif

#ifndef PLATFORM_FUTEX_NATIVE_SUPPORT
    #define PLATFORM_FUTEX_NATIVE_SUPPORT 1
#endif

// Posix specification says alignof(max_align_t) should be min alignment supported.
// However, tests revealed that some pointers were only aligned to 8 byte!
#ifndef PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT
    #define PLATFORM_MEMORY_MALLOC_MIN_ALIGNMENT 8
#endif

#ifdef __cplusplus
extern "C" {
#endif
// From #include <signal.h>
int raise(int sig);
#ifdef __cplusplus
}
#endif

// SIGTRAP from #include <signal.h>
// checked via static assert in platform config.
#define DETAIL_BASELIB_SIGTRAP 5

#define BASELIB_DEBUG_TRAP() raise(DETAIL_BASELIB_SIGTRAP)
