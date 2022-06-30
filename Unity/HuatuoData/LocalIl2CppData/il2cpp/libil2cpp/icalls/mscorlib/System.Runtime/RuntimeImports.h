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
namespace Runtime
{
    class LIBIL2CPP_CODEGEN_API RuntimeImports
    {
    public:
        static void ecvt_s(char* buffer, int32_t sizeInBytes, double value, int32_t count, int32_t* dec, int32_t* sign);
        static void Memmove(uint8_t* dest, uint8_t* src, uint32_t len);
        static void Memmove_wbarrier(uint8_t* dest, uint8_t* src, uint32_t len, intptr_t type_handle);
        static void ZeroMemory(void* p, uint32_t byteLength);
    };
} // namespace Runtime
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
