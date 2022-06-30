#include "il2cpp-config.h"

#if IL2CPP_TARGET_DARWIN

#include <sys/types.h>
#if !IL2CPP_TARGET_IOS
#include <libproc.h>
#endif
#include <unistd.h>
#include <stdlib.h>

#include "os/Process.h"
#include "utils/Il2CppError.h"

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

    utils::Expected<ProcessHandle*> Process::GetProcess(int processId)
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

    utils::Expected<std::string> Process::GetProcessName(ProcessHandle* handle)
    {
#if !IL2CPP_TARGET_IOS
        const size_t bufferLength = 256;
        char buf[bufferLength];
        int length = proc_name((int)((intptr_t)handle), buf, bufferLength);

        if (length <= 0)
            return std::string();

        return std::string(buf, length);
#else
        return utils::Il2CppError(utils::NotSupported, "GetProcessName is not supported for non-Windows/OSX/Linux desktop platforms");
#endif
    }

    intptr_t Process::GetMainWindowHandle(int32_t pid)
    {
        return 0;
    }
}
}

#endif
