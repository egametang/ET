#include "il2cpp-config.h"
#include "il2cpp-vm-support.h"

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

    ProcessHandle* Process::GetProcess(int processId)
    {
        return (ProcessHandle*)OpenProcess(PROCESS_ALL_ACCESS, TRUE, processId);
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        ::CloseHandle((HANDLE)handle);
    }

    std::string Process::GetProcessName(ProcessHandle* handle)
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
}
}

#endif // IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINDOWS_GAMES
