#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppImage;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Module
    {
    public:
        // exported
        static uint32_t GetToken(const Il2CppImage *image);
    };
} /* namespace vm */
} /* namespace il2cpp */
