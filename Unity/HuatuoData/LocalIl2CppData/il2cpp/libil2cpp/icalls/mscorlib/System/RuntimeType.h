#pragma once

struct Il2CppReflectionRuntimeType;
struct mscorlib_System_Reflection_MethodInfo;

#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API RuntimeType
    {
    public:
        static bool IsTypeExportedToWindowsRuntime(Il2CppObject* type);
        static bool IsWindowsRuntimeObjectType(Il2CppObject* type);
        static int32_t get_core_clr_security_level(Il2CppObject* _this);
        static int32_t GetGenericParameterPosition(Il2CppReflectionRuntimeType* _this);
        static Il2CppObject* CreateInstanceInternal(Il2CppReflectionType* type);
        static int32_t GetGenericParameterAttributes(Il2CppReflectionRuntimeType* _this);
        static Il2CppObject* get_DeclaringMethod(Il2CppObject* _this);
        static Il2CppArray* GetConstructors_internal(Il2CppReflectionRuntimeType* _this, int32_t bindingAttr, Il2CppReflectionType* reflected_type);
        static Il2CppArray* GetEvents_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, Il2CppReflectionType* reflected_type);
        static Il2CppArray* GetFields_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, Il2CppReflectionType* reflected_type);
        static Il2CppArray* GetMethodsByName(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, bool ignoreCase, Il2CppReflectionType* reflected_type);
        static Il2CppArray* GetPropertiesByName(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, bool icase, Il2CppReflectionType* reflected_type);
        static Il2CppArray* GetNestedTypes_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingFlags);
        static Il2CppString* get_Name(Il2CppReflectionRuntimeType* _this);
        static Il2CppString* get_Namespace(Il2CppReflectionRuntimeType* _this);
        static Il2CppString* getFullName(Il2CppReflectionRuntimeType* _this, bool full_name, bool assembly_qualified);
        static Il2CppReflectionType* get_DeclaringType(Il2CppReflectionRuntimeType* _this);
        static Il2CppReflectionType* make_array_type(Il2CppReflectionRuntimeType* _this, int32_t rank);
        static Il2CppReflectionType* make_byref_type(Il2CppReflectionRuntimeType* _this);
        static Il2CppReflectionType* MakeGenericType(Il2CppReflectionType* gt, Il2CppArray* types);
        static Il2CppReflectionType* MakePointerType(Il2CppReflectionType* type);
        static Il2CppArray* GetGenericArgumentsInternal(Il2CppReflectionRuntimeType* _this, bool runtimeArray);
        static Il2CppArray* GetGenericParameterConstraints_impl(Il2CppReflectionRuntimeType* _this);
        static Il2CppArray* GetInterfaces(Il2CppReflectionRuntimeType* _this);
        static int32_t GetTypeCodeImplInternal(Il2CppReflectionType* type);
        static void GetInterfaceMapData(Il2CppReflectionType* t, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods);
        static void GetPacking(Il2CppReflectionRuntimeType* _this, int32_t* packing, int32_t* size);

        static intptr_t GetConstructors_native(Il2CppReflectionRuntimeType* thisPtr, int32_t bindingAttr);
        static intptr_t GetEvents_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr);
        static intptr_t GetFields_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr);
        static intptr_t GetMethodsByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t namePtr, int32_t bindingAttr, bool ignoreCase);
        static intptr_t GetNestedTypes_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr);
        static intptr_t GetPropertiesByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, bool icase);

        static void* /* System.Reflection.ConstructorInfo */ GetCorrespondingInflatedConstructor(void* /* System.MonoType */ self, void* /* System.Reflection.ConstructorInfo */ genericInfo);
        static mscorlib_System_Reflection_MethodInfo* GetCorrespondingInflatedMethod(Il2CppReflectionMonoType*, Il2CppReflectionMonoType*);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
