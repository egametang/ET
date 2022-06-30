#pragma once

struct Il2CppMonoAssemblyName;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    class LIBIL2CPP_CODEGEN_API RuntimeMarshal
    {
    public:
        static void FreeAssemblyName(Il2CppMonoAssemblyName* name, bool freeStruct);
    };
} // namespace Mono
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
