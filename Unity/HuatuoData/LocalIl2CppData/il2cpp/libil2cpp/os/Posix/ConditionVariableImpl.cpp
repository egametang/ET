#include "os/c-api/il2cpp-config-platforms.h"
#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "MutexImpl.h"
#include "ConditionVariableImpl.h"
#include <time.h>
#include <sys/time.h>

namespace il2cpp
{
namespace os
{
    ConditionVariableImpl::ConditionVariableImpl()
    {
        pthread_cond_init(&m_ConditionVariable, NULL);
    }

    ConditionVariableImpl::~ConditionVariableImpl()
    {
        pthread_cond_destroy(&m_ConditionVariable);
    }

    int ConditionVariableImpl::Wait(FastMutexImpl* lock)
    {
        return pthread_cond_wait(&m_ConditionVariable, lock->GetOSHandle());
    }

    int ConditionVariableImpl::TimedWait(FastMutexImpl* lock, uint32_t timeout_ms)
    {
        struct timeval tv;
        struct timespec ts;
        int64_t usecs;
        int res;

        if (timeout_ms == (uint32_t)0xFFFFFFFF)
            return pthread_cond_wait(&m_ConditionVariable, lock->GetOSHandle());

        /* ms = 10^-3, us = 10^-6, ns = 10^-9 */

        gettimeofday(&tv, NULL);
        tv.tv_sec += timeout_ms / 1000;
        usecs = tv.tv_usec + ((timeout_ms % 1000) * 1000);
        if (usecs >= 1000000)
        {
            usecs -= 1000000;
            tv.tv_sec++;
        }
        ts.tv_sec = tv.tv_sec;
        ts.tv_nsec = usecs * 1000;

        return pthread_cond_timedwait(&m_ConditionVariable, lock->GetOSHandle(), &ts);
    }

    void ConditionVariableImpl::Broadcast()
    {
        pthread_cond_broadcast(&m_ConditionVariable);
    }

    void ConditionVariableImpl::Signal()
    {
        pthread_cond_signal(&m_ConditionVariable);
    }
}
}

#endif
