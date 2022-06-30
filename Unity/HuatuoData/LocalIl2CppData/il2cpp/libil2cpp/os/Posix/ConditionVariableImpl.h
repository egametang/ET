#pragma once
#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include <pthread.h>
#include "utils/NonCopyable.h"

class FastMutexImpl;

namespace il2cpp
{
namespace os
{
    class ConditionVariableImpl : public il2cpp::utils::NonCopyable
    {
    public:
        ConditionVariableImpl();
        ~ConditionVariableImpl();

        int Wait(FastMutexImpl* lock);
        int TimedWait(FastMutexImpl* lock, uint32_t timeout_ms);
        void Broadcast();
        void Signal();

    private:
        pthread_cond_t m_ConditionVariable;
    };
}
}

#endif
