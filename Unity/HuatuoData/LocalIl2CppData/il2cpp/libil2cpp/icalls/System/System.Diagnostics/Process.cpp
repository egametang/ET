#include "il2cpp-config.h"

#include "icalls/System/System.Diagnostics/Process.h"
#include "os/Process.h"
#include "vm/Exception.h"
#include "vm/String.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Diagnostics
{
    int32_t Process::ExitCode_internal(intptr_t handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::ExitCode_internal);

        return 0;
    }

    int64_t Process::ExitTime_internal(intptr_t handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::ExitTime_internal);

        return 0;
    }

    bool Process::GetWorkingSet_internal(intptr_t handle, int32_t* min, int32_t* max)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetWorkingSet_internal);

        return false;
    }

    bool Process::SetWorkingSet_internal(intptr_t handle, int32_t min, int32_t max, bool use_min)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::SetWorkingSet_internal);

        return false;
    }

    Il2CppArray* Process::GetModules_internal(void* /* System.Diagnostics.Process */ self, intptr_t handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetModules_internal);

        return 0;
    }

    int64_t Process::GetProcessData(int32_t pid, int32_t data_type, int32_t* error)
    {
        return os::Process::GetProcessData(pid, data_type, error);
    }

    int32_t Process::GetPriorityClass(intptr_t handle, int32_t* error)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetPriorityClass);

        return 0;
    }

    bool Process::SetPriorityClass(intptr_t handle, int32_t priority, int32_t* error)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::SetPriorityClass);

        return false;
    }

    int64_t Process::Times(intptr_t handle, int32_t type)
    {
        os::ProcessHandle *pHandle = (os::ProcessHandle*)handle;
        return os::Process::Times(pHandle, type);
    }

    Il2CppString* Process::ProcessName_internal(intptr_t handle)
    {
        os::ProcessHandle *pHandle = (os::ProcessHandle*)handle;
        std::string name = os::Process::GetProcessName(pHandle);
        return il2cpp::vm::String::New(name.c_str());
    }

    int64_t Process::StartTime_internal(intptr_t handle)
    {
        os::ProcessHandle *pHandle = (os::ProcessHandle*)handle;
        return os::Process::StartTime(pHandle);
    }

    bool Process::Kill_internal(intptr_t handle, int32_t signo)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::Kill_internal);

        return false;
    }

    intptr_t Process::GetProcess_internal(int32_t pid)
    {
        return reinterpret_cast<intptr_t>(os::Process::GetProcess(pid));
    }

    int32_t Process::GetPid_internal()
    {
        return os::Process::GetCurrentProcessId();
    }

    Il2CppArray* Process::GetProcesses_internal()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetProcesses_internal);

        return 0;
    }

    bool Process::ShellExecuteEx_internal(void* /* System.Diagnostics.ProcessStartInfo */ startInfo, ProcInfo* proc_info)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::ShellExecuteEx_internal);

        return false;
    }

    bool Process::CreateProcess_internal(void* /* System.Diagnostics.ProcessStartInfo */ startInfo, intptr_t _stdin, intptr_t _stdout, intptr_t _stderr, ProcInfo* proc_info)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::CreateProcess_internal);

        return false;
    }

    bool Process::WaitForExit_internal(void* /* System.Diagnostics.Process */ self, intptr_t handle, int32_t ms)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::WaitForExit_internal);

        return false;
    }

    bool Process::WaitForInputIdle_internal(void* /* System.Diagnostics.Process */ self, intptr_t handle, int32_t ms)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::WaitForInputIdle_internal);

        return false;
    }

    void Process::Process_free_internal(void* /* System.Diagnostics.Process */ self, intptr_t handle)
    {
        os::Process::FreeProcess((os::ProcessHandle*)handle);
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
