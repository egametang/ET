#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include "os/Time.h"
#include <stdlib.h>
#include <stdio.h>

#include <sys/param.h>
#if IL2CPP_TARGET_DARWIN
#include <sys/sysctl.h>
#include <mach/mach.h>
#include <mach/mach_time.h>
#endif

#include <time.h>

#if IL2CPP_TARGET_LINUX
#include <sys/time.h>
#endif

/* a made up uptime of 300 seconds */
#define MADEUP_BOOT_TIME (300 * MTICKS_PER_SEC)

namespace il2cpp
{
namespace os
{
    const int64_t MTICKS_PER_SEC = 10000000;

    static int64_t
    GetBootTime()
    {
#if IL2CPP_TARGET_DARWIN
        int mib[2];
        size_t size;
        time_t now;
        struct timeval boottime;

        (void)time(&now);

        mib[0] = CTL_KERN;
        mib[1] = KERN_BOOTTIME;

        size = sizeof(boottime);

        if (sysctl(mib, 2, &boottime, &size, NULL, 0) != -1)
            return (int64_t)((now - boottime.tv_sec) * MTICKS_PER_SEC);
#else
        FILE *uptime = fopen("/proc/uptime", "r");

        if (uptime)
        {
            double upt;

            if (fscanf(uptime, "%lf", &upt) == 1)
            {
                const int64_t now = Time::GetTicks100NanosecondsMonotonic();
                fclose(uptime);
                return now - (int64_t)(upt * MTICKS_PER_SEC);
            }

            fclose(uptime);
        }

#endif
        /* a made up uptime of 300 seconds */
        return (int64_t)MADEUP_BOOT_TIME;
    }

    uint32_t Time::GetTicksMillisecondsMonotonic()
    {
#if IL2CPP_TARGET_ANDROID
        struct timespec ts;
        if (clock_gettime(CLOCK_MONOTONIC, &ts) != 0)
        {
            return (int64_t)MADEUP_BOOT_TIME;
        }
        return (ts.tv_sec * 1000) + (ts.tv_nsec / 1000000);
#else
        static int64_t boot_time = 0;
        int64_t now;
        if (!boot_time)
            boot_time = GetBootTime();
        now = GetTicks100NanosecondsMonotonic();
        return (uint32_t)((now - boot_time) / 10000);
#endif
    }

    int64_t Time::GetTicks100NanosecondsMonotonic()
    {
        struct timeval tv;
#if IL2CPP_TARGET_DARWIN
        /* http://developer.apple.com/library/mac/#qa/qa1398/_index.html */
        static mach_timebase_info_data_t timebase;
        uint64_t now = mach_absolute_time();
        if (timebase.denom == 0)
        {
            mach_timebase_info(&timebase);
            timebase.denom *= 100; /* we return 100ns ticks */
        }
        return now * timebase.numer / timebase.denom;
#elif defined(CLOCK_MONOTONIC)
        struct timespec tspec;
        static struct timespec tspec_freq = {0};
        static int can_use_clock = 0;
        if (!tspec_freq.tv_nsec)
        {
            can_use_clock = clock_getres(CLOCK_MONOTONIC, &tspec_freq) == 0;
        }
        if (can_use_clock)
        {
            if (clock_gettime(CLOCK_MONOTONIC, &tspec) == 0)
            {
                return ((int64_t)tspec.tv_sec * MTICKS_PER_SEC + tspec.tv_nsec / 100);
            }
        }

#endif
        if (gettimeofday(&tv, NULL) == 0)
            return ((int64_t)tv.tv_sec * 1000000 + tv.tv_usec) * 10;
        return 0;
    }

/*
 * Magic number to convert a time which is relative to
 * Jan 1, 1970 into a value which is relative to Jan 1, 0001.
 */
    const uint64_t EPOCH_ADJUST = ((uint64_t)62135596800LL);

    int64_t Time::GetTicks100NanosecondsDateTime()
    {
        struct timeval tv;
        if (gettimeofday(&tv, NULL) == 0)
            return (((int64_t)tv.tv_sec + EPOCH_ADJUST) * 1000000 + tv.tv_usec) * 10;
        return 0;
    }

    static const int64_t kSecondsBetween1601And1970 = 11644473600LL;
    static const int64_t kSecondsTo100NanoSeconds = 10000000;

    int64_t Time::GetSystemTimeAsFileTime()
    {
        timeval currentTime;
        int getTimeOfDayResult = gettimeofday(&currentTime, NULL);
        IL2CPP_ASSERT(getTimeOfDayResult == 0 && "gettimeofday() failed");

        return kSecondsTo100NanoSeconds * (static_cast<int64_t>(currentTime.tv_sec) + kSecondsBetween1601And1970) + 10 * currentTime.tv_usec;
    }
}
}

#endif
