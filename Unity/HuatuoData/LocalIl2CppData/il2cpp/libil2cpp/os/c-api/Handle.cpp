#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/Handle-c-api.h"
#include "os/Handle.h"
#include <vector>

extern "C"
{
    void UnityPalHandleDestroy(UnityPalHandle* handle)
    {
        delete handle;
    }

    UnityPalWaitStatus UnityPalHandleWait(UnityPalHandle* handle, int32_t interruptible)
    {
        return handle->Wait((bool)interruptible);
    }

    UnityPalWaitStatus UnityPalHandleWaitMs(UnityPalHandle* handle, uint32_t ms, int32_t interruptible)
    {
        return handle->Wait(ms, interruptible);
    }

    UnityPalWaitStatus UnityPalHandleSignalAndWait(UnityPalHandle* toSignal, UnityPalHandle* toWait, uint32_t ms, int32_t interruptible)
    {
        toSignal->Signal();

        return toWait->Wait(ms, interruptible);
    }

    UnityPalWaitStatus UnityPalWaitForMultipleHandles(UnityPalHandle** handles, size_t numberOfHandles, int32_t waitAll, uint32_t ms, int32_t interruptible)
    {
        if (numberOfHandles == 0)
            return kWaitStatusFailure;

        if (numberOfHandles == 1)
            return UnityPalHandleWaitMs(handles[0], ms, interruptible);

        std::vector<il2cpp::os::Handle*> handleVector(numberOfHandles);
        for (size_t i = 0; i < numberOfHandles; ++i)
            handleVector[i] = handles[i];

        if (waitAll)
        {
            return il2cpp::os::Handle::WaitAll(handleVector, ms) ? kWaitStatusSuccess : kWaitStatusTimeout;
        }
        else
        {
            int32_t ret = il2cpp::os::Handle::WaitAny(handleVector, ms);
            if (ret == 258)
                return kWaitStatusTimeout;
            return (UnityPalWaitStatus)(kWaitStatusSuccess + ret);
        }
    }
}

#endif
