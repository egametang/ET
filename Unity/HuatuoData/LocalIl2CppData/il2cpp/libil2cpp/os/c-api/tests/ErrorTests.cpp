#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Error-c-api.h"
#include "../../Error.h"

struct ErrorFixture
{
    ErrorFixture()
    {
        UnityPalSetLastError(il2cpp::os::kErrorWrongDisk);
    }
};

SUITE(Error)
{
    TEST_FIXTURE(ErrorFixture, SetErrorSuccessfulSetsToWrongDiskError)
    {
        // Note: the set actually occurs in the fixture, using
        // class version to verify the set worked
        CHECK_EQUAL(il2cpp::os::kErrorWrongDisk, il2cpp::os::Error::GetLastError());
    }

    TEST_FIXTURE(ErrorFixture, GetErrorMatchesWrongDiskError)
    {
        CHECK_EQUAL(il2cpp::os::kErrorWrongDisk, UnityPalGetLastError());
    }

    TEST_FIXTURE(ErrorFixture, ApiGetErrorMatchesClassGetError)
    {
        CHECK_EQUAL(il2cpp::os::Error::GetLastError(), UnityPalGetLastError());
    }

    TEST(ApiGetErrorMatchesClassGetErrorWhenClassSets)
    {
        il2cpp::os::Error::SetLastError(il2cpp::os::kErrorWrongDisk);
        CHECK_EQUAL(il2cpp::os::Error::GetLastError(), UnityPalGetLastError());
    }

    TEST(SuccessReturnsOneForSuccessfulErrorCode)
    {
        CHECK_EQUAL(1, UnityPalSuccess(il2cpp::os::kErrorCodeSuccess));
    }

    TEST(SuccessReturnsZeroForUnsuccessfulErrorCode)
    {
        CHECK_EQUAL(0, UnityPalSuccess(il2cpp::os::kErrorWrongDisk));
    }
}

#endif // ENABLE_UNIT_TESTS
