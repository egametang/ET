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
    bool Process::CreateProcess_internal(Il2CppObject* startInfo, intptr_t _stdin, intptr_t _stdout, intptr_t _stderr, ProcInfo* procInfo)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::CreateProcess_internal);

        return false;
    }

    bool Process::ShellExecuteEx_internal(Il2CppObject* startInfo, ProcInfo* procInfo)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::ShellExecuteEx_internal);

        return false;
    }

    Il2CppArray* Process::GetModules_icall(Il2CppObject* thisPtr, intptr_t handle)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetModules_icall);

        return 0;
    }

    Il2CppArray* Process::GetProcesses_internal()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Process::GetProcesses_internal);

        return 0;
    }

    int64_t Process::GetProcessData(int32_t pid, int32_t data_type, int32_t* error)
    {
        return os::Process::GetProcessData(pid, data_type, error);
    }

    intptr_t Process::GetProcess_internal(int32_t pid)
    {
        auto process = os::Process::GetProcess(pid);
        vm::Exception::RaiseIfError(process.GetError());
        return reinterpret_cast<intptr_t>(process.Get());
    }

    Il2CppString* Process::ProcessName_icall(intptr_t handle)
    {
        os::ProcessHandle *pHandle = (os::ProcessHandle*)handle;
        auto name = os::Process::GetProcessName(pHandle);
        vm::Exception::RaiseIfError(name.GetError());
        return il2cpp::vm::String::New(name.Get().c_str());
    }

    intptr_t Process::MainWindowHandle_icall(int32_t pid)
    {
        return os::Process::GetMainWindowHandle(pid);
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
