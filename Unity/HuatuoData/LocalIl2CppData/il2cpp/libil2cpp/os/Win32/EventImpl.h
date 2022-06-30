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
    class EventImpl : public il2cpp::utils::NonCopyable
    {
    public:
        EventImpl(bool manualReset = false, bool signaled = false);
        ~EventImpl();

        ErrorCode Set();
        ErrorCode Reset();
        WaitStatus Wait(bool interruptible);
        WaitStatus Wait(uint32_t ms, bool interruptible);
        void* GetOSHandle();

    private:
        HANDLE m_Event;
    };
}
}

#endif
