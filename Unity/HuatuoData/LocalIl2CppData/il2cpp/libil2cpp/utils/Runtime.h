#pragma once

#include <string>

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API Runtime
    {
    public:
        static NORETURN void Abort();
        static void SetDataDir(const char *path);
        static std::string GetDataDir();
    };
} // utils
} // il2cpp
