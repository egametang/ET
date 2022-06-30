#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/Time.h"
#include "os/Win32/WindowsHeaders.h"
#include "utils/MathUtils.h"

#define MTICKS_PER_SEC 10000000LL

namespace il2cpp
{
namespace os
{
    static LARGE_INTEGER s_PerformanceCounterFrequency;

    static inline void InitializePerformanceCounterFrequency()
    {
        if (!s_PerformanceCounterFrequency.QuadPart)
        {
            // From MSDN: On systems that run Windows XP or later, the function will always succeed and will thus never return zero.
            // so I'll just assume we never run on older than XP

            BOOL qpfResult = QueryPerformanceFrequency(&s_PerformanceCounterFrequency);
            IL2CPP_ASSERT(qpfResult != FALSE);
        }
    }

    uint32_t Time::GetTicksMillisecondsMonotonic()
    {
        InitializePerformanceCounterFrequency();

        LARGE_INTEGER value;
        QueryPerformanceCounter(&value);
        return static_cast<uint32_t>(value.QuadPart * 1000 / s_PerformanceCounterFrequency.QuadPart);
    }

    int64_t Time::GetTicks100NanosecondsMonotonic()
    {
        InitializePerformanceCounterFrequency();

        LARGE_INTEGER value;
        QueryPerformanceCounter(&value);
        return utils::MathUtils::A_Times_B_DividedBy_C(value.QuadPart, MTICKS_PER_SEC, s_PerformanceCounterFrequency.QuadPart);
    }

/*
 * Magic number to convert FILETIME base Jan 1, 1601 to DateTime - base Jan, 1, 0001
 */
    const uint64_t FILETIME_ADJUST = ((uint64_t)504911232000000000LL);

    int64_t Time::GetTicks100NanosecondsDateTime()
    {
        ULARGE_INTEGER ft;

        IL2CPP_ASSERT(sizeof(ft) == sizeof(FILETIME));

        ::GetSystemTimeAsFileTime((FILETIME*)&ft);
        return FILETIME_ADJUST + ft.QuadPart;
    }

    int64_t Time::GetSystemTimeAsFileTime()
    {
        int64_t fileTime;
        ::GetSystemTimeAsFileTime(reinterpret_cast<FILETIME*>(&fileTime));
        return fileTime;
    }
}
}

#endif
