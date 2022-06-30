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
        static double Abs(double value);
        static double Acos(double d);
        static double Acosh(double d);
        static double Asin(double d);
        static double Asinh(double d);
        static double Atan(double d);
        static double Atan2(double y, double x);
        static double Atanh(double d);
        static double Cbrt(double d);
        static double Ceiling(double a);
        static double Cos(double d);
        static double Cosh(double value);
        static double Exp(double d);
        static double Floor(double d);
        static double FMod(double x, double y);
        static double Log(double d);
        static double Log10(double d);
        static double ModF(double x, double* intptr);
        static double Pow(double val, double exp);
        static double Round(double x);
        static double RoundDigits(double x, int32_t digits);
        static double RoundMidpoint(double value, int32_t digits, bool away_from_zero);
        static double Sin(double a);
        static double Sinh(double value);
        static double Sqrt(double d);
        static double Tan(double a);
        static double Tanh(double value);
        static float Abs(float value);
#if IL2CPP_TINY
        static double SplitFractionDouble(double* value);
#endif
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
