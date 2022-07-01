#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

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
namespace Threading
{
    class LIBIL2CPP_CODEGEN_API Interlocked
    {
    public:
        static int32_t Add(int32_t* location1, int32_t value);
        static int64_t Add64(int64_t* location1, int64_t value);
        static int32_t CompareExchange(int32_t* location, int32_t value, int32_t comparand);
        static int64_t CompareExchange64(int64_t* location1, int64_t value, int64_t comparand);
        static double CompareExchangeDouble(double* location1, double value, double comparand);
        static intptr_t CompareExchangeIntPtr(intptr_t* location, intptr_t value, intptr_t comparand);
        static float CompareExchangeSingle(float* location1, float value, float comparand);
        static void* CompareExchange_T(void** location, void* value, void* comparand);
        static int32_t Decrement(int32_t* location);
        static int64_t Decrement64(int64_t* location);
        static intptr_t ExchangeIntPtr(intptr_t* location, intptr_t value);
        static int32_t Exchange(int32_t* location1, int32_t value);
        static int64_t Exchange64(int64_t* location1, int64_t value);
        static double ExchangeDouble(double* location1, double value);
        static void* ExchangePointer(void** location1, void* value);
        static float ExchangeSingle(float* location1, float value);
        static int32_t Increment(int32_t* value);
        static int64_t Increment64(int64_t* location);
        static int64_t Read(int64_t* location);
        static int32_t CompareExchange(int32_t* location1, int32_t value, int32_t comparand, bool* succeeded);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace tiny */
