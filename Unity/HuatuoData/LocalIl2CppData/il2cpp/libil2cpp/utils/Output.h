#pragma once

#include "il2cpp-config.h"

struct Il2CppString;

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API Output
    {
    public:
        static void WriteToStdout(const char* message);
        static void WriteToStderr(const char* message);
    };
}
}
