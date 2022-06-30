#pragma once

#if IL2CPP_THREADS_STD

#include "os/ErrorCodes.h"
#include "os/Thread.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

#include <thread>

namespace il2cpp
{
namespace os
{
    class ThreadImpl : public il2cpp::utils::NonCopyable
    {
    public:
        ThreadImpl();
        ~ThreadImpl();
        uint64_t Id();
        ErrorCode Run(Thread::StartFunc func, void* arg, int64_t affinityMask);
        WaitStatus Join();
        WaitStatus Join(uint32_t ms);
        static WaitStatus Sleep(uint32_t milliseconds);
        static uint64_t CurrentThreadId();
    private:
        std::thread m_Thread;
    };
}
}

#endif
