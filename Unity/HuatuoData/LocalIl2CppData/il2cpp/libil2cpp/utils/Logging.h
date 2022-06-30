#pragma once

#include "il2cpp-config.h"

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API Logging
    {
    public:
        static void Write(const char* format, ...);
        static void SetLogCallback(Il2CppLogCallback method);
        static bool IsLogCallbackSet();

    private:
        static Il2CppLogCallback s_Callback;
    };
}
}
