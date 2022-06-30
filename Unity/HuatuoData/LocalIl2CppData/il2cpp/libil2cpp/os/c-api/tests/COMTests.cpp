#if ENABLE_UNIT_TESTS
#if IL2CPP_TARGET_WINDOWS

#include "UnitTest++.h"

#include "../COM-c-api.h"
#include "../../COM.h"

SUITE(Com)
{
    struct ComFixture
    {
        ComFixture()
        {
            int dimCount = 1;
            int elemSize = 2;
            safeBound.element_count = 123;
            safeBound.lower_bound = 0;
            safeArray = UnityPalCOMSafeArrayCreate(IL2CPP_VT_BOOL, dimCount, &safeBound);
            UnityPalCOMVariantInit(&classVariant);
            UnityPalCOMVariantInit(&classVariant);
        }

        ~ComFixture()
        {
            UnityPalCOMSafeArrayDestroy(safeArray);
        }

        Il2CppSafeArrayBound safeBound;
        UnityPalIl2CppSafeArray* safeArray;
        Il2CppVariant apiVariant;
        Il2CppVariant classVariant;
    };

    TEST_FIXTURE(ComFixture, SafeArrayCreateNotNull)
    {
        CHECK_NOT_NULL(safeArray);
    }

    TEST_FIXTURE(ComFixture, SafeArrayCreateDimensionsValid)
    {
        CHECK_EQUAL(dimCount, safeArray->dimension_count);
    }

    TEST_FIXTURE(ComFixture, SafeArrayCreateBoundsElementCountIsValid)
    {
        CHECK_EQUAL(safeBound.element_count, safeArray->bounds->element_count);
    }

    TEST(SafeArrayCreateBoundsElementSizeValid)
    {
        CHECK_EQUAL(elemSize, safeArray->element_size);
    }

    TEST(SafeArrayDestroyReturnsSuccess)
    {
        UnityPalIl2CppSafeArray* tempArray;
        tempArray = il2cpp::os::COM::SafeArrayCreate(IL2CPP_VT_BOOL, dimCount, &safeBound);
        CHECK_EQUAL(IL2CPP_S_OK, UnityPalCOMSafeArrayDestroy(tempArray));
    }

    TEST(VariantClearReturnsSuccess)
    {
        CHECK_EQUAL(IL2CPP_S_OK, UnityPalCOMVariantClear(&apiVariant));
    }

    TEST(ApiVariantClearMatchesClassResult)
    {
        CHECK_EQUAL(il2cpp::os::COM::VariantClear(&classVariant), UnityPalCOMVariantClear(&apiVariant));
    }
}

#endif // IL2CPP_TARGET_WINDOWS
#endif // ENABLE_UNIT_TESTS
