#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppObject;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API GC
    {
    public:
        static int32_t GetCollectionCount(int32_t generation);
        static int32_t GetGeneration(Il2CppObject* obj);
        static int32_t GetMaxGeneration();
        static int64_t GetAllocatedBytesForCurrentThread();
        static int64_t GetTotalMemory(bool forceFullCollection);
        static Il2CppObject* get_ephemeron_tombstone();
        static void _ReRegisterForFinalize(Il2CppObject* o);
        static void _SuppressFinalize(Il2CppObject* o);
        static void InternalCollect(int32_t generation);
        static void RecordPressure(int64_t bytesAllocated);
        static void register_ephemeron_array(Il2CppArray* array);
        static void WaitForPendingFinalizers();
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
