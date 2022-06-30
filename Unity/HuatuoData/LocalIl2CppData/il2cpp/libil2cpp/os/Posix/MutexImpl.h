#pragma once

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"
#include "PosixWaitObject.h"

#include <pthread.h>

namespace il2cpp
{
namespace os
{
    class Thread;

    class MutexImpl : public posix::PosixWaitObject
    {
    public:
        MutexImpl();

        void Lock(bool interruptible);
        bool TryLock(uint32_t milliseconds, bool interruptible);
        void Unlock();

    private:
        /// Thread that currently owns the object. Used for recursion checks.
        Thread* m_OwningThread;

        /// Number of recursive locks on the owning thread.
        uint32_t m_RecursionCount;
    };

    class FastMutexImpl
    {
    public:

        FastMutexImpl()
        {
            pthread_mutexattr_t attr;
            pthread_mutexattr_init(&attr);
            pthread_mutexattr_settype(&attr, PTHREAD_MUTEX_RECURSIVE);
            pthread_mutex_init(&m_Mutex, &attr);
            pthread_mutexattr_destroy(&attr);
        }

        ~FastMutexImpl()
        {
            pthread_mutex_destroy(&m_Mutex);
        }

        void Lock()
        {
            pthread_mutex_lock(&m_Mutex);
        }

        void Unlock()
        {
            pthread_mutex_unlock(&m_Mutex);
        }

        pthread_mutex_t* GetOSHandle()
        {
            return &m_Mutex;
        }

    private:
        pthread_mutex_t m_Mutex;
    };
}
}

#endif
