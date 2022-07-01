#pragma once

#include <stdint.h>
#include "os/Process.h"

#include <vector>
#include <string>

namespace il2cpp
{
namespace os
{
    class NativeMethods
    {
    public:
        static bool CloseProcess(ProcessHandle* handle);
        static bool GetExitCodeProcess(ProcessHandle* handle, int32_t* exitCode);
        static int32_t GetCurrentProcessId();
        static ProcessHandle* GetCurrentProcess();
    };
}
}
