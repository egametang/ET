#pragma once

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include "PosixWaitObject.h"
#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"

#include <stdint.h>

namespace il2cpp
{
namespace os
{
    class SemaphoreImpl : public posix::PosixWaitObject
    {
    public:
        SemaphoreImpl(int32_t initialValue, int32_t maximumValue);

        bool Post(int32_t releaseCount, int32_t* previousCount);

    protected:
        uint32_t m_MaximumValue;
    };
}
}

#endif
