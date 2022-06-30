#pragma once

#include "Internal/Baselib_EnumSizeCheck.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Can be used to control the wakeup behavior on platforms that don't support waking up a specific number of thread.
// Syscalls don't come for free so you need to weigh the cost of doing multiple syscalls against the cost of having lots of context switches.
//
// There are however two easy cases.
// * When you only want to notify one thread use Baselib_WakeupFallbackStrategy_OneByOne.
// * When you want to wakeup all threads use Baselib_WakeupFallbackStrategy_All
//
// For the not so easy cases.
// * Use Baselib_WakeupFallbackStrategy_OneByOne when wake count is low, or significantly lower than the number of waiting threads.
// * Use Baselib_WakeupFallbackStrategy_All if wake count is high.
typedef enum Baselib_WakeupFallbackStrategy
{
    // Do one syscall for each waiting thread or notification.
    Baselib_WakeupFallbackStrategy_OneByOne,

    // Do a single syscall to wake all waiting threads.
    Baselib_WakeupFallbackStrategy_All,
} Baselib_WakeupFallbackStrategy;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_WakeupFallbackStrategy);


#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
