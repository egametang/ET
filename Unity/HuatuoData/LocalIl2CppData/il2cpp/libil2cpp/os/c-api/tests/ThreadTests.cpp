#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../../Thread.h"
#include "../Thread-c-api.h"

SUITE(Thread)
{
    TEST(GetCurrentThreadIdMatchesClass)
    {
        CHECK(UnityPalGetCurrentThreadId() == il2cpp::os::Thread::GetCurrentThread()->Id());
    }
}

#endif // ENABLE_UNIT_TESTS
