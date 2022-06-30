#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Messages-c-api.h"
#include "../../Messages.h"

SUITE(Messages)
{
    TEST(CheckMessageFromCodeReturnsAGoodString)
    {
        CHECK_EQUAL("Is a directory", UnityPalMessagesFromCode(il2cpp::os::kErrorDirectory));
    }

    TEST(CheckMessageFromCodeReturnsSameStringAsApi)
    {
        CHECK_EQUAL(il2cpp::os::Messages::FromCode(il2cpp::os::kErrorDirectory), UnityPalMessagesFromCode(il2cpp::os::kErrorDirectory));
    }
}

#endif // ENABLE_UNIT_TESTS
