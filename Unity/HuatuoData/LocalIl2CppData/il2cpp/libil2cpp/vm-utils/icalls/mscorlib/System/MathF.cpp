#include "il2cpp-config.h"
#include "MathF.h"

#include <cmath>
#include <limits>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    static bool IsInteger(float value)
    {
        double unused;
        return std::modf(value, &unused) == 0.0;
    }

    // Use this function to test for odd integers instead of converting a
    // double to int64_t then ANDing with 1 (or modulo). In C++, double to integer
    // conversions are truncated but the behavior is undefined if the truncated
    // value cannot be represented in the destination type. This means that huge
    // doubles may not be handled correctly.
    static bool IsOddInteger(float value)
    {
        return std::fmod(value, 2.0) == std::copysign(1.0, value);
    }

    float MathF::Pow(float val, float exp)
    {
        if (std::isnan(val))
            return val;
        if (std::isnan(exp))
            return exp;

        if (val > -1 && val < 1 && exp == -std::numeric_limits<float>::infinity())
            return std::numeric_limits<float>::infinity();

        if (val > -1 && val < 1 && exp == std::numeric_limits<float>::infinity())
            return 0.0;

        if ((val < -1 || val > 1) && exp == -std::numeric_limits<float>::infinity())
            return 0.0;

        if ((val < -1 || val > 1) && exp == std::numeric_limits<float>::infinity())
            return std::numeric_limits<float>::infinity();

        if (val == -std::numeric_limits<float>::infinity())
        {
            if (exp < 0)
                return 0.0;

            if (exp > 0)
            {
                return IsOddInteger(exp) ? -std::numeric_limits<float>::infinity() : std::numeric_limits<float>::infinity();
            }

            return 1.0;
        }

        if (val < 0)
        {
            if (!IsInteger(exp) || exp == std::numeric_limits<float>::infinity() || exp == -std::numeric_limits<float>::infinity())
                return std::numeric_limits<float>::quiet_NaN();
        }

        float res = pow(val, exp);
        if (std::isnan(res))
            return 1.0;

        if (res == -0.0)
            return 0.0;

        return res;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
