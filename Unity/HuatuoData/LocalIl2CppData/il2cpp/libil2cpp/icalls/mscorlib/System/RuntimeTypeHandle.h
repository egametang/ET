#pragma once

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
        static bool IsArray(Il2CppReflectionRuntimeType* type);
        static bool IsByRef(Il2CppReflectionRuntimeType* type);
        static bool IsComObject(Il2CppReflectionRuntimeType* type);
        static bool IsGenericTypeDefinition(Il2CppReflectionRuntimeType* type);
        static bool IsGenericVariable(Il2CppReflectionRuntimeType* type);
        static bool IsInstanceOfType(Il2CppReflectionRuntimeType* type, Il2CppObject* o);
        static bool IsPointer(Il2CppReflectionRuntimeType* type);
        static bool IsPrimitive(Il2CppReflectionRuntimeType* type);
        static bool type_is_assignable_from(Il2CppReflectionType* a, Il2CppReflectionType* b);
        static int32_t GetArrayRank(Il2CppReflectionRuntimeType* type);
        static int32_t GetMetadataToken(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionAssembly* GetAssembly(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionModule* GetModule(Il2CppReflectionRuntimeType* type);
        static int32_t GetAttributes(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionRuntimeType* GetBaseType(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionRuntimeType* GetElementType(Il2CppReflectionRuntimeType* type);
        static Il2CppReflectionType* GetGenericTypeDefinition_impl(Il2CppReflectionRuntimeType* type);
        static intptr_t GetGenericParameterInfo(Il2CppReflectionRuntimeType* type);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
