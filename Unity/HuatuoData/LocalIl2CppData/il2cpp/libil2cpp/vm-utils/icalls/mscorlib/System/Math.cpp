#include "il2cpp-config.h"
#include <cmath>
#include <limits>
#include <float.h>
#include "Math.h"
#include "vm/Exception.h"

#if RUNTIME_TINY
namespace tiny
#else
namespace il2cpp
#endif
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    double Math::Acos(double val)
    {
        return acos(val);
    }

    double Math::Asin(double val)
    {
        return asin(val);
    }

    double Math::Atan(double val)
    {
        return atan(val);
    }

    double Math::Atan2(double y, double x)
    {
        return atan2(y, x);
    }

    double Math::Cos(double val)
    {
        return cos(val);
    }

    double Math::Cosh(double val)
    {
        return cosh(val);
    }

    double Math::Exp(double val)
    {
        return exp(val);
    }

    double Math::Floor(double x)
    {
        return floor(x);
    }

    double Math::Log(double x)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Math::Log, "Determin what value of NAN to use");

        if (x == 0)
            return -HUGE_VAL;
        else if (x < 0)
            return std::numeric_limits<double>::signaling_NaN();
        //return NAN;

        return log(x);
    }

    double Math::Log10(double val)
    {
        return log10(val);
    }

    static bool IsInteger(double value)
    {
        double unused;
        return std::modf(value, &unused) == 0.0;
    }

    // Use this function to test for odd integers instead of converting a
    // double to int64_t then ANDing with 1 (or modulo). In C++, double to integer
    // conversions are truncated but the behavior is undefined if the truncated
    // value cannot be represented in the destination type. This means that huge
    // doubles may not be handled correctly.
    static bool IsOddInteger(double value)
    {
        return std::fmod(value, 2.0) == std::copysign(1.0, value);
    }

    double Math::Pow(double val, double exp)
    {
        if (std::isnan(val))
            return val;
        if (std::isnan(exp))
            return exp;

        if (val > -1 && val < 1 && exp == -std::numeric_limits<double>::infinity())
            return std::numeric_limits<double>::infinity();

        if (val > -1 && val < 1 && exp == std::numeric_limits<double>::infinity())
            return 0.0;

        if ((val < -1 || val > 1) && exp == -std::numeric_limits<double>::infinity())
            return 0.0;

        if ((val < -1 || val > 1) && exp == std::numeric_limits<double>::infinity())
            return std::numeric_limits<double>::infinity();

        if (val == -std::numeric_limits<double>::infinity())
        {
            if (exp < 0)
                return 0.0;

            if (exp > 0)
            {
                return IsOddInteger(exp) ? -std::numeric_limits<double>::infinity() : std::numeric_limits<double>::infinity();
            }

            return 1.0;
        }

        if (val < 0)
        {
            if (!IsInteger(exp) || exp == std::numeric_limits<double>::infinity() || exp == -std::numeric_limits<double>::infinity())
                return std::numeric_limits<double>::quiet_NaN();
        }

        double res = pow(val, exp);
        if (std::isnan(res))
            return 1.0;

        if (res == -0.0)
            return 0.0;

        return res;
    }

    double Math::Round(double x)
    {
        double int_part, dec_part;
        int_part = floor(x);
        dec_part = x - int_part;
        if (((dec_part == 0.5) &&
             ((2.0 * ((int_part / 2.0) - floor(int_part / 2.0))) != 0.0)) ||
            (dec_part > 0.5))
        {
            int_part++;
        }
        return int_part;
    }

    double Math::RoundDigits(double value, int32_t digits)
    {
        return RoundMidpoint(value, digits, false);
    }

    double Math::RoundMidpoint(double value, int32_t digits, bool away_from_zero)
    {
        double p;
        if (value == HUGE_VAL)
            return HUGE_VAL;
        if (value == -HUGE_VAL)
            return -HUGE_VAL;
        if (digits == 0 && !away_from_zero)
            return Round(value);

        p = pow(10.0, digits);

        if (away_from_zero)
            return std::round(value * p) / p;
        else
            return std::rint(value * p) / p;
    }

    double Math::Sin(double val)
    {
        return sin(val);
    }

    double Math::Sinh(double val)
    {
        return sinh(val);
    }

    double Math::Sqrt(double val)
    {
        return sqrt(val);
    }

    double Math::Tan(double val)
    {
        return tan(val);
    }

    double Math::Tanh(double val)
    {
        return tanh(val);
    }

    double Math::Abs(double value)
    {
        return fabs(value);
    }

    double Math::Ceiling(double a)
    {
        return ceil(a);
    }

    double Math::SplitFractionDouble(double* value)
    {
        return modf(*value, value);
    }

    float Math::Abs(float value)
    {
        return fabsf(value);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
