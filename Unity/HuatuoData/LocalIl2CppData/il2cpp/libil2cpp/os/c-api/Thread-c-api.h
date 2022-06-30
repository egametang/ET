#pragma once

#include <stdint.h>

#if defined(__cplusplus)
#include "os/Thread.h"

typedef il2cpp::os::Thread::ThreadId UnityPalThreadId;

#else

typedef size_t UnityPalThreadId;

#endif //__cplusplus

#if defined(__cplusplus)
extern "C"
{
#endif

void UnityPalThreadInitialize();
void UnityPalSleep(uint32_t milliseconds);
UnityPalThreadId UnityPalGetCurrentThreadId();

#if defined(__cplusplus)
}
#endif
