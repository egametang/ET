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
        static bool ParseName(Il2CppReflectionAssemblyName* aname, Il2CppString* assemblyName);
        static void get_public_token(uint8_t* token, uint8_t* pubkey, int32_t len);
        static Il2CppMonoAssemblyName* GetNativeName(intptr_t assembly_ptr);
        static bool ParseAssemblyName(intptr_t name, Il2CppMonoAssemblyName* aname, bool* is_version_defined, bool* is_token_defined);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
