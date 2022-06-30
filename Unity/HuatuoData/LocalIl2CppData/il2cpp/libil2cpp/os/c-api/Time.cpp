#include "os/c-api/il2cpp-config-platforms.h"

#include "os/Time.h"

#include <stdint.h>
#include <time.h>

extern "C"
{
#if !RUNTIME_TINY
    uint32_t UnityPalGetTicksMillisecondsMonotonic()
    {
        return il2cpp::os::Time::GetTicksMillisecondsMonotonic();
    }

    int64_t UnityPalGetTicks100NanosecondsDateTime()
    {
        return il2cpp::os::Time::GetTicks100NanosecondsDateTime();
    }

#endif

    int64_t STDCALL UnityPalGetTicks100NanosecondsMonotonic()
    {
        return il2cpp::os::Time::GetTicks100NanosecondsMonotonic();
    }

    int64_t STDCALL UnityPalGetSystemTimeAsFileTime()
    {
        return il2cpp::os::Time::GetSystemTimeAsFileTime();
    }

#if IL2CPP_TINY
    int32_t STDCALL UnityPalGetUtcOffsetHours(int64_t secondsSinceEpoch)
    {
        struct tm* utcTime = gmtime((time_t*)&secondsSinceEpoch);
        time_t utc = mktime(utcTime);

        struct tm* localTime = localtime((time_t*)&secondsSinceEpoch);
        time_t local = mktime(localTime);

        time_t offsetSeconds = local - utc;

        int daylightSavingsOffset = 0;
        if (localTime->tm_isdst)
            daylightSavingsOffset = 1;
        return (int32_t)offsetSeconds / 3600 + daylightSavingsOffset;
    }

    bool STDCALL UnityPalIsDaylightSavingsTime(int64_t secondsSinceEpoch)
    {
        struct tm* tt = localtime((time_t*)&secondsSinceEpoch);
        int32_t daylightSavingsOffset = 0;
        return tt->tm_isdst > 0;
    }

#endif
}
