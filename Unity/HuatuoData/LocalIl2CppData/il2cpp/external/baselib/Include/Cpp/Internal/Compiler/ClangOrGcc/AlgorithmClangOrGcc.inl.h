#pragma once

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace Algorithm
        {
            inline int HighestBitNonZero(uint32_t value)
            {
                return 31 - __builtin_clz(value);
            }

            inline int HighestBitNonZero(uint64_t value)
            {
#if PLATFORM_ARCH_64
                return 63 - __builtin_clzll(value);
#else
                return (value & 0xffffffff00000000ULL) ? (63 - __builtin_clz((uint32_t)(value >> 32))) : (31 - __builtin_clz((uint32_t)value));
#endif
            }

            inline int HighestBit(uint32_t value)
            {
                return value == 0 ? -1 : HighestBitNonZero(value);
            }

            inline int HighestBit(uint64_t value)
            {
                return value == 0 ? -1 : HighestBitNonZero(value);
            }

            inline int LowestBitNonZero(uint32_t value)
            {
                return __builtin_ctz(value);
            }

            inline int LowestBitNonZero(uint64_t value)
            {
#if  PLATFORM_ARCH_64
                return __builtin_ctzll(value);
#else
                return (value & 0x00000000ffffffffULL) ? __builtin_ctz((uint32_t)(value)) : (32 + __builtin_ctz((uint32_t)(value >> 32)));
#endif
            }

            inline int LowestBit(uint32_t value)
            {
                return value == 0 ? -1 : LowestBitNonZero(value);
            }

            inline int LowestBit(uint64_t value)
            {
                return value == 0 ? -1 : LowestBitNonZero(value);
            }

            inline int BitsInMask(uint64_t mask)   { return __builtin_popcountll(mask); }
            inline int BitsInMask(uint32_t mask)   { return __builtin_popcount(mask); }
            inline int BitsInMask(uint16_t mask)   { return BitsInMask((uint32_t)mask); }
            inline int BitsInMask(uint8_t mask)    { return BitsInMask((uint32_t)mask); }
        }
    }
}
