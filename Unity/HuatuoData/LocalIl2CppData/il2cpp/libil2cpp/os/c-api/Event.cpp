#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include "os/c-api/Event-c-api.h"
#include "os/Event.h"

extern "C"
{
    UnityPalEvent* UnityPalEventNew(int32_t manualReset, int32_t signaled)
    {
        return new il2cpp::os::Event(manualReset, signaled);
    }

    void UnityPalEventDelete(UnityPalEvent* event)
    {
        IL2CPP_ASSERT(event);
        delete event;
    }

    UnityPalErrorCode UnityPalEventSet(UnityPalEvent* event)
    {
        IL2CPP_ASSERT(event);
        return event->Set();
    }

    UnityPalErrorCode UnityPalEventReset(UnityPalEvent* event)
    {
        IL2CPP_ASSERT(event);
        return event->Reset();
    }

    UnityPalWaitStatus UnityPalEventWait(UnityPalEvent* event, int32_t interruptible)
    {
        IL2CPP_ASSERT(event);
        return event->Wait((bool)interruptible);
    }

    UnityPalWaitStatus UnityPalEventWaitMs(UnityPalEvent* event, uint32_t ms, int32_t interruptible)
    {
        IL2CPP_ASSERT(event);
        return event->Wait(ms, interruptible);
    }

    UnityPalEventHandle* UnityPalEventHandleNew(UnityPalEvent* event)
    {
        IL2CPP_ASSERT(event);
        return new UnityPalEventHandle(event);
    }

    void UnityPalEventHandleDelete(UnityPalEventHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        delete handle;
    }

    int32_t UnityPalEventHandleWait(UnityPalEventHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait();
    }

    int32_t UnityPalEventHandleWaitMs(UnityPalEventHandle* handle, uint32_t ms)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait(ms);
    }

    void UnityPalEventHandleSignal(UnityPalEventHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        handle->Signal();
    }

    UnityPalEvent* UnityPalEventHandleGet(UnityPalEventHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return &handle->Get();
    }
}

#endif
