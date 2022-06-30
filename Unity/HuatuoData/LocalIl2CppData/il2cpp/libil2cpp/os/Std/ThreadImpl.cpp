#include "il2cpp-config.h"

#if IL2CPP_THREADS_STD

#include "os/Thread.h"
#include "ThreadImpl.h"

#include <thread>

namespace il2cpp
{
namespace os
{
    struct StartData
    {
        Thread::StartFunc m_StartFunc;
        void* m_StartArg;
    };


    static void ThreadStartWrapper(void* arg)
    {
        StartData* startData = (StartData*)arg;
        startData->m_StartFunc(startData->m_StartArg);

        free(startData);
    }

    uint64_t ThreadImpl::Id()
    {
        return m_Thread.get_id().hash();
    }

    ErrorCode ThreadImpl::Run(Thread::StartFunc func, void* arg, int64_t affinityMask)
    {
        StartData* startData = (StartData*)malloc(sizeof(StartData));
        startData->m_StartFunc = func;
        startData->m_StartArg = arg;

        std::thread t(ThreadStartWrapper, startData);
        if (affinityMask != Thread::kThreadAffinityAll)
        {
            IL2CPP_ASSERT(0 && "Using non-default thread affinity is not supported on the STD implementation.");
        }

        m_Thread.swap(t);

        return kErrorCodeSuccess;
    }

    WaitStatus ThreadImpl::Join(uint32_t ms)
    {
        m_Thread.join();

        return kWaitStatusSuccess;
    }

    ErrorCode ThreadImpl::Sleep(uint32_t milliseconds)
    {
        std::chrono::milliseconds dura(milliseconds);
        std::this_thread::sleep_for(dura);

        return kErrorCodeSuccess;
    }

    uint64_t ThreadImpl::CurrentThreadId()
    {
        return std::this_thread::get_id().hash();
    }
}
}

#endif
