#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINDOWS_GAMES

#include "WindowsHelpers.h"
#include <Psapi.h>

#include "os/Process.h"

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
        return (ProcessHandle*)OpenProcess(PROCESS_ALL_ACCESS, TRUE, processId);
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        ::CloseHandle((HANDLE)handle);
    }

    utils::Expected<std::string> Process::GetProcessName(ProcessHandle* handle)
    {
        const size_t bufferLength = 256;
        WCHAR buf[bufferLength];

        DWORD length = ::GetProcessImageFileName((HANDLE)handle, buf, bufferLength);

        if (length == 0)
            return std::string();

        char multiByteStr[bufferLength];

        size_t numConverted = wcstombs(multiByteStr, buf, bufferLength);
        if (numConverted <= 0)
            return std::string();

        return std::string(multiByteStr, numConverted);
    }

#if IL2CPP_TARGET_WINDOWS_GAMES
    intptr_t Process::GetMainWindowHandle(int32_t pid)
    {
        return 0;
    }

#else
    typedef struct
    {
        DWORD pid;
        HWND hwnd;
    } EnumWindowsArgs;

    static BOOL STDCALL Il2CppEnumWindowsCallback(HWND hwnd, LPARAM lparam)
    {
        EnumWindowsArgs* args = (EnumWindowsArgs*)lparam;
        DWORD pid = 0;
        GetWindowThreadProcessId(hwnd, &pid);
        if (pid != args->pid || GetWindow(hwnd, GW_OWNER) != NULL || !IsWindowVisible(hwnd)) return TRUE;
        args->hwnd = hwnd;
        return FALSE;
    }

    intptr_t Process::GetMainWindowHandle(int32_t pid)
    {
        EnumWindowsArgs args = { (DWORD)pid, 0 };
        EnumWindows(Il2CppEnumWindowsCallback, (LPARAM)&args);
        return (intptr_t)args.hwnd;
    }

#endif
}
}

#endif // IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINDOWS_GAMES
