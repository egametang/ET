#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppArray;
struct Il2CppString;
struct mscorlib_System_Reflection_Module;

typedef int32_t PortableExecutableKinds;
typedef int32_t ImageFileMachine;
typedef int32_t ResolveTokenError;

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
    class LIBIL2CPP_CODEGEN_API Module
    {
    public:
        static Il2CppReflectionType* GetGlobalType(Il2CppReflectionModule* self);
        static Il2CppString* GetGuidInternal(mscorlib_System_Reflection_Module * thisPtr);
        static int32_t GetMDStreamVersion(intptr_t module_handle);
        static void GetPEKind(intptr_t module, PortableExecutableKinds* peKind, ImageFileMachine* machine);
        static Il2CppArray* InternalGetTypes(Il2CppReflectionModule * self);
        static intptr_t ResolveFieldToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, ResolveTokenError* error);
        static void* /* System.Reflection.MemberInfo */ ResolveMemberToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, ResolveTokenError* error);
        static intptr_t ResolveMethodToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, ResolveTokenError* error);
        static Il2CppArray* ResolveSignature(intptr_t module, int32_t metadataToken, ResolveTokenError* error);
        static Il2CppString* ResolveStringToken(intptr_t module, int32_t token, ResolveTokenError* error);
        static intptr_t ResolveTypeToken(intptr_t module, int32_t token, Il2CppArray* type_args, Il2CppArray* method_args, ResolveTokenError* error);
        static int32_t get_MetadataToken(Il2CppReflectionModule* self);
        static intptr_t GetHINSTANCE(mscorlib_System_Reflection_Module * thisPtr);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
