#pragma once

#include "il2cpp-config.h"
#include <cmath>
#include <limits>
#include <stdint.h>

namespace il2cpp
{
namespace utils
{
namespace MathUtils
{
    // Do math on low/high part separately as 64-bit integers because otherwise
    // we might easily overflow during initial multiplication
    inline int64_t A_Times_B_DividedBy_C(int64_t multiplicand, int64_t multiplier, int64_t divisor)
    {
        IL2CPP_ASSERT((llabs(divisor) & (1LL << 62)) == 0 && "Can't divide by numbers with absolute value larger than 2^62 - 1.");
        bool resultIsNegative = static_cast<uint64_t>(multiplicand ^ multiplier ^ divisor) >> 63;   // Result is negative if odd number of operands are negative

        multiplicand = llabs(multiplicand);
        IL2CPP_ASSERT(multiplicand > 0 && "Can't multiply by -2^63.");

        multiplier = llabs(multiplier);
        IL2CPP_ASSERT(multiplier > 0 && "Can't multiply by -2^63.");

        divisor = llabs(divisor);   // We already asserted on divisor size

        uint64_t multiplicand_low = multiplicand & 0xFFFFFFFF;
        uint64_t multiplicand_high = multiplicand >> 32;

        uint64_t multiplier_low = multiplier & 0xFFFFFFFF;
        uint64_t multiplier_high = multiplier >> 32;

        // We're gonna assume our multiplicated value is 128-bit integer
        // so we're gonna compose it of two uint64_t's
        // a * b =
        // (a_high * 2^32 + a_low) * (b_high * 2^32 + b_low) =
        // a_high * b_high * 2^64 + (a_high * b_low + a_low * b_high) * 2^32 + a_low * b_low
        uint64_t dividends[2] =
        {
            multiplicand_low * multiplier_low,   // low part, bits [0, 63]
            multiplicand_high * multiplier_high  // high part, bits [64, 127]
        };

        uint64_t resultMid1 = multiplicand_high * multiplier_low + multiplicand_low * multiplier_high;  // mid part, bits [32, 95]

        dividends[1] += resultMid1 >> 32; // add the higher bits of mid part ([64, 95]) to high part
        resultMid1 = (resultMid1 & 0xFFFFFFFF) << 32; // Now this contains the lower bits of mid part ([32, 63])

        // Check for lower part overflow below adding the lower bits of mid part to it
        // Add carry to high part if overflow occurs
        if (dividends[0] > std::numeric_limits<uint64_t>::max() - resultMid1)
            dividends[1]++;

        dividends[0] += resultMid1; // add the lower bits of mid part to low part

        // At this point, we got our whole divident 128-bit value inside 'dividends'

        uint64_t workValue = 0; // Value that we're gonna be dividing
        uint64_t result = 0;    // The final result
        const uint64_t kOne = 1;
        int bitIndex = 127;     // Current bit that we're gonna be add to the workValue

        // Let's find the starting point for our division
        // We'll keep adding bits from our divident to the workValue until it's higher than the divisor
        // We did divisor = llabs(divisor) earlier, so cast to unsigned is safe
        while (workValue < static_cast<uint64_t>(divisor))
        {
            workValue <<= 1;

            if (bitIndex > -1)
            {
                workValue |= (dividends[bitIndex / 64] & (kOne << (bitIndex % 64))) != 0;
            }
            else
            {
                return 0;
            }

            bitIndex--;
        }

        // Main division loop
        for (; bitIndex > -2 || workValue >= static_cast<uint64_t>(divisor); bitIndex--)
        {
            result <<= 1;   // Shift result left

            // Since it's binary, the division result can be only 0 and 1
            // It's 1 if workValue is higher or equal to divisor
            if (workValue >= static_cast<uint64_t>(divisor))
            {
                workValue -= static_cast<uint64_t>(divisor);
                result++;
            }

            // Shift work value to the left and append the next bit of our dividend
            IL2CPP_ASSERT((workValue & (1LL << 63)) == 0 && "overflow!");

            if (bitIndex > -1)
            {
                workValue <<= 1;
                workValue |= (dividends[bitIndex / 64] & (kOne << (bitIndex % 64))) != 0;
            }
        }

        // Negate result if it's supposed to be negative
        if (resultIsNegative)
            return -static_cast<int64_t>(result);

        return result;
    }
}
}
}
