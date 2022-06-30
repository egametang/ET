#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppReflectionAssemblyName;
struct Il2CppMonoAssemblyName;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    class LIBIL2CPP_CODEGEN_API AssemblyName
    {
    public:
        static Il2CppMonoAssemblyName* GetNativeName(intptr_t assembly_ptr);
        static bool ParseAssemblyName(intptr_t name, Il2CppMonoAssemblyName* aname, bool* is_version_definited, bool* is_token_defined);
        static void get_public_token(uint8_t* token, uint8_t* pubkey, int32_t len);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
