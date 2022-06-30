#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHelpers.h"

#include "os/NativeMethods.h"
#include "utils/Expected.h"
#include "utils/Il2CppError.h"

namespace il2cpp
{
namespace os
{
    bool NativeMethods::CloseProcess(ProcessHandle* handle)
    {
        return ::CloseHandle(handle) != FALSE;
    }

    utils::Expected<bool> NativeMethods::GetExitCodeProcess(ProcessHandle* handle, int32_t* exitCode)
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        return ::GetExitCodeProcess((HANDLE)handle, (LPDWORD)exitCode);
#else
        return utils::Il2CppError(utils::NotSupported, "Getting process exit code is not supported on WinRT based platforms.");
#endif
    }

    int32_t NativeMethods::GetCurrentProcessId()
    {
        return ::GetCurrentProcessId();
    }

    utils::Expected<ProcessHandle*> NativeMethods::GetCurrentProcess()
    {
        return (ProcessHandle*)::GetCurrentProcess();
    }
}
}

#endif
