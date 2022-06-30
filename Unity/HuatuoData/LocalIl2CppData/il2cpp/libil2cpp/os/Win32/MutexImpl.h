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
    class MutexImpl : public il2cpp::utils::NonCopyable
    {
    public:
        MutexImpl();
        ~MutexImpl();

        void Lock(bool interruptible);
        bool TryLock(uint32_t milliseconds, bool interruptible);
        void Unlock();
        void* GetOSHandle();

    private:
        HANDLE m_MutexHandle;
    };

    class FastMutexImpl
    {
    public:
        FastMutexImpl()
        {
            InitializeCriticalSection(&m_CritialSection);
        }

        ~FastMutexImpl()
        {
            DeleteCriticalSection(&m_CritialSection);
        }

        void Lock()
        {
            EnterCriticalSection(&m_CritialSection);
        }

        void Unlock()
        {
            LeaveCriticalSection(&m_CritialSection);
        }

        CRITICAL_SECTION* GetOSHandle()
        {
            return &m_CritialSection;
        }

    private:
        CRITICAL_SECTION m_CritialSection;
    };
}
}

#endif
