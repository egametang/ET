#include "il2cpp-config.h"

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "EventImpl.h"
#include "PosixHelpers.h"

namespace il2cpp
{
namespace os
{
    EventImpl::EventImpl(bool manualReset, bool signaled)
        : posix::PosixWaitObject(manualReset ? kManualResetEvent : kAutoResetEvent)
    {
        if (signaled)
            m_Count = 1;
    }

    ErrorCode EventImpl::Set()
    {
        posix::PosixAutoLock lock(&m_Mutex);

        m_Count = 1;

        if (HaveWaitingThreads())
            pthread_cond_broadcast(&m_Condition);

        return kErrorCodeSuccess;
    }

    ErrorCode EventImpl::Reset()
    {
        posix::PosixAutoLock lock(&m_Mutex);
        m_Count = 0;

        return kErrorCodeSuccess;
    }
}
}

#endif
