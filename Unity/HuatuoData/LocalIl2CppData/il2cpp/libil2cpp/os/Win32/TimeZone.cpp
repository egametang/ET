#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/TimeZone.h"
#include "os/Win32/WindowsHeaders.h"

/*
 * Magic number to convert FILETIME base Jan 1, 1601 to DateTime - base Jan, 1, 0001
 */
const uint64_t FILETIME_ADJUST = ((uint64_t)504911232000000000LL);

namespace il2cpp
{
namespace os
{
    static void
    convert_to_absolute_date(SYSTEMTIME *date)
    {
#define IS_LEAP(y) ((y % 4) == 0 && ((y % 100) != 0 || (y % 400) == 0))
        static int days_in_month[] = { 31, 28, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        static int leap_days_in_month[] = { 31, 29, 31, 30, 31, 30, 31, 31, 30, 31, 30, 31};
        /* from the calendar FAQ */
        int a = (14 - date->wMonth) / 12;
        int y = date->wYear - a;
        int m = date->wMonth + 12 * a - 2;
        int d = (1 + y + y / 4 - y / 100 + y / 400 + (31 * m) / 12) % 7;

        /* d is now the day of the week for the first of the month (0 == Sunday) */

        int day_of_week = date->wDayOfWeek;

        /* set day_in_month to the first day in the month which falls on day_of_week */
        int day_in_month = 1 + (day_of_week - d);
        if (day_in_month <= 0)
            day_in_month += 7;

        /* wDay is 1 for first weekday in month, 2 for 2nd ... 5 means last - so work that out allowing for days in the month */
        date->wDay = day_in_month + (date->wDay - 1) * 7;
        if (date->wDay > (IS_LEAP(date->wYear) ? leap_days_in_month[date->wMonth - 1] : days_in_month[date->wMonth - 1]))
            date->wDay -= 7;
    }

// names[0] - standardName
// names[1] - daylightName
    bool TimeZone::GetTimeZoneData(int32_t year, int64_t data[4], std::string names[2], bool* daylight_inverted)
    {
        //On Windows, we should always load timezones in managed
        return false;
    }
}
}

#endif
