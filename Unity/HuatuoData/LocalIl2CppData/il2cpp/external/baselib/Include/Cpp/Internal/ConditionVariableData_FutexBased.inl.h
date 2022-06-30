#pragma once

#include "../Atomic.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace detail
        {
            struct ConditionVariableData
            {
                atomic<int32_t>     waiters;
                atomic<int32_t>     wakeups;

                ConditionVariableData() : waiters(0), wakeups(0) {}

                inline bool HasWaiters() const
                {
                    return waiters.load(memory_order_acquire) > 0;
                }

                inline bool TryConsumeWakeup()
                {
                    int32_t previousCount = wakeups.load(memory_order_relaxed);
                    while (previousCount > 0)
                    {
                        if (wakeups.compare_exchange_weak(previousCount, previousCount - 1, memory_order_acquire, memory_order_relaxed))
                        {
                            return true;
                        }
                    }
                    return false;
                }
            };
        }
    }
}
