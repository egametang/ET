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
    enum
    {
        BFLAGS_IgnoreCase = 1,
        BFLAGS_DeclaredOnly = 2,
        BFLAGS_Instance = 4,
        BFLAGS_Static = 8,
        BFLAGS_Public = 0x10,
        BFLAGS_NonPublic = 0x20,
        BFLAGS_FlattenHierarchy = 0x40,
        BFLAGS_InvokeMethod = 0x100,
        BFLAGS_CreateInstance = 0x200,
        BFLAGS_GetField = 0x400,
        BFLAGS_SetField = 0x800,
        BFLAGS_GetProperty = 0x1000,
        BFLAGS_SetProperty = 0x2000,
        BFLAGS_ExactBinding = 0x10000,
        BFLAGS_SuppressChangeType = 0x20000,
        BFLAGS_OptionalParamBinding = 0x40000,
        BFLAGS_MatchAll = 0xFFFFF
    };

    enum
    {
        MLISTTYPE_All = 0,
        MLISTTYPE_CaseSensitive = 1,
        MLISTTYPE_CaseInsensitive = 2,
        MLISTTYPE_HandleToInfo = 3
    };

    class LIBIL2CPP_CODEGEN_API RuntimeType
    {
    public:
        static int32_t GetGenericParameterPosition(Il2CppReflectionRuntimeType* thisPtr);
        static intptr_t GetConstructors_native(Il2CppReflectionRuntimeType* thisPtr, int32_t bindingAttr);
        static intptr_t GetEvents_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t listType);
        static intptr_t GetFields_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType);
        static intptr_t GetMethodsByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t namePtr, int32_t bindingAttr, int32_t listType);
        static intptr_t GetNestedTypes_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType);
        static intptr_t GetPropertiesByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType);
        static Il2CppObject* CreateInstanceInternal(Il2CppReflectionType* type);
        static Il2CppObject* GetCorrespondingInflatedConstructor(Il2CppReflectionRuntimeType* thisPtr, Il2CppObject* generic);
        static Il2CppObject* get_DeclaringMethod(Il2CppReflectionRuntimeType* thisPtr);
        static Il2CppObject* GetCorrespondingInflatedMethod(Il2CppReflectionRuntimeType* thisPtr, Il2CppObject* generic);
        static Il2CppString* get_Name(Il2CppReflectionRuntimeType* thisPtr);
        static Il2CppString* get_Namespace(Il2CppReflectionRuntimeType* thisPtr);
        static Il2CppString* getFullName(Il2CppReflectionRuntimeType* thisPtr, bool full_name, bool assembly_qualified);
        static Il2CppReflectionType* get_DeclaringType(Il2CppReflectionRuntimeType* thisPtr);
        static Il2CppReflectionType* make_array_type(Il2CppReflectionRuntimeType* thisPtr, int32_t rank);
        static Il2CppReflectionType* make_byref_type(Il2CppReflectionRuntimeType* thisPtr);
        static Il2CppReflectionType* MakeGenericType(Il2CppReflectionType* type, Il2CppArray* types);
        static Il2CppReflectionType* MakePointerType(Il2CppReflectionType* type);
        static Il2CppArray* GetGenericArgumentsInternal(Il2CppReflectionRuntimeType* thisPtr, bool runtimeArray);
        static Il2CppArray* GetInterfaces(Il2CppReflectionRuntimeType* thisPtr);
        static int32_t GetTypeCodeImplInternal(Il2CppReflectionType* type);
        static void GetInterfaceMapData(Il2CppReflectionType* type, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods);
        static void GetPacking(Il2CppReflectionType* type, int32_t* packing, int32_t* size);
        static void GetGUID(Il2CppReflectionType* type, Il2CppArray* types);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
