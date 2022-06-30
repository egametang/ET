#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/Handle.h"
#include "WindowsHelpers.h"

namespace il2cpp
{
namespace os
{
    static std::vector<HANDLE> GetOSHandles(const std::vector<Handle*>& handles)
    {
        std::vector<HANDLE> osHandles;
        for (size_t i = 0; i < handles.size(); ++i)
            osHandles.push_back((HANDLE)handles[i]->GetOSHandle());

        return osHandles;
    }

    static uint32_t GetOSWaitTime(int32_t ms)
    {
        return ms == -1 ? INFINITE : ms;
    }

    int32_t Handle::WaitAny(const std::vector<Handle*>& handles, int32_t ms)
    {
        return il2cpp::os::win::WaitForAnyObjectAndAccountForAPCs(GetOSHandles(handles), GetOSWaitTime(ms), true);
    }

    bool Handle::WaitAll(std::vector<Handle*>& handles, int32_t ms)
    {
        return il2cpp::os::win::WaitForAllObjectsAndAccountForAPCs(GetOSHandles(handles), GetOSWaitTime(ms), true);
    }
} // namespace os
} // naemspace il2cpp

#endif
