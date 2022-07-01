#pragma once

#include "il2cpp-config.h"
struct Il2CppObject;
struct Il2CppReflectionType;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Enum
    {
    public:
        static Il2CppObject* ToObject(Il2CppReflectionType* enumType, Il2CppObject* value);
        static int compare_value_to(Il2CppObject* thisPtr, Il2CppObject* other);
        static int32_t get_hashcode(Il2CppObject* thisPtr);
        static Il2CppObject* get_value(Il2CppObject* thisPtr);
        static Il2CppReflectionType* get_underlying_type(Il2CppReflectionType* type);

        static bool GetEnumValuesAndNames(Il2CppReflectionRuntimeType* enumType, Il2CppArray** values, Il2CppArray** names);
        static bool InternalHasFlag(Il2CppObject* thisPtr, Il2CppObject* flags);
        static int32_t InternalCompareTo(Il2CppObject* o1, Il2CppObject* o2);
        static Il2CppObject* InternalBoxEnum(Il2CppReflectionRuntimeType* enumType, int64_t value);
        static Il2CppReflectionRuntimeType* InternalGetUnderlyingType(Il2CppReflectionRuntimeType* enumType);
#if IL2CPP_TINY
        static bool TinyEnumEquals(Il2CppObject* left, Il2CppObject* right);
#endif
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
