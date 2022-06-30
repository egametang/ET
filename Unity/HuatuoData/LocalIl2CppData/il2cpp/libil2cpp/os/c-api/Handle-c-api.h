#pragma once

#include "WaitStatus-c-api.h"
#include <stddef.h>

#if defined(__cplusplus)
#include "os/Handle.h"
typedef il2cpp::os::Handle UnityPalHandle;
#else
typedef struct UnityPalHandle UnityPalHandle;
#endif

#if defined(__cplusplus)
extern "C"
{
#endif

void UnityPalHandleDestroy(UnityPalHandle* handle);
UnityPalWaitStatus UnityPalHandleWait(UnityPalHandle* handle, int32_t interruptible);
UnityPalWaitStatus UnityPalHandleWaitMs(UnityPalHandle* handle, uint32_t ms, int32_t interruptible);
UnityPalWaitStatus UnityPalHandleSignalAndWait(UnityPalHandle* toSignal, UnityPalHandle* toWait, uint32_t ms, int32_t interruptible);
UnityPalWaitStatus UnityPalWaitForMultipleHandles(UnityPalHandle** handles, size_t numberOfHandlers, int32_t waitAll, uint32_t ms, int32_t interruptible);

#if defined(__cplusplus)
}
#endif
