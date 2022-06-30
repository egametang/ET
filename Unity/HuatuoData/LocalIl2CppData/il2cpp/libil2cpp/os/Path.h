#pragma once
#include <string>
#include <stdint.h>

#undef GetTempPath

namespace il2cpp
{
namespace os
{
    class Path
    {
    public:
        static std::string GetExecutablePath();
        static std::string GetTempPath();
        static bool IsAbsolute(const std::string& path);
    };
}
}
