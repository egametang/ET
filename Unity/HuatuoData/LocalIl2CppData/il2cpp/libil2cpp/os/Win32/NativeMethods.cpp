#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHelpers.h"

#include "il2cpp-vm-support.h"
#include "os/NativeMethods.h"

namespace il2cpp
{
namespace os
{
    bool NativeMethods::CloseProcess(ProcessHandle* handle)
    {
        return ::CloseHandle(handle) != FALSE;
    }

    bool NativeMethods::GetExitCodeProcess(ProcessHandle* handle, int32_t* exitCode)
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        return ::GetExitCodeProcess((HANDLE)handle, (LPDWORD)exitCode);
#else
        IL2CPP_VM_NOT_SUPPORTED("GetExitCodeProcess", "Getting process exit code is not supported on WinRT based platforms.");
        return FALSE;
#endif
    }

    int32_t NativeMethods::GetCurrentProcessId()
    {
        return ::GetCurrentProcessId();
    }

    ProcessHandle* NativeMethods::GetCurrentProcess()
    {
        return (ProcessHandle*)::GetCurrentProcess();
    }
}
}

#endif
