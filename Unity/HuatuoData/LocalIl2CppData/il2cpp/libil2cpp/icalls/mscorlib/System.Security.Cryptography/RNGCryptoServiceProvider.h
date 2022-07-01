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
        static void RngClose(intptr_t handle);
        static intptr_t RngGetBytes(intptr_t, Il2CppArray *);
        static intptr_t RngInitialize(Il2CppArray *);
        static bool RngOpen();
    };
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
