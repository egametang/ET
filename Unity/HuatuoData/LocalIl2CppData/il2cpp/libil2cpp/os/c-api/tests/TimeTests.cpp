#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Time-c-api.h"
#include "../../Time.h"

SUITE(Time)
{
    TEST(TicksMillisecondsMonotonicTestValid)
    {
        CHECK(UnityPalGetTicksMillisecondsMonotonic() > 0);
    }

    TEST(TicksMillisecondsMonotonicEqualsClass)
    {
        CHECK_CLOSE(il2cpp::os::Time::GetTicksMillisecondsMonotonic(), UnityPalGetTicksMillisecondsMonotonic(), 100);
    }

    TEST(Ticks100NanosecondsMonotonicTestValid)
    {
        CHECK(UnityPalGetTicks100NanosecondsMonotonic() > 0);
    }

    TEST(Ticks100NanosecondsMonotonicEqualsClass)
    {
        // This number is fairly sensitive to time between calls, check to see if they are close enough, chop off the last few digits
        CHECK_CLOSE(il2cpp::os::Time::GetTicks100NanosecondsMonotonic() / 1000L, UnityPalGetTicks100NanosecondsMonotonic() / 1000L, 100);
    }

    TEST(GetTicks100NanosecondsDateTime)
    {
        CHECK(UnityPalGetTicks100NanosecondsDateTime() > 0);
    }

    TEST(GetTicks100NanosecondsDateTimeEqualsClass)
    {
        CHECK_CLOSE(il2cpp::os::Time::GetTicks100NanosecondsDateTime() / 1000L, UnityPalGetTicks100NanosecondsDateTime() / 1000L, 100);
    }

// GetSystemTimeAsFileTime is not implemented on PS4
#if !IL2CPP_TARGET_PS4
    TEST(GetSystemTimeAsFileTime)
    {
        CHECK(UnityPalGetSystemTimeAsFileTime() > 0);
    }

    TEST(GetSystemTimeAsFileTimeEqualsClass)
    {
        CHECK_CLOSE(il2cpp::os::Time::GetSystemTimeAsFileTime() / 1000L, UnityPalGetSystemTimeAsFileTime() / 1000L, 100);
    }
#endif
}

#endif // ENABLE_UNIT_TESTS
