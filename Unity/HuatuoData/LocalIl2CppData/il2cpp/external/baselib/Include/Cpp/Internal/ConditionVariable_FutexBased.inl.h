#pragma once

#include "../CountdownTimer.h"
#include "../../C/Baselib_SystemFutex.h"
#include "../../C/Baselib_Thread.h"

#if !PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "Only use this implementation on top of a proper futex, in all other situations us ConditionVariable_SemaphoreBased.inl.h"
#endif

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        inline void ConditionVariable::Wait()
        {
            m_Data.waiters.fetch_add(1, memory_order_relaxed);
            m_Lock.Release();
            while (!m_Data.TryConsumeWakeup())
            {
                Baselib_SystemFutex_Wait(&m_Data.wakeups.obj, 0, std::numeric_limits<uint32_t>::max());
            }
            m_Lock.Acquire();
        }

        inline bool ConditionVariable::TimedWait(const timeout_ms timeoutInMilliseconds)
        {
            m_Data.waiters.fetch_add(1, memory_order_relaxed);
            m_Lock.Release();

            uint32_t timeLeft = timeoutInMilliseconds.count();
            auto timer = CountdownTimer::StartNew(timeoutInMilliseconds);
            do
            {
                Baselib_SystemFutex_Wait(&m_Data.wakeups.obj, 0, timeLeft);
                if (m_Data.TryConsumeWakeup())
                {
                    m_Lock.Acquire();
                    return true;
                }
                timeLeft = timer.GetTimeLeftInMilliseconds().count();
            }
            while (timeLeft);

            do
            {
                int32_t waiters = m_Data.waiters.load(memory_order_relaxed);
                while (waiters > 0)
                {
                    if (m_Data.waiters.compare_exchange_weak(waiters, waiters - 1, memory_order_relaxed, memory_order_relaxed))
                    {
                        m_Lock.Acquire();
                        return false;
                    }
                }
                Baselib_Thread_YieldExecution();
            }
            while (!m_Data.TryConsumeWakeup());

            m_Lock.Acquire();
            return true;
        }

        inline void ConditionVariable::Notify(uint16_t count)
        {
            int32_t waitingThreads = m_Data.waiters.load(memory_order_acquire);
            do
            {
                int32_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
                if (threadsToWakeup == 0)
                {
                    atomic_thread_fence(memory_order_release);
                    return;
                }

                if (m_Data.waiters.compare_exchange_weak(waitingThreads, waitingThreads - threadsToWakeup, memory_order_relaxed, memory_order_relaxed))
                {
                    m_Data.wakeups.fetch_add(threadsToWakeup, memory_order_release);
                    Baselib_SystemFutex_Notify(&m_Data.wakeups.obj, threadsToWakeup, Baselib_WakeupFallbackStrategy_OneByOne);
                    return;
                }
            }
            while (waitingThreads > 0);
        }
    }
}
