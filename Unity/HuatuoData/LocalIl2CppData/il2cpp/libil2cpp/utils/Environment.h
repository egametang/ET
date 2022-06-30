#pragma once

#include <stdint.h>
#include <vector>
#include "il2cpp-config.h"
#include "il2cpp-string-types.h"
struct Il2CppDomain;

namespace il2cpp
{
namespace utils
{
    class LIBIL2CPP_CODEGEN_API Environment
    {
    public:
        static const std::vector<UTF16String>& GetMainArgs();
        static int GetNumMainArgs();
        static void SetMainArgs(const char* const* args, int num_args);
        static void SetMainArgs(const Il2CppChar* const* args, int num_args);
    };
} /* namespace vm */
} /* namespace il2cpp */
