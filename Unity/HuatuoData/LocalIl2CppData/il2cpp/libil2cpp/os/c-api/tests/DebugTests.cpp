#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Debug-c-api.h"
#include "../../Debug.h"


SUITE(Debug)
{
    TEST(ApiIsDebugPresentMatchesClassTest)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Debug::IsDebuggerPresent(), UnityPalIsDebuggerPresent());
    }
}

#endif // ENABLE_UNIT_TESTS
