#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppArray;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Buffer
    {
    public:
        static bool InternalBlockCopy(Il2CppArray* src, int32_t srcOffsetBytes, Il2CppArray* dst, int32_t dstOffsetBytes, int32_t byteCount);
        static int32_t _ByteLength(Il2CppArray* array);
        static void InternalMemcpy(uint8_t* dest, uint8_t* src, int32_t count);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
