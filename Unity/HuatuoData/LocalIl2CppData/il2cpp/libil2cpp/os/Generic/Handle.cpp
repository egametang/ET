#include "il2cpp-config.h"

#if !IL2CPP_TARGET_WINDOWS

#include "os/Handle.h"

#if IL2CPP_SUPPORT_THREADS

#include <algorithm>
#include "os/Thread.h"

namespace il2cpp
{
namespace os
{
    int32_t Handle::WaitAny(const std::vector<Handle*>& handles, int32_t ms)
    {
        int timeWaitedMs = 0;
        while (ms == -1 || timeWaitedMs <= ms)
        {
            int32_t numberOfOsHandles = (int32_t)handles.size();
            for (int32_t i = 0; i < numberOfOsHandles; ++i)
            {
                if (handles[i]->Wait(0U))
                    return i;
            }

            os::Thread::Sleep(m_waitIntervalMs, true);
            timeWaitedMs += m_waitIntervalMs;
        }

        return 258; // WAIT_TIMEOUT value
    }

    bool Handle::WaitAll(std::vector<Handle*>& handles, int32_t ms)
    {
        int timeWaitedMs = 0;
        while (ms == -1 || timeWaitedMs <= ms)
        {
            size_t numberOfOsHandles = handles.size();
            std::vector<Handle*> signaledHandles;
            for (size_t i = 0; i < numberOfOsHandles; ++i)
            {
                if (handles[i]->Wait(0U))
                    signaledHandles.push_back(handles[i]);
            }

            if (signaledHandles.size() == numberOfOsHandles)
                return true; // All handles have been signaled

            for (size_t i = 0; i < signaledHandles.size(); ++i)
                handles.erase(std::remove(handles.begin(), handles.end(), signaledHandles[i]), handles.end());

            os::Thread::Sleep(m_waitIntervalMs, true);
            timeWaitedMs += m_waitIntervalMs;
        }

        return false; // Timed out waiting for all handles to be signaled
    }
} // namespace os
} // naemspace il2cpp

#else

namespace il2cpp
{
namespace os
{
    int32_t Handle::WaitAny(const std::vector<Handle*>& handles, int32_t ms)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return 0;
    }

    bool Handle::WaitAll(std::vector<Handle*>& handles, int32_t ms)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return false;
    }
} // namespace os
} // naemspace il2cpp

#endif

#endif
