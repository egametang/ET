#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Cryptography-c-api.h"
#include "../../Cryptography.h"

static const uint32_t BUFFERSIZE = 16;

struct CryptoFixture
{
    CryptoFixture()
    {
        il2cpp::os::Cryptography::OpenCryptographyProvider();
        apiCryptoProvider = UnityPalGetCryptographyProvider();
        classCryptoProvider = il2cpp::os::Cryptography::GetCryptographyProvider();
    }

    ~CryptoFixture()
    {
        UnityPalReleaseCryptographyProvider(apiCryptoProvider);
        il2cpp::os::Cryptography::ReleaseCryptographyProvider(classCryptoProvider);
    }

    void* apiCryptoProvider;
    void* classCryptoProvider;
    unsigned char data[BUFFERSIZE];
};

SUITE(Cryptography)
{
    TEST_FIXTURE(CryptoFixture, GetCryptographyProviderHasValidPointer)
    {
        CHECK_NOT_NULL(apiCryptoProvider);
    }

    TEST_FIXTURE(CryptoFixture, ApiHasValidPointers)
    {
        CHECK_NOT_NULL(apiCryptoProvider);
    }

    TEST_FIXTURE(CryptoFixture, ClassHasValidPointers)
    {
        CHECK_NOT_NULL(classCryptoProvider);
    }

    TEST_FIXTURE(CryptoFixture, FillRandomBytesReturnsSuccess)
    {
        CHECK(UnityPalCryptographyFillBufferWithRandomBytes(apiCryptoProvider, BUFFERSIZE, data));
    }

    TEST_FIXTURE(CryptoFixture, ApiFillRandomBytesReturnMatchesClassReturn)
    {
        CHECK_EQUAL((int32_t)il2cpp::os::Cryptography::FillBufferWithRandomBytes(classCryptoProvider, BUFFERSIZE, data), UnityPalCryptographyFillBufferWithRandomBytes(apiCryptoProvider, BUFFERSIZE, data));
    }

    TEST(FillRandomBytesWillBadProviderFails)
    {
        void* badCryptoProvider = NULL;
        unsigned char* badBuffer = NULL;

        CHECK(!UnityPalCryptographyFillBufferWithRandomBytes(badCryptoProvider, BUFFERSIZE, badBuffer));
    }
}

#endif // ENABLE_UNIT_TESTS
