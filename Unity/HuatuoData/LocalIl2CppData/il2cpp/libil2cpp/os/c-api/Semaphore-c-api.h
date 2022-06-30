#pragma once

#include "WaitStatus-c-api.h"

#if defined(__cplusplus)
#include "os/Semaphore.h"
typedef il2cpp::os::Semaphore UnityPalSemaphore;
typedef il2cpp::os::SemaphoreHandle UnityPalSemaphoreHandle;
#else
typedef struct UnityPalSemaphore UnityPalSemaphore;
typedef struct UnityPalSemaphoreHandle UnityPalSemaphoreHandle;
#endif


#if defined(__cplusplus)
extern "C"
{
#endif

UnityPalSemaphore* UnityPalSemaphoreNew(int32_t initialValue, int32_t maximumValue);
void UnityPalSemaphoreDelete(UnityPalSemaphore* semaphore);
int32_t UnityPalSemaphorePost(UnityPalSemaphore* semaphore, int32_t releaseCount, int32_t* previousCount);
UnityPalWaitStatus UnityPalSemaphoreWait(UnityPalSemaphore* semaphore, int32_t interruptible);
UnityPalWaitStatus UnityPalSemaphoreWaitMs(UnityPalSemaphore* semaphore, uint32_t ms, int32_t interruptible);

UnityPalSemaphoreHandle* UnityPalSemaphoreHandleNew(UnityPalSemaphore* semaphore);
void UnityPalSemaphoreHandleDelete(UnityPalSemaphoreHandle* handle);
int32_t UnityPalSemaphoreHandleWait(UnityPalSemaphoreHandle* handle);
int32_t UnityPalSemaphoreHandleWaitMs(UnityPalSemaphoreHandle* handle, uint32_t ms);
void UnityPalSemaphoreHandleSignal(UnityPalSemaphoreHandle* handle);
UnityPalSemaphore* UnityPalSemaphoreHandleGet(UnityPalSemaphoreHandle* handle);

#if defined(__cplusplus)
}
#endif
