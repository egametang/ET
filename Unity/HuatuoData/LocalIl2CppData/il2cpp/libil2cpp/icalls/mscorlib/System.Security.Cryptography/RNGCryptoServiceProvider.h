#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Security
{
namespace Cryptography
{
    class LIBIL2CPP_CODEGEN_API RNGCryptoServiceProvider
    {
    public:
        static bool RngOpen();
        static intptr_t RngGetBytes(intptr_t handle, uint8_t* data, intptr_t data_length);
        static intptr_t RngInitialize(uint8_t* seed, intptr_t seed_length);
        static void RngClose(intptr_t handle);
    };
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
