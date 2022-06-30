#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Console-c-api.h"
#include "../../Console.h"

struct ConsoleFixture
{
    ConsoleFixture()
    {
        ms_timeout = 0;
    }

    ~ConsoleFixture()
    {
    }

    int32_t ms_timeout;
    int32_t *size;
    int32_t *classSize;
    uint8_t controlChars[17];
};

SUITE(Console)
{
// Console commands are only supported on POSIX platforms
#if IL2CPP_TARGET_POSIX
    TEST_FIXTURE(ConsoleFixture, InternalKeyAvailableReturnsValid)
    {
        CHECK_EQUAL(0, UnityPalConsoleInternalKeyAvailable(ms_timeout));
    }

    TEST_FIXTURE(ConsoleFixture, InternalKeyAvailableMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::Console::InternalKeyAvailable(ms_timeout), UnityPalConsoleInternalKeyAvailable(ms_timeout));
    }

    TEST_FIXTURE(ConsoleFixture, SetBreakReturnsValid)
    {
        CHECK_EQUAL(0, UnityPalConsoleSetBreak(true));
    }

    TEST_FIXTURE(ConsoleFixture, SetBreakMatchesClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Console::SetBreak(true), UnityPalConsoleSetBreak(true));
    }

    TEST_FIXTURE(ConsoleFixture, SetEchoReturnsValid)
    {
        CHECK_EQUAL(0, UnityPalConsoleSetEcho(true));
    }

    TEST_FIXTURE(ConsoleFixture, SetEchoMatchesClass)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Console::SetEcho(true), UnityPalConsoleSetBreak(true));
    }

    TEST_FIXTURE(ConsoleFixture, TtySetupMatchesClass)
    {
        il2cpp::os::Console::TtySetup("", "", controlChars, &classSize);
        UnityPalConsoleTtySetup("", "", controlChars, &size);
        CHECK_EQUAL(*classSize, *size);
    }

#endif
}

#endif // ENABLE_UNIT_TESTS
