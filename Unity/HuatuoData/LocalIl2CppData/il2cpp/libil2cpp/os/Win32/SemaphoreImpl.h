#pragma once

#if IL2CPP_THREADS_WIN32

#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

#include "WindowsHeaders.h"

namespace il2cpp
{
namespace os
{
    class SemaphoreImpl : public il2cpp::utils::NonCopyable
    {
    public:
        SemaphoreImpl(int32_t initialValue, int32_t maximumValue);
        ~SemaphoreImpl();
        bool Post(int32_t releaseCount, int32_t* previousCount = NULL);
        WaitStatus Wait(bool interruptible);
        WaitStatus Wait(uint32_t ms, bool interruptible);
        void* GetOSHandle();
    private:
        HANDLE m_Handle;
    };
}
}

#endif
