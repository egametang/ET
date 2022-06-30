#pragma once

#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"
#include "os/Handle.h"
#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace os
{
    class SemaphoreImpl;

    class Semaphore : public il2cpp::utils::NonCopyable
    {
    public:
        Semaphore(int32_t initialValue = 0, int32_t maximumValue = 1);
        ~Semaphore();

        bool Post(int32_t releaseCount = 1, int32_t* previousCount = NULL);
        WaitStatus Wait(bool interruptible = false);
        WaitStatus Wait(uint32_t ms, bool interruptible = false);
        void* GetOSHandle();

    private:
        SemaphoreImpl* m_Semaphore;
    };

    class SemaphoreHandle : public Handle
    {
    public:
        SemaphoreHandle(Semaphore* semaphore) : m_Semaphore(semaphore) {}
        virtual ~SemaphoreHandle() { delete m_Semaphore; }
        virtual bool Wait() { m_Semaphore->Wait(true); return true; }
        virtual bool Wait(uint32_t ms) { return m_Semaphore->Wait(ms, true) != kWaitStatusTimeout; }
        virtual WaitStatus Wait(bool interruptible) { return m_Semaphore->Wait(interruptible); }
        virtual WaitStatus Wait(uint32_t ms, bool interruptible) { return m_Semaphore->Wait(ms, interruptible); }
        virtual void Signal() { m_Semaphore->Post(1, NULL); }
        virtual void* GetOSHandle() { return m_Semaphore->GetOSHandle(); }
        Semaphore& Get() { return *m_Semaphore; }

    private:
        Semaphore* m_Semaphore;
    };
}
}
