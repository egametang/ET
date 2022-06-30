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
    class LIBIL2CPP_CODEGEN_API RuntimeTypeHandle
    {
    public:
        static bool HasInstantiation(Il2CppReflectionRuntimeType* type);
        static bool HasReferences(Il2CppReflectionRuntimeType* type);
        static bool is_subclass_of(Il2CppType* childType, Il2CppType* baseType);
        static bool IsByRefLike(Il2CppReflectionRuntimeType* type);
        static bool IsComObject(Il2CppReflectionRuntimeType* type);
        static bool IsGenericTypeDefinition(Il2CppReflectionRuntimeType* type);
        static bool IsGenericVariable(Il2CppReflectionRuntimeType* type);
        static bool IsInstanceOfType(Il2CppReflectionRuntimeType* type, Il2CppObject* o);
        static bool type_is_assignable_from(Il2CppReflectionType* a, Il2CppReflectionType* b);
        static int32_t GetArrayRank(Il2CppReflectionRuntimeType* type);
        static int32_t GetMetadataToken(Il2CppReflectionRuntimeType* type);
        static intptr_t GetGenericParameterInfo(Il2CppReflectionRuntimeType* type);
        static uint8_t GetCorElementType(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionAssembly* GetAssembly(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionModule* GetModule(Il2CppReflectionRuntimeType* type);
        static int32_t GetAttributes(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionRuntimeType* GetBaseType(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionRuntimeType* GetElementType(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionType* internal_from_name(Il2CppString* name, int32_t* stackMark, Il2CppObject* callerAssembly, bool throwOnError, bool ignoreCase, bool reflectionOnly);
        static Il2CppReflectionType* GetGenericTypeDefinition_impl(Il2CppReflectionRuntimeType* type);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
