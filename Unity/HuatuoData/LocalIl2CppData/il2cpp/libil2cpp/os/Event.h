#pragma once

#include "os/ErrorCodes.h"
#include "os/Handle.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace os
{
    class EventImpl;

    class Event : public il2cpp::utils::NonCopyable
    {
    public:
        Event(bool manualReset = false, bool signaled = false);
        ~Event();

        ErrorCode Set();
        ErrorCode Reset();
        WaitStatus Wait(bool interruptible = false);
        WaitStatus Wait(uint32_t ms, bool interruptible = false);
        void* GetOSHandle();

    private:
        EventImpl* m_Event;
    };

    class EventHandle : public Handle
    {
    public:
        EventHandle(Event* event)
            : m_Event(event) {}

        virtual ~EventHandle() { delete m_Event; }
        virtual bool Wait() { m_Event->Wait(true); return true; }
        virtual bool Wait(uint32_t ms) { return m_Event->Wait(ms, true) != kWaitStatusTimeout; }
        virtual WaitStatus Wait(bool interruptible) { return m_Event->Wait(interruptible); }
        virtual WaitStatus Wait(uint32_t ms, bool interruptible) { return m_Event->Wait(ms, interruptible); }
        virtual void Signal() { m_Event->Set(); }
        virtual void* GetOSHandle() { return m_Event->GetOSHandle(); }
        Event& Get() { return *m_Event; }

    private:
        Event* m_Event;
    };
}
}
