#pragma once

#include <stdint.h>
#include <string>

namespace il2cpp
{
namespace os
{
    class TimeZone
    {
    public:
        /*
         * This is heavily based on zdump.c from glibc 2.2.
         *
         *  * data[0]:  start of daylight saving time (in DateTime ticks).
         *  * data[1]:  end of daylight saving time (in DateTime ticks).
         *  * data[2]:  utcoffset (in TimeSpan ticks).
         *  * data[3]:  additional offset when daylight saving (in TimeSpan ticks).
         *  * name[0]:  name of this timezone when not daylight saving.
         *  * name[1]:  name of this timezone when daylight saving.
         *
         *  FIXME: This only works with "standard" Unix dates (years between 1900 and 2100) while
         *         the class library allows years between 1 and 9999.
         *
         *  Returns true on success and zero on failure.
         */
        static bool GetTimeZoneData(int32_t year, int64_t data[4], std::string names[2], bool* daylight_inverted);
    };
}
}
