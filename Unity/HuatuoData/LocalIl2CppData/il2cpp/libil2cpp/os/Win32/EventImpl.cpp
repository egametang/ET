#include "il2cpp-config.h"

#if IL2CPP_THREADS_WIN32

#include "EventImpl.h"
#include "WindowsHelpers.h"

namespace il2cpp
{
namespace os
{
    EventImpl::EventImpl(bool manualReset, bool signaled)
    {
        m_Event = ::CreateEvent(NULL, manualReset ? TRUE : FALSE, signaled ? TRUE : FALSE, NULL);

        IL2CPP_ASSERT(m_Event);
    }

    EventImpl::~EventImpl()
    {
        IL2CPP_ASSERT(m_Event);

        ::CloseHandle(m_Event);
    }

    ErrorCode EventImpl::Set()
    {
        if (::SetEvent(m_Event))
            return kErrorCodeSuccess;

        return kErrorCodeGenFailure;
    }

    ErrorCode EventImpl::Reset()
    {
        if (::ResetEvent(m_Event))
            return kErrorCodeSuccess;

        return kErrorCodeGenFailure;
    }

    WaitStatus EventImpl::Wait(bool interruptible)
    {
        return Wait(INFINITE, interruptible);
    }

    WaitStatus EventImpl::Wait(uint32_t ms, bool interruptible)
    {
        return il2cpp::os::win::WaitForSingleObjectAndAccountForAPCs(m_Event, ms, interruptible);
    }

    void* EventImpl::GetOSHandle()
    {
        return (void*)m_Event;
    }
}
}

#endif
