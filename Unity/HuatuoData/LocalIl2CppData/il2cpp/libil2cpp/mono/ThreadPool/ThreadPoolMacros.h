#pragma once

#include "os/Atomic.h"

#define COUNTER_CHECK(counter) \
    do { \
        IL2CPP_ASSERT(counter._.max_working > 0); \
        IL2CPP_ASSERT(counter._.working >= 0); \
        IL2CPP_ASSERT(counter._.active >= 0); \
    } while (0)

#define COUNTER_READ() (il2cpp::os::Atomic::Read64 (&g_ThreadPool->counters.as_int64_t))

#define COUNTER_ATOMIC(var, block) \
    do { \
        ThreadPoolCounter __old; \
        do { \
            IL2CPP_ASSERT(g_ThreadPool); \
            __old.as_int64_t = COUNTER_READ (); \
            (var) = __old; \
            { block; } \
            COUNTER_CHECK (var); \
        } while (il2cpp::os::Atomic::CompareExchange64 (&g_ThreadPool->counters.as_int64_t, (var).as_int64_t, __old.as_int64_t) != __old.as_int64_t); \
    } while (0)

#define CPU_USAGE_LOW 80
#define CPU_USAGE_HIGH 95
