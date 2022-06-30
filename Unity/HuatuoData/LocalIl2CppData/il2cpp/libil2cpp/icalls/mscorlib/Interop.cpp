#include "il2cpp-config.h"
#include "Interop.h"
#include <stdio.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
    // int ves_icall_Interop_Sys_DoubleToString(double value, char *format, char *buffer, int bufferLength)
    int32_t Interop::Sys::DoubleToString(double value, char* format, char* buffer, int32_t bufferLength)
    {
#if IL2CPP_TARGET_ARMV7
        /* workaround for faulty vcmp.f64 implementation on some 32bit ARM CPUs */
        int64_t bits = *(int64_t *)&value;
        if (bits == 0x1)   /* 4.9406564584124654E-324 */
        {
            IL2CPP_ASSERT(!strcmp(format, "%.40e"));
            return snprintf(buffer, bufferLength, "%s", "4.9406564584124654417656879286822137236506e-324");
        }
        else if (bits == 0x4)     /* 2E-323 */
        {
            IL2CPP_ASSERT(!strcmp(format, "%.40e"));
            return snprintf(buffer, bufferLength, "%s", "1.9762625833649861767062751714728854894602e-323");
        }
#endif

        return ::snprintf(buffer, bufferLength, format, value);
    }
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
