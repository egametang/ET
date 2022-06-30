#include "il2cpp-config.h"
#include "os/Event.h"

#if IL2CPP_SUPPORT_THREADS

#if IL2CPP_THREADS_WIN32
#include "os/Win32/EventImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/EventImpl.h"
#else
#include "os/EventImpl.h"
#endif


namespace il2cpp
{
namespace os
{
    Event::Event(bool manualReset, bool signaled)
        : m_Event(new EventImpl(manualReset, signaled))
    {
    }

    Event::~Event()
    {
        delete m_Event;
    }

    ErrorCode Event::Set()
    {
        return m_Event->Set();
    }

    ErrorCode Event::Reset()
    {
        return m_Event->Reset();
    }

    WaitStatus Event::Wait(bool interruptible)
    {
        return m_Event->Wait(interruptible);
    }

    WaitStatus Event::Wait(uint32_t ms, bool interruptible)
    {
        return m_Event->Wait(ms, interruptible);
    }

    void* Event::GetOSHandle()
    {
        return m_Event->GetOSHandle();
    }
}
}

#else

namespace il2cpp
{
namespace os
{
    Event::Event(bool manualReset, bool signaled)
    {
    }

    Event::~Event()
    {
    }

    ErrorCode Event::Set()
    {
        return kErrorCodeSuccess;
    }

    ErrorCode Event::Reset()
    {
        return kErrorCodeSuccess;
    }

    WaitStatus Event::Wait(bool interruptible)
    {
        return kWaitStatusSuccess;
    }

    WaitStatus Event::Wait(uint32_t ms, bool interruptible)
    {
        return kWaitStatusSuccess;
    }

    void* Event::GetOSHandle()
    {
        return NULL;
    }
}
}

#endif
