#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
struct Il2CppArray;
struct Il2CppReflectionType;
struct Il2CppObject;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Array
    {
    public:
        static void ClearInternal(Il2CppArray* arr, int32_t idx, int32_t length);
        static Il2CppArray* Clone(Il2CppArray* arr);
        static Il2CppArray* CreateInstanceImpl(Il2CppReflectionType* elementType, Il2CppArray* lengths, Il2CppArray* bounds);
        static bool FastCopy(Il2CppArray* source, int32_t source_idx, Il2CppArray* dest, int32_t dest_idx, int32_t length);
        static int32_t GetLength(Il2CppArray* thisPtr, int dimension);
        static int32_t GetLowerBound(Il2CppArray* , int32_t);
        static Il2CppObject* GetValue(Il2CppArray* thisPtr, Il2CppArray* indices);
        static Il2CppObject* GetValueImpl(Il2CppArray* thisPtr, int32_t pos);
        static void SetValue(Il2CppArray* , Il2CppObject* , Il2CppArray*);
        static void SetValueImpl(Il2CppArray* thisPtr, Il2CppObject* value, int index);
        static int GetRank(Il2CppArray* array);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
