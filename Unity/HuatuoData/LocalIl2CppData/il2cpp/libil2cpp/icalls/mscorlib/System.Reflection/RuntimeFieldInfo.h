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
    class LIBIL2CPP_CODEGEN_API RuntimeFieldInfo
    {
    public:
        static int32_t get_metadata_token(Il2CppObject* monoField);
        static int32_t GetFieldOffset(Il2CppReflectionField* field);
        static Il2CppObject* GetRawConstantValue(Il2CppReflectionField* field);
        static Il2CppObject* GetValueInternal(Il2CppReflectionField* field, Il2CppObject* obj);
        static Il2CppObject* UnsafeGetValue(Il2CppReflectionField* field, Il2CppObject* obj);
        static Il2CppReflectionType* GetParentType(Il2CppReflectionField* field, bool declaring);
        static Il2CppObject* ResolveType(Il2CppObject* thisPtr);
        static Il2CppArray* GetTypeModifiers(Il2CppObject* thisPtr, bool optional);
        static void SetValueInternal(Il2CppReflectionField* fi, Il2CppObject* obj, Il2CppObject* value);
    };
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
