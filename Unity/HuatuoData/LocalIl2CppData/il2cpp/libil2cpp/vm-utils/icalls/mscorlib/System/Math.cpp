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
    double Math::Abs(double value)
    {
        return fabs(value);
    }

    double Math::Acos(double d)
    {
        return acos(d);
    }

    double Math::Acosh(double d)
    {
        return acosh(d);
    }

    double Math::Asin(double d)
    {
        return asin(d);
    }

    double Math::Asinh(double d)
    {
        return asinh(d);
    }

    double Math::Atan(double d)
    {
        return atan(d);
    }

    double Math::Atan2(double y, double x)
    {
        return atan2(y, x);
    }

    double Math::Atanh(double d)
    {
        return atanh(d);
    }

    double Math::Cbrt(double d)
    {
        return cbrt(d);
    }

    double Math::Ceiling(double a)
    {
        return ceil(a);
    }

    double Math::Cos(double d)
    {
        return cos(d);
    }

    double Math::Cosh(double value)
    {
        return cosh(value);
    }

    double Math::Exp(double d)
    {
        return exp(d);
    }

    double Math::Floor(double d)
    {
        return floor(d);
    }

    double Math::FMod(double x, double y)
    {
        return fmod(x, y);
    }

    double Math::Log(double d)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Math::Log, "Determin what value of NAN to use");

        if (d == 0)
            return -HUGE_VAL;
        else if (d < 0)
            return std::numeric_limits<double>::signaling_NaN();
        //return NAN;

        return log(d);
    }

    double Math::Log10(double d)
    {
        return log10(d);
    }

    double Math::ModF(double x, double* d)
    {
        return modf(x, d);
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

    double Math::Sin(double a)
    {
        return sin(a);
    }

    double Math::Sinh(double value)
    {
        return sinh(value);
    }

    double Math::Sqrt(double d)
    {
        return sqrt(d);
    }

    double Math::Tan(double a)
    {
        return tan(a);
    }

    double Math::Tanh(double value)
    {
        return tanh(value);
    }

    float Math::Abs(float value)
    {
        return fabsf(value);
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

#if IL2CPP_TINY
    double Math::SplitFractionDouble(double* value)
    {
        return modf(*value, value);
    }

#endif
} /*namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
