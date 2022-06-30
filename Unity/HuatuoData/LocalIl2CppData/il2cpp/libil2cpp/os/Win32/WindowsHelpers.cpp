#include "il2cpp-config.h"
#include "WindowsHelpers.h"
#include "os/Time.h"

#if IL2CPP_TARGET_WINDOWS

namespace il2cpp
{
namespace os
{
namespace win
{
    WaitStatus WaitForSingleObjectAndAccountForAPCs(HANDLE handle, uint32_t ms, bool interruptible)
    {
        uint32_t remainingWaitTimeMS = ms;
        while (true)
        {
            uint32_t waitStartTime = os::Time::GetTicksMillisecondsMonotonic();
            DWORD result = ::WaitForSingleObjectEx(handle, remainingWaitTimeMS, interruptible);

            if (result == WAIT_OBJECT_0)
                return kWaitStatusSuccess;

            if (result == WAIT_TIMEOUT)
                return kWaitStatusTimeout;

            if (result == WAIT_IO_COMPLETION)
            {
                if (ms != INFINITE)
                {
                    uint32_t haveWaitedTimeMS = os::Time::GetTicksMillisecondsMonotonic() - waitStartTime;
                    if (haveWaitedTimeMS >= remainingWaitTimeMS)
                        return kWaitStatusTimeout;
                    remainingWaitTimeMS -= haveWaitedTimeMS;
                }
                continue;
            }

            break;
        }

        return kWaitStatusFailure;
    }

    int32_t WaitForAnyObjectAndAccountForAPCs(const std::vector<HANDLE>& handles, uint32_t ms, bool interruptible)
    {
        IL2CPP_ASSERT(handles.size() != 0);

        uint32_t remainingWaitTimeMS = ms;
        while (true)
        {
            uint32_t waitStartTime = os::Time::GetTicksMillisecondsMonotonic();
            DWORD result = ::WaitForMultipleObjectsEx((DWORD)handles.size(), handles.data(), false, remainingWaitTimeMS, interruptible);

            // If we are waiting for just one of many objects, the return value
            // might be WAIT_OBJECT_0 + the index of the handle that was signaled.
            // Check for a return value in that range and return the index of that handle.
            if (result >= WAIT_OBJECT_0 && result < WAIT_OBJECT_0 + handles.size())
                return result - WAIT_OBJECT_0;

            if (result == WAIT_TIMEOUT)
                return WAIT_TIMEOUT;

            if (result == WAIT_IO_COMPLETION)
            {
                if (ms != INFINITE)
                {
                    uint32_t haveWaitedTimeMS = os::Time::GetTicksMillisecondsMonotonic() - waitStartTime;
                    if (haveWaitedTimeMS >= remainingWaitTimeMS)
                        return WAIT_TIMEOUT;
                    remainingWaitTimeMS -= haveWaitedTimeMS;
                }
                continue;
            }

            break;
        }

        return WAIT_FAILED;
    }

    bool WaitForAllObjectsAndAccountForAPCs(const std::vector<HANDLE>& handles, uint32_t ms, bool interruptible)
    {
        IL2CPP_ASSERT(handles.size() != 0);

        uint32_t remainingWaitTimeMS = ms;
        while (true)
        {
            uint32_t waitStartTime = os::Time::GetTicksMillisecondsMonotonic();
            DWORD result = ::WaitForMultipleObjectsEx((DWORD)handles.size(), handles.data(), true, remainingWaitTimeMS, interruptible);

            if (result == WAIT_OBJECT_0)
                return true;

            if (result == WAIT_TIMEOUT)
                return false;

            if (result == WAIT_IO_COMPLETION)
            {
                if (ms != INFINITE)
                {
                    uint32_t haveWaitedTimeMS = os::Time::GetTicksMillisecondsMonotonic() - waitStartTime;
                    if (haveWaitedTimeMS >= remainingWaitTimeMS)
                        return false;
                    remainingWaitTimeMS -= haveWaitedTimeMS;
                }
                continue;
            }

            break;
        }

        return false;
    }
}
}
}

#endif // IL2CPP_TARGET_WINDOWS
