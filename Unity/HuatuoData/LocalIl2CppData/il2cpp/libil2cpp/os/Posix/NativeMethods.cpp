#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/NativeMethods.h"
#include "os/Process.h"
#include "utils/Expected.h"

namespace il2cpp
{
namespace os
{
    bool NativeMethods::CloseProcess(ProcessHandle* handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::CloseProcess);
        IL2CPP_UNREACHABLE;
        return false;
    }

    utils::Expected<bool> NativeMethods::GetExitCodeProcess(ProcessHandle* handle, int32_t* exitCode)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::GetExitCodeProcess);
        IL2CPP_UNREACHABLE;
        return false;
    }

    int32_t NativeMethods::GetCurrentProcessId()
    {
        return Process::GetCurrentProcessId();
    }

    utils::Expected<ProcessHandle*> NativeMethods::GetCurrentProcess()
    {
        return Process::GetProcess(Process::GetCurrentProcessId());
    }
}
}
#endif
