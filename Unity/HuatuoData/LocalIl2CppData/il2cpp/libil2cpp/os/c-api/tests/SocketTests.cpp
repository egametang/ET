#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"
#include <string>
#include <cstring>
#include <vector>

#include "../../Socket.h"
#include "../../../utils/Memory.h"
#include "../Socket-c-api.h"

SUITE(Socket)
{
    const char* hostname = "";

    struct GetHostByNameFixture
    {
        std::string className;
        int32_t classFamily;
        std::vector<std::string> classAliases;
        std::vector<void*> classAddressList;
        int32_t classAddressSize;

        char* palName;
        int32_t palFamily;
        char** palAliases;
        void** palAddresses;
        int32_t palAddresseSize;

        GetHostByNameFixture()
        {
            palName = NULL;
            palFamily = -1;
            palAliases = NULL;
            palAddresses = NULL;
            palAddresseSize = -1;
        }

        ~GetHostByNameFixture()
        {
            il2cpp::utils::Memory::Free(palName);
            FreeNullTerminatedArray((void**)palAliases);
            FreeNullTerminatedArray((void**)palAddresses);
        }

        int NullTerminatedArrayLength(void** array)
        {
            int i = 0;
            if (array != NULL)
            {
                while (array[i] != NULL)
                    i++;
            }

            return i;
        }

        void FreeNullTerminatedArray(void** array)
        {
            if (array != NULL)
            {
                int i = 0;
                while (array[i] != NULL)
                {
                    free(array[i]);
                    i++;
                }
            }
            free(array);
        }
    };

    TEST_FIXTURE(GetHostByNameFixture, ReturnsSuccessForEmptyHostname)
    {
        CHECK(UnityPalGetHostByName(hostname, NULL, NULL, NULL, NULL, NULL) == kWaitStatusSuccess);
    }

    TEST_FIXTURE(GetHostByNameFixture, ReturnsTheSameValueAsClass)
    {
        CHECK(UnityPalGetHostByName(hostname, NULL, NULL, NULL, NULL, NULL) == il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize));
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameNameAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, NULL, NULL, NULL, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        CHECK(className == palName);
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameFamilyAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, NULL, NULL, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        CHECK(classFamily == palFamily);
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameNumberOfAliasesAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, &palAliases, NULL, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        CHECK(classAliases.size() == NullTerminatedArrayLength((void**)palAliases));
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameAliasesAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, &palAliases, NULL, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        for (size_t i = 0; i < classAliases.size(); ++i)
            CHECK(classAliases[i] == palAliases[i]);
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameAddressSizeAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, &palAliases, &palAddresses, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        CHECK(classAddressList.size() == NullTerminatedArrayLength((void**)palAddresses));
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameNumberOfAddressesAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, &palAliases, &palAddresses, &palAddresseSize);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        CHECK(classAddressSize == palAddresseSize);
    }

    TEST_FIXTURE(GetHostByNameFixture, ReportsTheSameAddressesAsClass)
    {
        UnityPalGetHostByName(hostname, &palName, &palFamily, &palAliases, &palAddresses, NULL);
        il2cpp::os::Socket::GetHostByName(hostname, className, classFamily, classAliases, classAddressList, classAddressSize);
        for (size_t i = 0; i < classAddressList.size(); ++i)
            CHECK(std::memcmp(classAddressList[i], palAddresses[i], classAddressSize) == 0);
    }
}

#endif // ENABLE_UNIT_TESTS
