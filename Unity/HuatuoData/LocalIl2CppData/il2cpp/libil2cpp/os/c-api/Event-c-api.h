#pragma once

#include "Error-c-api.h"
#include "WaitStatus-c-api.h"

#if defined(__cplusplus)
#include "os/Event.h"
typedef il2cpp::os::Event UnityPalEvent;
typedef il2cpp::os::EventHandle UnityPalEventHandle;
#else
typedef struct UnityPalEvent UnityPalEvent;
typedef struct UnityPalEventHandle UnityPalEventHandle;
#endif


#if defined(__cplusplus)
extern "C"
{
#endif

UnityPalEvent* UnityPalEventNew(int32_t manualReset, int32_t signaled);
void UnityPalEventDelete(UnityPalEvent* event);
UnityPalErrorCode UnityPalEventSet(UnityPalEvent* event);
UnityPalErrorCode UnityPalEventReset(UnityPalEvent* event);
UnityPalWaitStatus UnityPalEventWait(UnityPalEvent* event, int32_t interruptible);
UnityPalWaitStatus UnityPalEventWaitMs(UnityPalEvent* event, uint32_t ms, int32_t interruptible);

UnityPalEventHandle* UnityPalEventHandleNew(UnityPalEvent* Event);
void UnityPalEventHandleDelete(UnityPalEventHandle* Event);
int32_t UnityPalEventHandleWait(UnityPalEventHandle* handle);
int32_t UnityPalEventHandleWaitMs(UnityPalEventHandle* handle, uint32_t ms);
void UnityPalEventHandleSignal(UnityPalEventHandle* handle);
UnityPalEvent* UnityPalEventHandleGet(UnityPalEventHandle* handle);

#if defined(__cplusplus)
}
#endif
