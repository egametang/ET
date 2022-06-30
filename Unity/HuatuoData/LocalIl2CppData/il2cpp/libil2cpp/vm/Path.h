#pragma once

#include "il2cpp-config.h"
#include <string>

#undef GetTempPath // Get rid of windows.h #define.

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Path
    {
    public:
        static void SetTempPath(const char* path);
        static std::string GetTempPath();
    };
} /* namespace vm */
} /* namespace il2cpp */
