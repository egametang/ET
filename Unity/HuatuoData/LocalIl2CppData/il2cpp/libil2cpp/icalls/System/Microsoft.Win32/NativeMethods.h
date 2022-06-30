#pragma once

#include "il2cpp-object-internals.h"

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
    class LIBIL2CPP_CODEGEN_API NativeMethods
    {
    public:
        static bool CloseProcess(intptr_t handle);
        static bool GetExitCodeProcess(intptr_t processHandle, int32_t* exitCode);
        static bool GetProcessTimes(intptr_t handle, int64_t* creation, int64_t* exit, int64_t* kernel, int64_t* user);
        static bool GetProcessWorkingSetSize(intptr_t handle, intptr_t* min, intptr_t* max);
        static bool SetPriorityClass(intptr_t handle, int32_t priorityClass);
        static bool SetProcessWorkingSetSize(intptr_t handle, intptr_t min, intptr_t max);
        static bool TerminateProcess(intptr_t processHandle, int32_t exitCode);
        static int32_t GetCurrentProcessId();
        static int32_t GetPriorityClass(intptr_t handle);
        static int32_t WaitForInputIdle(intptr_t handle, int32_t milliseconds);
        static intptr_t GetCurrentProcess();
    };
} // namespace Win32
} // namespace Microsoft
} // namespace System
} // namespace icalls
} // namespace il2cpp
