#include "MemoryRead.h"
#include "il2cpp-config.h"

namespace il2cpp
{
namespace utils
{
    uint32_t ReadCompressedUInt32(const char** p)
    {
        uint32_t val = 0;
        uint8_t read = Read8(p);

        if ((read & 0x80) == 0)
        {
            // 1 byte written
            val = read;
        }
        else if ((read & 0xC0) == 0x80)
        {
            // 2 bytes written
            val = (read & ~0x80) << 8;
            val |= Read8(p);
        }
        else if ((read & 0xE0) == 0xC0)
        {
            // 4 bytes written
            val = (read & ~0xC0) << 24;
            val |= ((uint32_t)Read8(p) << 16);
            val |= ((uint32_t)Read8(p) << 8);
            val |= Read8(p);
        }
        else if (read == 0xF0)
        {
            // 5 bytes written, we had a really large int32!
            val = Read32(p);
        }
        else if (read == 0xFE)
        {
            // Special encoding for Int32.MaxValue
            val = UINT32_MAX - 1;
        }
        else if (read == 0xFF)
        {
            // Yes we treat UInt32.MaxValue (and Int32.MinValue, see ReadCompressedInt32) specially
            val = UINT32_MAX;
        }
        else
        {
            IL2CPP_ASSERT(false && "Invalid compressed integer format");
        }

        return val;
    }

    int32_t ReadCompressedInt32(const char** p)
    {
        uint32_t encoded = ReadCompressedUInt32(p);

        // -UINT32_MAX can't be represted safely in an int32_t, so we treat it specially
        if (encoded == UINT32_MAX)
            return INT32_MIN;

        bool isNegative = encoded & 1;
        encoded >>= 1;
        if (isNegative)
            return -(int32_t)(encoded + 1);
        return encoded;
    }
} /* utils */
} /* il2cpp */
