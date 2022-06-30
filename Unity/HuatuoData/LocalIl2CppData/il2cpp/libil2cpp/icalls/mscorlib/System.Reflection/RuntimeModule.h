#pragma once

#include "il2cpp-object-internals.h"

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
    class LIBIL2CPP_CODEGEN_API RuntimeModule
    {
    public:
        static Il2CppArray* ResolveSignature(intptr_t module, int32_t metadataToken, int32_t* error);
        static int32_t get_MetadataToken(Il2CppReflectionModule* module);
        static int32_t GetMDStreamVersion(intptr_t module);
        static intptr_t GetHINSTANCE(intptr_t module);
        static intptr_t ResolveFieldToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error);
        static intptr_t ResolveMethodToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error);
        static intptr_t ResolveTypeToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error);
        static Il2CppObject* ResolveMemberToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, int32_t* error);
        static Il2CppString* ResolveStringToken(intptr_t module, int32_t token, int32_t* error);
        static Il2CppObject* GetGlobalType(intptr_t module);
        static Il2CppArray* InternalGetTypes(const Il2CppImage* module);
        static void GetGuidInternal(intptr_t module, Il2CppArray* guid);
        static void GetPEKind(intptr_t module, int32_t* peKind, int32_t* machine);
    };
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
