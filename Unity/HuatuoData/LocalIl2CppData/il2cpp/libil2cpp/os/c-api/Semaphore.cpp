#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include "os/c-api/Semaphore-c-api.h"
#include "os/Semaphore.h"

extern "C"
{
    UnityPalSemaphore* UnityPalSemaphoreNew(int32_t manualReset, int32_t signaled)
    {
        return new il2cpp::os::Semaphore(manualReset, signaled);
    }

    void UnityPalSemaphoreDelete(UnityPalSemaphore* semaphore)
    {
        IL2CPP_ASSERT(semaphore);
        delete semaphore;
    }

    int32_t UnityPalSemaphorePost(UnityPalSemaphore* semaphore, int32_t releaseCount, int32_t* previousCount)
    {
        IL2CPP_ASSERT(semaphore);
        return semaphore->Post(releaseCount, previousCount);
    }

    UnityPalWaitStatus UnityPalSemaphoreWait(UnityPalSemaphore* semaphore, int32_t interruptible)
    {
        IL2CPP_ASSERT(semaphore);
        return semaphore->Wait((bool)interruptible);
    }

    UnityPalWaitStatus UnityPalSemaphoreWaitMs(UnityPalSemaphore* semaphore, uint32_t ms, int32_t interruptible)
    {
        IL2CPP_ASSERT(semaphore);
        return semaphore->Wait(ms, interruptible);
    }

    UnityPalSemaphoreHandle* UnityPalSemaphoreHandleNew(UnityPalSemaphore* semaphore)
    {
        IL2CPP_ASSERT(semaphore);
        return new UnityPalSemaphoreHandle(semaphore);
    }

    void UnityPalSemaphoreHandleDelete(UnityPalSemaphoreHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        delete handle;
    }

    int32_t UnityPalSemaphoreHandleWait(UnityPalSemaphoreHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait();
    }

    int32_t UnityPalSemaphoreHandleWaitMs(UnityPalSemaphoreHandle* handle, uint32_t ms)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait(ms);
    }

    void UnityPalSemaphoreHandleSignal(UnityPalSemaphoreHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        handle->Signal();
    }

    UnityPalSemaphore* UnityPalSemaphoreHandleGet(UnityPalSemaphoreHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return &handle->Get();
    }
}

#endif
