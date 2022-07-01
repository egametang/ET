#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !(IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_LUMIN) && !RUNTIME_TINY

#include <sys/types.h>
#include <unistd.h>

#if IL2CPP_TARGET_LINUX
#include <fcntl.h>
#include <sys/stat.h>
#endif

#include "os/Process.h"

#include "il2cpp-vm-support.h"

struct ProcessHandle
{
    pid_t pid;
};

namespace il2cpp
{
namespace os
{
    int Process::GetCurrentProcessId()
    {
        return getpid();
    }

    ProcessHandle* Process::GetProcess(int processId)
    {
        // If/when we implement the CreateProcess_internal icall we will likely
        // need to so something smarter here to find the process if we did
        // not create it and return a known pseudo-handle. For now this
        // is sufficient though.
        return (ProcessHandle*)(intptr_t)processId;
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        // We have nothing to do here.
    }

    std::string Process::GetProcessName(ProcessHandle* handle)
    {
#if IL2CPP_TARGET_LINUX
        char pathBuffer[32];
        snprintf(pathBuffer, IL2CPP_ARRAY_SIZE(pathBuffer), "/proc/%d/comm", static_cast<int>(reinterpret_cast<intptr_t>(handle)));

        int fileHandle = open(pathBuffer, 0, O_RDONLY);
        if (fileHandle == -1)
            return std::string();

        std::string processName;
        char buffer[256];
        ssize_t bytesRead;

        do
        {
            bytesRead = read(fileHandle, buffer, IL2CPP_ARRAY_SIZE(buffer));
            if (bytesRead > 0)
                processName.append(buffer, bytesRead);
        }
        while (bytesRead == IL2CPP_ARRAY_SIZE(buffer));

        close(fileHandle);

        // Truncate name to first line ending
        size_t index = processName.find_first_of('\n');
        if (index != std::string::npos)
            processName.resize(index);

        return processName;
#else
        IL2CPP_VM_NOT_SUPPORTED(Process::GetProcessName, "GetProcessName is not supported for non-Windows/OSX/Linux desktop platforms");
        return std::string();
#endif
    }
}
}

#endif
