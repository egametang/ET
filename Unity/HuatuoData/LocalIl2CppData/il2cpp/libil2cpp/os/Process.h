#pragma once

#include "utils/Expected.h"

#include <stdint.h>
#include <string>

namespace il2cpp
{
namespace os
{
    struct ProcessHandle;

    class Process
    {
    public:
        static int GetCurrentProcessId();
        static utils::Expected<ProcessHandle*> GetProcess(int processId);
        static void FreeProcess(ProcessHandle* handle);
        static utils::Expected<std::string> GetProcessName(ProcessHandle* handle);
        static int64_t Times(ProcessHandle* handle, int32_t type);
        static int64_t StartTime(ProcessHandle* handle);
        static int64_t GetProcessData(int32_t pid, int32_t data_type, int32_t* error);
        static intptr_t GetMainWindowHandle(int32_t pid);
    };
}
}
