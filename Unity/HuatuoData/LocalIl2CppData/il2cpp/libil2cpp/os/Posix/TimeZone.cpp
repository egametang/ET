#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/TimeZone.h"

#include <sys/time.h>
#include <time.h>

namespace il2cpp
{
namespace os
{
/*
 * Magic number to convert a time which is relative to
 * Jan 1, 1970 into a value which is relative to Jan 1, 0001.
 */
    const uint64_t TZ_EPOCH_ADJUST = ((uint64_t)62135596800LL);

    /*
    * Return's the offset from GMT of a local time.
    *
    *  tm is a local time
    *  t  is the same local time as seconds.
    */
    static int
    GMTOffset(struct tm *tm, time_t t)
    {
#if defined(HAVE_TM_GMTOFF)
        return tm->tm_gmtoff;
#else
        struct tm g;
        time_t t2;
        g = *gmtime(&t);
        g.tm_isdst = tm->tm_isdst;
        t2 = mktime(&g);
        return (int)difftime(t, t2);
#endif
    }

    bool TimeZone::GetTimeZoneData(int32_t year, int64_t data[4], std::string names[2], bool* daylight_inverted)
    {
        struct tm start, tt;
        time_t t;

        long int gmtoff, gmtoff_start;
        int is_transitioned = 0, day;
        char tzone[64];

        /*
         * no info is better than crashing: we'll need our own tz data
         * to make this work properly, anyway. The range is probably
         * reduced to 1970 .. 2037 because that is what mktime is
         * guaranteed to support (we get into an infinite loop
         * otherwise).
         */

        memset(&start, 0, sizeof(start));

        start.tm_mday = 1;
        start.tm_year = year - 1900;

        t = mktime(&start);

        if ((year < 1970) || (year > 2037) || (t == -1))
        {
            t = time(NULL);
            tt = *localtime(&t);
            strftime(tzone, sizeof(tzone), "%Z", &tt);
            names[0] = tzone;
            names[1] = tzone;
            *daylight_inverted = false;
            return true;
        }

        *daylight_inverted = start.tm_isdst;

        gmtoff = GMTOffset(&start, t);
        gmtoff_start = gmtoff;

        /* For each day of the year, calculate the tm_gmtoff. */
        for (day = 0; day < 365; day++)
        {
            t += 3600 * 24;
            tt = *localtime(&t);

            /* Daylight saving starts or ends here. */
            if (GMTOffset(&tt, t) != gmtoff)
            {
                struct tm tt1;
                time_t t1;

                /* Try to find the exact hour when daylight saving starts/ends. */
                t1 = t;
                do
                {
                    t1 -= 3600;
                    tt1 = *localtime(&t1);
                }
                while (GMTOffset(&tt1, t1) != gmtoff);

                /* Try to find the exact minute when daylight saving starts/ends. */
                do
                {
                    t1 += 60;
                    tt1 = *localtime(&t1);
                }
                while (GMTOffset(&tt1, t1) == gmtoff);
                t1 += gmtoff;
                strftime(tzone, sizeof(tzone), "%Z", &tt);

                /* Write data, if we're already in daylight saving, we're done. */
                if (is_transitioned)
                {
                    if (!start.tm_isdst)
                        names[0] = tzone;
                    else
                        names[1] = tzone;

                    data[1] = ((int64_t)t1 + TZ_EPOCH_ADJUST) * 10000000L;
                    return true;
                }
                else
                {
                    if (!start.tm_isdst)
                        names[1] = tzone;
                    else
                        names[0] = tzone;

                    data[0] = ((int64_t)t1 + TZ_EPOCH_ADJUST) * 10000000L;
                    is_transitioned = 1;
                }

                /* This is only set once when we enter daylight saving. */
                if (!*daylight_inverted)
                {
                    data[2] = (int64_t)gmtoff * 10000000L;
                    data[3] = (int64_t)(GMTOffset(&tt, t) - gmtoff) * 10000000L;
                }
                else
                {
                    data[2] = (int64_t)(gmtoff_start + (GMTOffset(&tt, t) - gmtoff)) * 10000000L;
                    data[3] = (int64_t)(gmtoff - GMTOffset(&tt, t)) * 10000000L;
                }

                gmtoff = GMTOffset(&tt, t);
            }
        }

        if (!is_transitioned)
        {
            strftime(tzone, sizeof(tzone), "%Z", &tt);
            names[0] = tzone;
            names[1] = tzone;
            data[0] = 0;
            data[1] = 0;
            data[2] = (int64_t)gmtoff * 10000000L;
            data[3] = 0;
            *daylight_inverted = false;
        }

        return true;
    }
}
}

#endif
