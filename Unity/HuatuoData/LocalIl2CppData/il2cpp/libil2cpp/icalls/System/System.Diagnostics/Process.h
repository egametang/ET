#pragma once

#include "il2cpp-object-internals.h"

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
    struct ProcInfo;

    class LIBIL2CPP_CODEGEN_API Process
    {
    public:
        static bool CreateProcess_internal(Il2CppObject* startInfo, intptr_t _stdin, intptr_t _stdout, intptr_t _stderr, ProcInfo* procInfo);
        static bool ShellExecuteEx_internal(Il2CppObject* startInfo, ProcInfo* procInfo);
        static Il2CppArray* GetModules_icall(Il2CppObject* thisPtr, intptr_t handle);
        static Il2CppArray* GetProcesses_internal();
        static int64_t GetProcessData(int32_t pid, int32_t data_type, int32_t* error);
        static intptr_t GetProcess_internal(int32_t pid);
        static Il2CppString* ProcessName_icall(intptr_t handle);
        static intptr_t MainWindowHandle_icall(int32_t pid);
    };
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
