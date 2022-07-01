#pragma once
#include "il2cpp-config.h"

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
    class LIBIL2CPP_CODEGEN_API Math
    {
    public:
        static double Acos(double val);
        static double Asin(double val);
        static double Atan(double val);
        static double Atan2(double y, double x);
        static double Cos(double val);
        static double Cosh(double val);
        static double Exp(double val);
        static double Floor(double x);
        static double Log(double x);
        static double Log10(double val);
        static double Pow(double val, double exp);
        static double Round(double x);
        static double RoundDigits(double x, int32_t digits);
        static double RoundMidpoint(double value, int32_t digits, bool away_from_zero);
        static double Sin(double val);
        static double Sinh(double val);
        static double Sqrt(double val);
        static double Tan(double val);
        static double Tanh(double val);
        static double Abs(double value);
        static double Ceiling(double a);
        static double SplitFractionDouble(double* value);
        static float Abs(float value);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
