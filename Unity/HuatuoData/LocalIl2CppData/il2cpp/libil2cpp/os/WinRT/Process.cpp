#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
#include "os/Win32/WindowsHeaders.h"

#include "os/Process.h"
#include "utils/Il2CppError.h"
#include "utils/StringUtils.h"

struct ProcessHandle
{
    HANDLE handle;
};


namespace il2cpp
{
namespace os
{
    int Process::GetCurrentProcessId()
    {
        return ::GetCurrentProcessId();
    }

    utils::Expected<ProcessHandle*> Process::GetProcess(int processId)
    {
        if (processId == GetCurrentProcessId())
            return (ProcessHandle*)::GetCurrentProcess();

        return utils::Il2CppError(utils::NotSupported, "It is not possible to interact with other system processes on current platform.");
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        // We have nothing to do here.
    }

    utils::Expected<std::string> Process::GetProcessName(ProcessHandle* handle)
    {
        if (handle == ::GetCurrentProcess())
        {
            wchar_t path[MAX_PATH + 1];
            SetLastError(ERROR_SUCCESS);

            DWORD pathLength = GetModuleFileNameW(NULL, path, MAX_PATH + 1);
            return utils::StringUtils::Utf16ToUtf8(path, static_cast<int>(pathLength));
        }

        return utils::Il2CppError(utils::NotSupported, "It is not possible to interact with other system processes on current platform.");
    }

    intptr_t Process::GetMainWindowHandle(int32_t pid)
    {
        return 0;
    }
}
}
#endif
