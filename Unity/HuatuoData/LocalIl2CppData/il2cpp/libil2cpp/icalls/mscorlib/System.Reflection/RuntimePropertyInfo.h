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
    typedef enum
    {
        PInfo_Attributes = 1,
        PInfo_GetMethod  = 1 << 1,
        PInfo_SetMethod  = 1 << 2,
        PInfo_ReflectedType = 1 << 3,
        PInfo_DeclaringType = 1 << 4,
        PInfo_Name = 1 << 5
    } PInfo;

    class LIBIL2CPP_CODEGEN_API RuntimePropertyInfo
    {
    public:
        static int32_t get_metadata_token(Il2CppObject* monoProperty);
        static Il2CppObject* get_default_value(Il2CppObject* prop);
        static Il2CppReflectionProperty* internal_from_handle_type(intptr_t handlePtr, intptr_t typePtr);
        static Il2CppArray* GetTypeModifiers(Il2CppObject* prop, bool optional);
        static void get_property_info(Il2CppReflectionProperty *property, Il2CppPropertyInfo *info, PInfo req_info);
    };
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
