#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppString;

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
namespace Principal
{
    class LIBIL2CPP_CODEGEN_API WindowsIdentity
    {
    public:
        static intptr_t GetCurrentToken();
        static intptr_t GetUserToken(Il2CppString* username);
        static Il2CppString* GetTokenName(intptr_t token);
        static Il2CppArray* _GetRoles(intptr_t token);
    };
} /* namespace Principal */
} /* namespace Security */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
