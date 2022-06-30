#pragma once

#include "../C/Baselib_Timer.h"
#include <chrono>
#include <cmath>

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        using timeout_ms = std::chrono::duration<uint32_t, std::milli>;
        using timeout_us = std::chrono::duration<uint64_t, std::micro>;

        struct high_precision_clock
        {
            using duration = std::chrono::duration<double, std::nano>;
            using time_point = std::chrono::time_point<high_precision_clock, duration>;
            using rep = duration::rep;
            using period = duration::period;

            static constexpr bool is_steady = true;

            static time_point now()
            {
                return time_point_from_ticks(now_in_ticks());
            }

            static Baselib_Timer_Ticks now_in_ticks()
            {
                return Baselib_Timer_GetHighPrecisionTimerTicks();
            }

            static duration duration_from_ticks(Baselib_Timer_Ticks ticks)
            {
                return duration(ticks * Baselib_Timer_TickToNanosecondsConversionFactor);
            }

            static Baselib_Timer_Ticks ticks_from_duration_roundup(duration d)
            {
                double ticks = d.count() / Baselib_Timer_TickToNanosecondsConversionFactor;
                return (Baselib_Timer_Ticks)std::ceil(ticks);
            }

            static time_point time_point_from_ticks(Baselib_Timer_Ticks ticks)
            {
                return time_point(duration_from_ticks(ticks));
            }
        };
    }
}
