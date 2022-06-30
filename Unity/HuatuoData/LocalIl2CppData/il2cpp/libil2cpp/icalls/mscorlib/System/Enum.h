#pragma once

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
        static bool GetEnumValuesAndNames(Il2CppReflectionRuntimeType* enumType, Il2CppArray** values, Il2CppArray** names);
        static bool InternalHasFlag(Il2CppObject* thisPtr, Il2CppObject* flags);
        static int32_t get_hashcode(Il2CppObject* thisPtr);
        static int32_t InternalCompareTo(Il2CppObject* o1, Il2CppObject* o2);
        static Il2CppObject* get_value(Il2CppObject* thisPtr);
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
