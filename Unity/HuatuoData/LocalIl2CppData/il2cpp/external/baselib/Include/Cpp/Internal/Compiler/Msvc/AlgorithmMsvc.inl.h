#pragma once

#include <intrin.h>

#pragma intrinsic(_BitScanReverse)
#if PLATFORM_ARCH_64
    #pragma intrinsic(_BitScanReverse64)
#endif

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace Algorithm
        {
            inline int HighestBit(uint32_t value)
            {
                unsigned long res;
                return _BitScanReverse(&res, value) ? (int)res : -1;
            }

            inline int HighestBit(uint64_t value)
            {
#if PLATFORM_ARCH_64
                unsigned long res;
                return _BitScanReverse64(&res, value) ? (int)res : -1;
#else
                unsigned long lower, upper;
                int lower_int = _BitScanReverse(&lower, (uint32_t)value) ? (int)lower : -1;
                return _BitScanReverse(&upper, (uint32_t)(value >> 32)) ? (int)(32 + upper) : lower_int;
#endif
            }

            inline int HighestBitNonZero(uint32_t value)
            {
                unsigned long res = 0;
                _BitScanReverse(&res, value);
                return (int)res;
            }

            inline int HighestBitNonZero(uint64_t value)
            {
#if PLATFORM_ARCH_64
                unsigned long res = 0;
                _BitScanReverse64(&res, value);
                return (int)res;
#else
                unsigned long lower, upper;
                _BitScanReverse(&lower, (uint32_t)value);
                return _BitScanReverse(&upper, (uint32_t)(value >> 32)) ? (32 + upper) : lower;
#endif
            }

            inline int LowestBit(uint32_t value)
            {
                unsigned long res;
                return _BitScanForward(&res, value) ? (int)res : -1;
            }

            inline int LowestBit(uint64_t value)
            {
#if PLATFORM_ARCH_64
                unsigned long res;
                return _BitScanForward64(&res, value) ? (int)res : -1;
#else
                unsigned long lower, upper;
                int upper_int = _BitScanForward(&upper, (uint32_t)(value >> 32)) ? (int)upper : -33;
                return _BitScanForward(&lower, (uint32_t)(value)) ? (int)lower : (32 + upper_int);
#endif
            }

            inline int LowestBitNonZero(uint32_t value)
            {
                unsigned long res = 0;
                _BitScanForward(&res, value);
                return (int)res;
            }

            inline int LowestBitNonZero(uint64_t value)
            {
#if PLATFORM_ARCH_64
                unsigned long res = 0;
                _BitScanForward64(&res, value);
                return (int)res;
#else
                unsigned long lower, upper;
                _BitScanForward(&upper, (uint32_t)(value >> 32));
                return _BitScanForward(&lower, (uint32_t)(value)) ? (int)lower : (int)(32 + upper);
#endif
            }

#if defined(_AMD64_) || defined(_X86_)
#ifdef _AMD64_
            inline int BitsInMask(uint64_t value)   { return (int)__popcnt64(value); }
#else
            inline int BitsInMask(uint64_t value)   { return BitsInMask((uint32_t)value) + BitsInMask((uint32_t)(value >> 32)); }
#endif
            inline int BitsInMask(uint32_t value)   { return (int)__popcnt(value); }
            inline int BitsInMask(uint16_t value)   { return (int)__popcnt16(value); }
            inline int BitsInMask(uint8_t  value)   { return BitsInMask((uint16_t)value); }

            // Todo: Consider using VCNT instruction on arm (NEON)
#else
            inline int BitsInMask(uint64_t value)
            {
                // From http://www-graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
                value = value - ((value >> 1) & (uint64_t) ~(uint64_t)0 / 3);
                value = (value & (uint64_t) ~(uint64_t)0 / 15 * 3) + ((value >> 2) & (uint64_t) ~(uint64_t)0 / 15 * 3);
                value = (value + (value >> 4)) & (uint64_t) ~(uint64_t)0 / 255 * 15;
                return (uint64_t)(value * ((uint64_t) ~(uint64_t)0 / 255)) >> (sizeof(uint64_t) - 1) * 8;
            }

            inline int BitsInMask(uint32_t value)
            {
                // From http://www-graphics.stanford.edu/~seander/bithacks.html#CountBitsSetParallel
                value = value - ((value >> 1) & 0x55555555);
                value = (value & 0x33333333) + ((value >> 2) & 0x33333333);
                return (((value + (value >> 4)) & 0xF0F0F0F) * 0x1010101) >> 24;
            }

            inline int BitsInMask(uint16_t value)   { return BitsInMask((uint32_t)value); }
            inline int BitsInMask(uint8_t value)    { return BitsInMask((uint32_t)value); }
#endif
        }
    }
}
