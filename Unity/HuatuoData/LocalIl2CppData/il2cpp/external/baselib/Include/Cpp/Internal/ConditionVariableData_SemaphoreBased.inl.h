#pragma once

#include "../Atomic.h"
#include "../Semaphore.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace detail
        {
            struct ConditionVariableData
            {
                Semaphore           semaphore;
                atomic<uint32_t>    waiters;

                ConditionVariableData() : semaphore(), waiters(0) {}

                inline bool HasWaiters() const
                {
                    return waiters.load(memory_order_acquire) > 0;
                }
            };
        }
    }
}
