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
        static bool BlockCopyInternal(Il2CppArray * src, int src_offset, Il2CppArray * dest, int dest_offset, int count);
        static int32_t ByteLengthInternal(Il2CppArray* arr);
        static uint8_t GetByteInternal(Il2CppArray* arr, int idx);
        static void SetByteInternal(Il2CppArray* arr, int idx, int value);
        static uint8_t _GetByte(Il2CppArray* array, int32_t index);
        static bool InternalBlockCopy(Il2CppArray* src, int32_t srcOffsetBytes, Il2CppArray* dst, int32_t dstOffsetBytes, int32_t byteCount);
        static int32_t _ByteLength(Il2CppArray* array);
        static void _SetByte(Il2CppArray* array, int32_t index, uint8_t value);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
