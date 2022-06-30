#pragma once

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Random
    {
    public:
        static bool Open();
        static void* Create();
        static void Free(void* handle);

        static bool TryGetBytes(void** handle, unsigned char *buffer, int buffer_size);
        static bool TryGetUnsignedInt32(void** handle, uint32_t *val, uint32_t min, uint32_t max);
        static uint32_t Next(void** handle, uint32_t min, uint32_t max);
    };
} /* namespace vm */
} /* namespace il2cpp */
