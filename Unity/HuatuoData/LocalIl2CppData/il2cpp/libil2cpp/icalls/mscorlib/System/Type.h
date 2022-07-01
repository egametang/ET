#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct mscorlib_System_Reflection_Assembly;
struct mscorlib_System_Reflection_Module;
struct mscorlib_System_Reflection_MethodInfo;

typedef int32_t Il2CppGenericParameterAttributes;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Type
    {
    public:
        static bool EqualsInternal(Il2CppReflectionType* left, Il2CppReflectionType* right);
        static Il2CppGenericParameterAttributes GetGenericParameterAttributes(Il2CppReflectionType* type);
        static Il2CppArray* GetGenericParameterConstraints_impl(Il2CppReflectionType* type);
        static int32_t GetGenericParameterPosition(Il2CppReflectionType* type);
        static Il2CppReflectionType* GetGenericTypeDefinition_impl(Il2CppReflectionType*);
        static void GetInterfaceMapData(Il2CppReflectionType* t, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods);
        static void GetPacking(Il2CppReflectionType* type, int32_t* packing, int32_t* size);
        static int GetTypeCodeInternal(Il2CppReflectionType*);
        static bool IsArrayImpl(Il2CppReflectionType* t);
        static bool IsInstanceOfType(Il2CppReflectionType* type, Il2CppObject* obj);
        static Il2CppReflectionType* MakeGenericType(Il2CppReflectionType* , Il2CppArray*);
        static Il2CppReflectionType* MakePointerType(Il2CppReflectionType* thisPtr);
        static bool get_IsGenericType(Il2CppReflectionType*);
        static bool get_IsGenericTypeDefinition(Il2CppReflectionType* type);
        static Il2CppReflectionType* internal_from_handle(intptr_t ptr);
        static Il2CppReflectionType* internal_from_name(Il2CppString* name, bool throwOnError, bool ignoreCase);
        static Il2CppReflectionType* make_array_type(Il2CppReflectionType* type, int32_t rank);
        static bool type_is_assignable_from(Il2CppReflectionType* type, Il2CppReflectionType* c);
        static bool type_is_subtype_of(Il2CppReflectionType* type, Il2CppReflectionType* c, bool check_interfaces);
        static Il2CppReflectionType* make_byref_type(Il2CppReflectionType* type);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
