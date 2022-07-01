#pragma once
#include <string>
#include <stdint.h>

#include "il2cpp-config.h"
#undef GetTempPath

namespace il2cpp
{
namespace os
{
    class Path
    {
    public:
#if __ENABLE_UNITY_PLUGIN__
        static std::string GetApplicationPath();
        static std::string GetFrameworksPath();
#endif // __ENABLE_UNITY_PLUGIN__
        static std::string GetExecutablePath();
        static std::string GetTempPath();
        static bool IsAbsolute(const std::string& path);
    };
}
}
