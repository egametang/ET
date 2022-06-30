#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHeaders.h"
#include "os/Thread.h"

typedef void (WINAPI *GetCurrentThreadStackLimitsPtr)(PULONG_PTR, PULONG_PTR);
static GetCurrentThreadStackLimitsPtr GetCurrentThreadStackLimitsFunc;

namespace il2cpp
{
namespace os
{
    bool Thread::GetCurrentThreadStackBounds(void** low, void** high)
    {
        // On Windows Desktop we still support Windows 7 and GetCurrentThreadStackLimits wasn't added until Windows 8, but GetProcAddress doesn't exist in UWP
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        if (GetCurrentThreadStackLimitsFunc == NULL)
            GetCurrentThreadStackLimitsFunc = (GetCurrentThreadStackLimitsPtr)GetProcAddress(GetModuleHandle(L"KERNEL32.DLL"), "GetCurrentThreadStackLimits");
        if (GetCurrentThreadStackLimitsFunc != NULL)
        {
            GetCurrentThreadStackLimitsFunc((PULONG_PTR)low, (PULONG_PTR)high);
            return true;
        }
        return false;
#else
        return false;
#endif // IL2CPP_TARGET_WINDOWS_DESKTOP
    }
}
}

#endif
