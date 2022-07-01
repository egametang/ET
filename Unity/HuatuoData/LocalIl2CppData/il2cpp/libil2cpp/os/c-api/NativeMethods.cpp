#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/NativeMethods.h"
#include "os/c-api/Process-c-api.h"

extern "C"
{
    int32_t UnityPalNativeCloseProcess(UnityPalProcessHandle* handle)
    {
        return il2cpp::os::NativeMethods::CloseProcess(handle);
    }

    int32_t UnityPalNativeGetExitCodeProcess(UnityPalProcessHandle* handle, int32_t* exitCode)
    {
        return il2cpp::os::NativeMethods::GetExitCodeProcess(handle, exitCode);
    }

    int32_t UnityPalNativeGetCurrentProcessId()
    {
        return il2cpp::os::NativeMethods::GetCurrentProcessId();
    }

    UnityPalProcessHandle* UnityPalNativeGetCurrentProcess()
    {
        return il2cpp::os::NativeMethods::GetCurrentProcess();
    }
}

#endif
