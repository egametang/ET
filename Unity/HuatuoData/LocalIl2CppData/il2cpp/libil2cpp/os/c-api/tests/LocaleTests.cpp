#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Locale-c-api.h"
#include "../../Locale.h"

struct LocaleFixture
{
    LocaleFixture()
    {
        UnityPalLocaleInitialize();
        localeResult = UnityPalGetLocale();
    }

    ~LocaleFixture()
    {
        UnityPalLocaleUnInitialize();
    }

    char* localeResult;
};

SUITE(Locale)
{
    TEST_FIXTURE(LocaleFixture, GetLocaleIsValidPointer)
    {
        CHECK_NOT_NULL(localeResult);
    }

    TEST_FIXTURE(LocaleFixture, ApiGetLocaleTestMatchesClassGetLocale)
    {
        CHECK_EQUAL(il2cpp::os::Locale::GetLocale().c_str(), UnityPalGetLocale());
    }

    TEST(ApiGetLocaleWithNoInitializeMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::Locale::GetLocale().c_str(), UnityPalGetLocale());
    }
}

#endif // ENABLE_UNIT_TESTS
