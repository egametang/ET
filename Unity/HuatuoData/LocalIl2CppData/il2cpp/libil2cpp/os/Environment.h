#pragma once
#include "il2cpp-config.h"
#include "utils/Expected.h"
#include <string>
#include <stdint.h>
#include <vector>

#undef SetEnvironmentVariable // Get rid of windows.h #define.
#undef GetEnvironmentVariable // Get rid of windows.h #define.

struct Il2CppArray;

namespace il2cpp
{
namespace os
{
    class Environment
    {
    public:
        static std::string GetMachineName();
        static int32_t GetProcessorCount();
        static std::string GetOsVersionString();
        static std::string GetOsUserName();
        static std::string GetEnvironmentVariable(const std::string& name);
        static void SetEnvironmentVariable(const std::string& name, const std::string& value);
        static std::vector<std::string> GetEnvironmentVariableNames();
        static std::string GetHomeDirectory();
        static std::vector<std::string> GetLogicalDrives();
        static void Exit(int result);
        static NORETURN void Abort();
        static utils::Expected<std::string> GetWindowsFolderPath(int32_t folder);

        static utils::Expected<bool> Is64BitOs();
    };
}
}
