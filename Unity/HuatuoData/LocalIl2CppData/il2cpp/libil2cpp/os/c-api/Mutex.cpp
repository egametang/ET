#include "il2cpp-config.h"

#if !RUNTIME_TINY

#include "os/c-api/Mutex-c-api.h"

extern "C"
{
    UnityPalMutex* UnityPalMutexNew(int32_t initiallyOwned)
    {
        return new UnityPalMutex(initiallyOwned);
    }

    void UnityPalMutexDelete(UnityPalMutex* mutex)
    {
        IL2CPP_ASSERT(mutex);
        delete mutex;
    }

    void UnityPalMutexLock(UnityPalMutex* mutex, int32_t interruptible)
    {
        IL2CPP_ASSERT(mutex);
        mutex->Lock(interruptible);
    }

    int32_t UnityPalMutexTryLock(UnityPalMutex* mutex, uint32_t milliseconds, int32_t interruptible)
    {
        IL2CPP_ASSERT(mutex);
        return mutex->TryLock(milliseconds, interruptible);
    }

    void UnityPalMutexUnlock(UnityPalMutex* mutex)
    {
        IL2CPP_ASSERT(mutex);
        mutex->Unlock();
    }

    UnityPalMutexHandle* UnityPalMutexHandleNew(UnityPalMutex* mutex)
    {
        IL2CPP_ASSERT(mutex);
        return new UnityPalMutexHandle(mutex);
    }

    void UnityPalMutexHandleDelete(UnityPalMutexHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        delete handle;
    }

    int32_t UnityPalMutexHandleWait(UnityPalMutexHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait();
    }

    int32_t UnityPalMutexHandleWaitMs(UnityPalMutexHandle* handle, uint32_t ms)
    {
        IL2CPP_ASSERT(handle);
        return handle->Wait(ms);
    }

    void UnityPalMutexHandleSignal(UnityPalMutexHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        handle->Signal();
    }

    UnityPalMutex* UnityPalMutexHandleGet(UnityPalMutexHandle* handle)
    {
        IL2CPP_ASSERT(handle);
        return handle->Get();
    }

    UnityPalFastMutex* UnityPalFastMutexNew()
    {
        return new UnityPalFastMutex();
    }

    void UnityPalFastMutexDelete(UnityPalFastMutex* fastMutex)
    {
        IL2CPP_ASSERT(fastMutex);
        delete fastMutex;
    }

    void UnityPalFastMutexLock(UnityPalFastMutex* fastMutex)
    {
        IL2CPP_ASSERT(fastMutex);
        fastMutex->Lock();
    }

    void UnityPalFastMutexUnlock(UnityPalFastMutex* fastMutex)
    {
        IL2CPP_ASSERT(fastMutex);
        fastMutex->Unlock();
    }

    UnityPalFastMutexImpl* UnityPalFastMutexGetImpl(UnityPalFastMutex* fastMutex)
    {
        IL2CPP_ASSERT(fastMutex);
        return fastMutex->GetImpl();
    }
}

#endif
