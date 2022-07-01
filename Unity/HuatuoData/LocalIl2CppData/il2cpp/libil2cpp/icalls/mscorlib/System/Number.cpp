// The MIT License(MIT)
//
// Copyright(c) Unity Technologies, Microsoft Corporation
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files(the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and / or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions :
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT.IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
// SOFTWARE.

#include "il2cpp-config.h"
#include "Number.h"
#include "utils/StringUtils.h"

#include <algorithm>

#define NUMBER_MAXDIGITS 50

struct NUMBER
{
    int precision;
    int scale;
    int sign;
    Il2CppChar digits[NUMBER_MAXDIGITS + 1];
    Il2CppChar* allDigits;

    NUMBER() :
        precision(0),
        scale(0),
        sign(0),
        allDigits(NULL)
    {
    }
};

struct FPDOUBLE
{
#if IL2CPP_BYTE_ORDER == IL2CPP_BIG_ENDIAN
    unsigned int sign : 1;
    unsigned int exp : 11;
    unsigned int mantHi : 20;
    unsigned int mantLo;
#else
    unsigned int mantLo;
    unsigned int mantHi : 20;
    unsigned int exp : 11;
    unsigned int sign : 1;
#endif
};


//
// precomputed tables with powers of 10. These allows us to do at most
// two Mul64 during the conversion. This is important not only
// for speed, but also for precision because of Mul64 computes with 1 bit error.
//
static const uint64_t rgval64Power10[] =
{
    // powers of 10
    /*1*/ 0xa000000000000000ULL,
    /*2*/ 0xc800000000000000ULL,
    /*3*/ 0xfa00000000000000ULL,
    /*4*/ 0x9c40000000000000ULL,
    /*5*/ 0xc350000000000000ULL,
    /*6*/ 0xf424000000000000ULL,
    /*7*/ 0x9896800000000000ULL,
    /*8*/ 0xbebc200000000000ULL,
    /*9*/ 0xee6b280000000000ULL,
    /*10*/ 0x9502f90000000000ULL,
    /*11*/ 0xba43b74000000000ULL,
    /*12*/ 0xe8d4a51000000000ULL,
    /*13*/ 0x9184e72a00000000ULL,
    /*14*/ 0xb5e620f480000000ULL,
    /*15*/ 0xe35fa931a0000000ULL,

    // powers of 0.1
    /*1*/ 0xcccccccccccccccdULL,
    /*2*/ 0xa3d70a3d70a3d70bULL,
    /*3*/ 0x83126e978d4fdf3cULL,
    /*4*/ 0xd1b71758e219652eULL,
    /*5*/ 0xa7c5ac471b478425ULL,
    /*6*/ 0x8637bd05af6c69b7ULL,
    /*7*/ 0xd6bf94d5e57a42beULL,
    /*8*/ 0xabcc77118461ceffULL,
    /*9*/ 0x89705f4136b4a599ULL,
    /*10*/ 0xdbe6fecebdedd5c2ULL,
    /*11*/ 0xafebff0bcb24ab02ULL,
    /*12*/ 0x8cbccc096f5088cfULL,
    /*13*/ 0xe12e13424bb40e18ULL,
    /*14*/ 0xb424dc35095cd813ULL,
    /*15*/ 0x901d7cf73ab0acdcULL,
};

static const int8_t rgexp64Power10[] =
{
    // exponents for both powers of 10 and 0.1
    /*1*/ 4,
    /*2*/ 7,
    /*3*/ 10,
    /*4*/ 14,
    /*5*/ 17,
    /*6*/ 20,
    /*7*/ 24,
    /*8*/ 27,
    /*9*/ 30,
    /*10*/ 34,
    /*11*/ 37,
    /*12*/ 40,
    /*13*/ 44,
    /*14*/ 47,
    /*15*/ 50,
};

static const uint64_t rgval64Power10By16[] =
{
    // powers of 10^16
    /*1*/ 0x8e1bc9bf04000000ULL,
    /*2*/ 0x9dc5ada82b70b59eULL,
    /*3*/ 0xaf298d050e4395d6ULL,
    /*4*/ 0xc2781f49ffcfa6d4ULL,
    /*5*/ 0xd7e77a8f87daf7faULL,
    /*6*/ 0xefb3ab16c59b14a0ULL,
    /*7*/ 0x850fadc09923329cULL,
    /*8*/ 0x93ba47c980e98cdeULL,
    /*9*/ 0xa402b9c5a8d3a6e6ULL,
    /*10*/ 0xb616a12b7fe617a8ULL,
    /*11*/ 0xca28a291859bbf90ULL,
    /*12*/ 0xe070f78d39275566ULL,
    /*13*/ 0xf92e0c3537826140ULL,
    /*14*/ 0x8a5296ffe33cc92cULL,
    /*15*/ 0x9991a6f3d6bf1762ULL,
    /*16*/ 0xaa7eebfb9df9de8aULL,
    /*17*/ 0xbd49d14aa79dbc7eULL,
    /*18*/ 0xd226fc195c6a2f88ULL,
    /*19*/ 0xe950df20247c83f8ULL,
    /*20*/ 0x81842f29f2cce373ULL,
    /*21*/ 0x8fcac257558ee4e2ULL,

    // powers of 0.1^16
    /*1*/ 0xe69594bec44de160ULL,
    /*2*/ 0xcfb11ead453994c3ULL,
    /*3*/ 0xbb127c53b17ec165ULL,
    /*4*/ 0xa87fea27a539e9b3ULL,
    /*5*/ 0x97c560ba6b0919b5ULL,
    /*6*/ 0x88b402f7fd7553abULL,
    /*7*/ 0xf64335bcf065d3a0ULL,
    /*8*/ 0xddd0467c64bce4c4ULL,
    /*9*/ 0xc7caba6e7c5382edULL,
    /*10*/ 0xb3f4e093db73a0b7ULL,
    /*11*/ 0xa21727db38cb0053ULL,
    /*12*/ 0x91ff83775423cc29ULL,
    /*13*/ 0x8380dea93da4bc82ULL,
    /*14*/ 0xece53cec4a314f00ULL,
    /*15*/ 0xd5605fcdcf32e217ULL,
    /*16*/ 0xc0314325637a1978ULL,
    /*17*/ 0xad1c8eab5ee43ba2ULL,
    /*18*/ 0x9becce62836ac5b0ULL,
    /*19*/ 0x8c71dcd9ba0b495cULL,
    /*20*/ 0xfd00b89747823938ULL,
    /*21*/ 0xe3e27a444d8d991aULL,
};

static const int16_t rgexp64Power10By16[] =
{
    // exponents for both powers of 10^16 and 0.1^16
    /*1*/ 54,
    /*2*/ 107,
    /*3*/ 160,
    /*4*/ 213,
    /*5*/ 266,
    /*6*/ 319,
    /*7*/ 373,
    /*8*/ 426,
    /*9*/ 479,
    /*10*/ 532,
    /*11*/ 585,
    /*12*/ 638,
    /*13*/ 691,
    /*14*/ 745,
    /*15*/ 798,
    /*16*/ 851,
    /*17*/ 904,
    /*18*/ 957,
    /*19*/ 1010,
    /*20*/ 1064,
    /*21*/ 1117,
};

#define Mul32x32To64(a, b) ((uint64_t)((uint32_t)(a)) * (uint64_t)((uint32_t)(b)))


#ifdef _DEBUG
//
// slower high precision version of Mul64 for computation of the tables
//
static uint64_t Mul64Precise(uint64_t a, uint64_t b, int* pexp)
{
    uint64_t hilo =
        ((Mul32x32To64(a >> 32, b) >> 1) +
         (Mul32x32To64(a, b >> 32) >> 1) +
         (Mul32x32To64(a, b) >> 33)) >> 30;

    uint64_t val = Mul32x32To64(a >> 32, b >> 32) + (hilo >> 1) + (hilo & 1);

    // normalize
    if ((val & 0x8000000000000000L) == 0)
    {
        val <<= 1; *pexp -= 1;
    }

    return val;
}

//
// debug-only verification of the precomputed tables
//
static void CheckTable(uint64_t val, int exp, const void* table, int size, const char* name, int tabletype)
{
    uint64_t multval = val;
    int mulexp = exp;
    bool fBad = false;

    for (int i = 0; i < size; i++)
    {
        switch (tabletype)
        {
            case 1:
                if (((uint64_t*)table)[i] != val)
                {
                    if (!fBad)
                    {
                        fprintf(stderr, "%s:\n", name);
                        fBad = true;
                    }
                    fprintf(stderr, "/*%d*/ I64(0x%llx),\n", i + 1, val);
                }
                break;
            case 2:
                if (((int8_t*)table)[i] != exp)
                {
                    if (!fBad)
                    {
                        fprintf(stderr, "%s:\n", name);
                        fBad = true;
                    }
                    fprintf(stderr, "/*%d*/ %d,\n", i + 1, exp);
                }
                break;
            case 3:
                if (((int16_t*)table)[i] != exp)
                {
                    if (!fBad)
                    {
                        fprintf(stderr, "%s:\n", name);
                        fBad = true;
                    }
                    fprintf(stderr, "/*%d*/ %d,\n", i + 1, exp);
                }
                break;
            default:
                IL2CPP_ASSERT(false);
                break;
        }

        exp += mulexp;
        val = Mul64Precise(val, multval, &exp);
    }
    IL2CPP_ASSERT(!fBad || !"NumberToDouble table not correct. Correct version dumped to stderr.");
}

void CheckTables()
{
    uint64_t val;
    int exp;

    val = 0xa000000000000000L; exp = 4; // 10
    CheckTable(val, exp, rgval64Power10, 15, "rgval64Power10", 1);
    CheckTable(val, exp, rgexp64Power10, 15, "rgexp64Power10", 2);

    val = 0x8e1bc9bf04000000L; exp = 54; //10^16
    CheckTable(val, exp, rgval64Power10By16, 21, "rgval64Power10By16", 1);
    CheckTable(val, exp, rgexp64Power10By16, 21, "rgexp64Power10By16", 3);

    val = 0xCCCCCCCCCCCCCCCDL; exp = -3; // 0.1
    CheckTable(val, exp, rgval64Power10 + 15, 15, "rgval64Power10 - inv", 1);

    val = 0xe69594bec44de160L; exp = -53; // 0.1^16
    CheckTable(val, exp, rgval64Power10By16 + 21, 21, "rgval64Power10By16 - inv", 1);
}

#endif // _DEBUG

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
#define DECIMAL_NEG ((uint8_t)0x80)
#define DECIMAL_PRECISION 29
#define DECIMAL_SCALE(dec)       ((dec).u.u.scale)
#define DECIMAL_SIGN(dec)        ((dec).u.u.sign)
#define DECIMAL_SIGNSCALE(dec)   ((dec).u.signscale)
#define DECIMAL_LO32(dec)        ((dec).v.v.Lo32)
#define DECIMAL_MID32(dec)       ((dec).v.v.Mid32)
#define DECIMAL_HI32(dec)        ((dec).Hi32)

    static void DecShiftLeft(Il2CppDecimal* value)
    {
        unsigned int c0 = DECIMAL_LO32(*value) & 0x80000000 ? 1 : 0;
        unsigned int c1 = DECIMAL_MID32(*value) & 0x80000000 ? 1 : 0;
        IL2CPP_ASSERT(value != NULL);

        DECIMAL_LO32(*value) <<= 1;
        DECIMAL_MID32(*value) = DECIMAL_MID32(*value) << 1 | c0;
        DECIMAL_HI32(*value) = DECIMAL_HI32(*value) << 1 | c1;
    }

    static int D32AddCarry(uint32_t* value, uint32_t i)
    {
        uint32_t v = *value;
        uint32_t sum = v + i;
        *value = sum;
        return sum < v || sum < i ? 1 : 0;
    }

    static void DecAdd(Il2CppDecimal *value, Il2CppDecimal* d)
    {
        IL2CPP_ASSERT(value != NULL && d != NULL);

        if (D32AddCarry(&DECIMAL_LO32(*value), DECIMAL_LO32(*d)))
        {
            if (D32AddCarry(&DECIMAL_MID32(*value), 1))
            {
                D32AddCarry(&DECIMAL_HI32(*value), 1);
            }
        }
        if (D32AddCarry(&DECIMAL_MID32(*value), DECIMAL_MID32(*d)))
        {
            D32AddCarry(&DECIMAL_HI32(*value), 1);
        }
        D32AddCarry(&DECIMAL_HI32(*value), DECIMAL_HI32(*d));
    }

    static void DecMul10(Il2CppDecimal* value)
    {
        Il2CppDecimal d = *value;
        IL2CPP_ASSERT(value != NULL);

        DecShiftLeft(value);
        DecShiftLeft(value);
        DecAdd(value, &d);
        DecShiftLeft(value);
    }

    static void DecAddInt32(Il2CppDecimal* value, unsigned int i)
    {
        IL2CPP_ASSERT(value != NULL);

        if (D32AddCarry(&DECIMAL_LO32(*value), i))
        {
            if (D32AddCarry(&DECIMAL_MID32(*value), 1))
            {
                D32AddCarry(&DECIMAL_HI32(*value), 1);
            }
        }
    }

    bool Number::NumberBufferToDecimal(uint8_t* number, Il2CppDecimal* value)
    {
        IL2CPP_ASSERT(number != NULL);
        IL2CPP_ASSERT(value != NULL);

        NUMBER* numberStruct = (NUMBER*)number;
        Il2CppChar* p = numberStruct->digits;
        Il2CppDecimal d;
        int e = numberStruct->scale;

        d.reserved = 0;
        DECIMAL_SIGNSCALE(d) = 0;
        DECIMAL_HI32(d) = 0;
        DECIMAL_LO32(d) = 0;
        DECIMAL_MID32(d) = 0;
        IL2CPP_ASSERT(p != NULL);
        if (!*p)
        {
            // To avoid risking an app-compat issue with pre 4.5 (where some app was illegally using Reflection to examine the internal scale bits), we'll only force
            // the scale to 0 if the scale was previously positive
            if (e > 0)
            {
                e = 0;
            }
        }
        else
        {
            if (e > DECIMAL_PRECISION)
                return 0;
            while ((e > 0 || (*p && e > -28)) && (DECIMAL_HI32(d) < 0x19999999 || (DECIMAL_HI32(d) == 0x19999999 && (DECIMAL_MID32(d) < 0x99999999 || (DECIMAL_MID32(d) == 0x99999999 && (DECIMAL_LO32(d) < 0x99999999 || (DECIMAL_LO32(d) == 0x99999999 && *p <= '5')))))))
            {
                DecMul10(&d);
                if (*p)
                    DecAddInt32(&d, *p++ - '0');
                e--;
            }
            if (*p++ >= '5')
            {
                bool round = true;
                if (*(p - 1) == '5' && *(p - 2) % 2 == 0)
                {
                    // Check if previous digit is even, only if the when we are unsure whether hows to do Banker's rounding
                    // For digits > 5 we will be roundinp up anyway.
                    int count = 20; // Look at the next 20 digits to check to round
                    while (*p == '0' && count != 0)
                    {
                        p++;
                        count--;
                    }
                    if (*p == '\0' || count == 0)
                        round = false; // Do nothing
                }

                if (round)
                {
                    DecAddInt32(&d, 1);
                    if ((DECIMAL_HI32(d) | DECIMAL_MID32(d) | DECIMAL_LO32(d)) == 0)
                    {
                        DECIMAL_HI32(d) = 0x19999999;
                        DECIMAL_MID32(d) = 0x99999999;
                        DECIMAL_LO32(d) = 0x9999999A;
                        e++;
                    }
                }
            }
        }
        if (e > 0)
            return 0;
        if (e <= -DECIMAL_PRECISION)
        {
            // Parsing a large scale zero can give you more precision than fits in the decimal.
            // This should only happen for actual zeros or very small numbers that round to zero.
            DECIMAL_SIGNSCALE(d) = 0;
            DECIMAL_HI32(d) = 0;
            DECIMAL_LO32(d) = 0;
            DECIMAL_MID32(d) = 0;
            DECIMAL_SCALE(d) = (DECIMAL_PRECISION - 1);
        }
        else
        {
            DECIMAL_SCALE(d) = (uint8_t)(-e);
        }

        DECIMAL_SIGN(d) = numberStruct->sign ? DECIMAL_NEG : 0;
        *value = d;
        return 1;
    }

//
// get 32-bit integer from at most 9 digits
//
    static inline unsigned DigitsToInt(Il2CppChar* p, int count)
    {
        IL2CPP_ASSERT(1 <= count && count <= 9);
        Il2CppChar* end = p + count;
        unsigned res = *p - '0';

        for (p = p + 1; p < end; p++)
            res = 10 * res + *p - '0';

        return res;
    }

    static uint64_t Mul64Lossy(uint64_t a, uint64_t b, int* pexp)
    {
        // it's ok to losse some precision here - Mul64 will be called
        // at most twice during the conversion, so the error won't propagate
        // to any of the 53 significant bits of the result
        uint64_t val = Mul32x32To64(a >> 32, b >> 32) +
            (Mul32x32To64(a >> 32, b) >> 32) +
            (Mul32x32To64(a, b >> 32) >> 32);

        // normalize
        if ((val & 0x8000000000000000ULL) == 0)
        {
            val <<= 1; *pexp -= 1;
        }

        return val;
    }

    static inline void NumberToDouble(NUMBER* number, double* value)
    {
        uint64_t val;
        int exp;
        Il2CppChar* src = number->digits;
        int remaining;
        int total;
        int count;
        int scale;
        int absscale;
        int index;

#ifdef _DEBUG
        static bool fCheckedTables = false;
        if (!fCheckedTables)
        {
            CheckTables();
            fCheckedTables = true;
        }
#endif // _DEBUG

        total = (int)utils::StringUtils::StrLen(src);
        remaining = total;

        // skip the leading zeros
        while (*src == '0')
        {
            remaining--;
            src++;
        }

        if (remaining == 0)
        {
            *value = 0;
            goto done;
        }

        count = std::min(remaining, 9);
        remaining -= count;
        val = DigitsToInt(src, count);

        if (remaining > 0)
        {
            count = std::min(remaining, 9);
            remaining -= count;

            // get the denormalized power of 10
            uint32_t mult = (uint32_t)(rgval64Power10[count - 1] >> (64 - rgexp64Power10[count - 1]));
            val = Mul32x32To64(val, mult) + DigitsToInt(src + 9, count);
        }

        scale = number->scale - (total - remaining);
        absscale = abs(scale);
        if (absscale >= 22 * 16)
        {
            // overflow / underflow
            *(uint64_t*)value = (scale > 0) ? 0x7FF0000000000000ULL : 0;
            goto done;
        }

        exp = 64;

        // normalize the mantissa
        if ((val & 0xFFFFFFFF00000000ULL) == 0)
        {
            val <<= 32; exp -= 32;
        }
        if ((val & 0xFFFF000000000000ULL) == 0)
        {
            val <<= 16; exp -= 16;
        }
        if ((val & 0xFF00000000000000ULL) == 0)
        {
            val <<= 8; exp -= 8;
        }
        if ((val & 0xF000000000000000ULL) == 0)
        {
            val <<= 4; exp -= 4;
        }
        if ((val & 0xC000000000000000ULL) == 0)
        {
            val <<= 2; exp -= 2;
        }
        if ((val & 0x8000000000000000ULL) == 0)
        {
            val <<= 1; exp -= 1;
        }

        index = absscale & 15;
        if (index)
        {
            int multexp = rgexp64Power10[index - 1];
            // the exponents are shared between the inverted and regular table
            exp += (scale < 0) ? (-multexp + 1) : multexp;

            uint64_t multval = rgval64Power10[index + ((scale < 0) ? 15 : 0) - 1];
            val = Mul64Lossy(val, multval, &exp);
        }

        index = absscale >> 4;
        if (index)
        {
            int multexp = rgexp64Power10By16[index - 1];
            // the exponents are shared between the inverted and regular table
            exp += (scale < 0) ? (-multexp + 1) : multexp;

            uint64_t multval = rgval64Power10By16[index + ((scale < 0) ? 21 : 0) - 1];
            val = Mul64Lossy(val, multval, &exp);
        }


        // round & scale down
        if ((uint32_t)val & (1 << 10))
        {
            // IEEE round to even
            uint64_t tmp = val + ((1 << 10) - 1) + (((uint32_t)val >> 11) & 1);
            if (tmp < val)
            {
                // overflow
                tmp = (tmp >> 1) | 0x8000000000000000ULL;
                exp += 1;
            }
            val = tmp;
        }

        // return the exponent to a biased state
        exp += 0x3FE;

        // handle overflow, underflow, "Epsilon - 1/2 Epsilon", denormalized, and the normal case
        if (exp <= 0)
        {
            if (exp == -52 && (val >= 0x8000000000000058ULL))
            {
                // round X where {Epsilon > X >= 2.470328229206232730000000E-324} up to Epsilon (instead of down to zero)
                val = 0x0000000000000001ULL;
            }
            else if (exp <= -52)
            {
                // underflow
                val = 0;
            }
            else
            {
                // denormalized
                val >>= (-exp + 11 + 1);
            }
        }
        else if (exp >= 0x7FF)
        {
            // overflow
            val = 0x7FF0000000000000ULL;
        }
        else
        {
            // normal postive exponent case
            val = ((uint64_t)exp << 52) + ((val >> 11) & 0x000FFFFFFFFFFFFFULL);
        }

        *(uint64_t*)value = val;

    done:
        if (number->sign)
            *(uint64_t*)value |= 0x8000000000000000ULL;
    }

    bool Number::NumberBufferToDouble(uint8_t* number, double* value)
    {
        double d = 0;
        NumberToDouble((NUMBER*)number, &d);
        unsigned int e = ((FPDOUBLE*)&d)->exp;
        unsigned int fmntLow = ((FPDOUBLE*)&d)->mantLo;
        unsigned int fmntHigh = ((FPDOUBLE*)&d)->mantHi;

        if (e == 0x7FF)
            return false;

        if (e == 0 && fmntLow == 0 && fmntHigh == 0)
            d = 0;

        *value = d;
        return true;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
