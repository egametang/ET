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
    class EventImpl : public posix::PosixWaitObject
    {
    public:
        EventImpl(bool manualReset = false, bool signaled = false);

        ErrorCode Set();
        ErrorCode Reset();
    };
}
}

#endif
