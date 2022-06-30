#include "os/c-api/il2cpp-config-platforms.h"

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "MutexImpl.h"
#include "PosixHelpers.h"
#include "os/Thread.h"

namespace il2cpp
{
namespace os
{
    MutexImpl::MutexImpl()
        : posix::PosixWaitObject(kMutex)
        , m_OwningThread(NULL)
        , m_RecursionCount(0)
    {
        // For a mutex, 1 means unlocked.
        m_Count = 1;
    }

    void MutexImpl::Lock(bool interruptible)
    {
        TryLock(posix::kNoTimeout, interruptible);
    }

    bool MutexImpl::TryLock(uint32_t milliseconds, bool interruptible)
    {
        Thread* currentThread = Thread::GetCurrentThread();
        if (m_OwningThread == currentThread)
        {
            IL2CPP_ASSERT(m_Count == 0);
            ++m_RecursionCount;
            return true;
        }

        if (Wait(milliseconds, interruptible) == kWaitStatusSuccess)
        {
            m_OwningThread = currentThread;
            m_RecursionCount = 1;
            return true;
        }

        return false;
    }

    void MutexImpl::Unlock()
    {
        IL2CPP_ASSERT(m_OwningThread == Thread::GetCurrentThread());

        // Undo one locking level.
        --m_RecursionCount;
        if (m_RecursionCount > 0)
        {
            // Still locked.
            return;
        }

        // Ok, we're releasing the mutex. Lock and signal. We don't absolutely
        // need the lock as we are already owning the mutex here but play it safe.
        posix::PosixAutoLock lock(&m_Mutex);

        IL2CPP_ASSERT(m_Count == 0);
        m_Count = 1; // Unintuitive but 1 means unlocked.
        m_OwningThread = NULL;

        // Signal condition so that either a thread that's already waiting or a thread that
        // comes around later and waits can claim the mutex.
        if (HaveWaitingThreads())
            pthread_cond_signal(&m_Condition);
    }
}
}

#endif
