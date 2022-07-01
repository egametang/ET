#include "il2cpp-config.h"
#include "NativeMethods.h"
#include "os/NativeMethods.h"
#include "os/Process.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace Microsoft
{
namespace Win32
{
    bool NativeMethods::CloseProcess(intptr_t handle)
    {
        return os::NativeMethods::CloseProcess((il2cpp::os::ProcessHandle*)handle);
    }

    bool NativeMethods::GetExitCodeProcess(intptr_t processHandle, int32_t* exitCode)
    {
        return os::NativeMethods::GetExitCodeProcess((il2cpp::os::ProcessHandle*)processHandle, exitCode);
    }

    bool NativeMethods::GetProcessTimes(intptr_t handle, int64_t* creation, int64_t* exit, int64_t* kernel, int64_t* user)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::GetProcessTimes);
        IL2CPP_UNREACHABLE;
        return false;
    }

    bool NativeMethods::GetProcessWorkingSetSize(intptr_t handle, intptr_t* min, intptr_t* max)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::GetProcessWorkingSetSize);
        IL2CPP_UNREACHABLE;
        return false;
    }

    bool NativeMethods::SetPriorityClass(intptr_t handle, int32_t priorityClass)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::SetPriorityClass);
        IL2CPP_UNREACHABLE;
        return false;
    }

    bool NativeMethods::SetProcessWorkingSetSize(intptr_t handle, intptr_t min, intptr_t max)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::SetProcessWorkingSetSize);
        IL2CPP_UNREACHABLE;
        return false;
    }

    bool NativeMethods::TerminateProcess(intptr_t processHandle, int32_t exitCode)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::TerminateProcess);
        IL2CPP_UNREACHABLE;
        return false;
    }

    int32_t NativeMethods::GetCurrentProcessId()
    {
        return os::NativeMethods::GetCurrentProcessId();
    }

    int32_t NativeMethods::GetPriorityClass(intptr_t handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::GetPriorityClass);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    int32_t NativeMethods::WaitForInputIdle(intptr_t handle, int32_t milliseconds)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(NativeMethods::WaitForInputIdle);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    intptr_t NativeMethods::GetCurrentProcess()
    {
        return reinterpret_cast<intptr_t>(os::NativeMethods::GetCurrentProcess());
    }
} // namespace Win32
} // namespace Microsoft
} // namespace System
} // namespace icalls
} // namespace il2cpp
