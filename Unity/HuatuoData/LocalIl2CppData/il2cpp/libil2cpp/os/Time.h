#pragma once

#include <stdint.h>

namespace il2cpp
{
namespace os
{
    class Time
    {
    public:
        /* Returns the number of milliseconds from boot time: this should be monotonic */
        static uint32_t GetTicksMillisecondsMonotonic();

        /* Returns the number of 100ns ticks from unspecified time: this should be monotonic */
        static int64_t GetTicks100NanosecondsMonotonic();

        /* Returns the number of 100ns ticks since 1/1/1, UTC timezone */
        static int64_t GetTicks100NanosecondsDateTime();

        // Retrieves the current system date and time. The information is in Coordinated Universal Time(UTC) format.
        static int64_t GetSystemTimeAsFileTime();
    };
}
}
