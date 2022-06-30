#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../ThreadLocalValue-c-api.h"
#include "../../ThreadLocalValue.h"
#include "../../Thread.h"

SUITE(ThreadLocalValue)
{
    static const int TEST_INT_VALUE = 7;

    struct ThreadLocalValueFixture
    {
        ThreadLocalValueFixture()
        {
            localObject = NULL;
            intValue = TEST_INT_VALUE;
            intCopyValue = 0;
            intPtr = &intCopyValue;
            localObject = UnityPalThreadLocalValueNew();
        }

        ~ThreadLocalValueFixture()
        {
            UnityPalThreadLocalValueDelete(localObject);
        }

        UnityPalThreadLocalValue* localObject;
        int intValue;
        int intCopyValue;
        int* intPtr;

        struct ThreadLocalTestData
        {
            UnityPalThreadLocalValue* threadLocalVariable;
            int valueOnAnotherThread;
        };

        static void CheckThreadLocalValueOnAnotherThread(void* arg)
        {
            ThreadLocalTestData* data = static_cast<ThreadLocalTestData*>(arg);
            int* checkingIntValue;
            checkingIntValue = &data->valueOnAnotherThread;
            UnityPalThreadLocalValueGetValue(data->threadLocalVariable, reinterpret_cast<void**>(&checkingIntValue));
        }

        static void SetThreadLocalValueOnAnotherThread(void* arg)
        {
            ThreadLocalTestData* data = static_cast<ThreadLocalTestData*>(arg);
            int settingIntValue = data->valueOnAnotherThread;
            UnityPalThreadLocalValueSetValue(data->threadLocalVariable, &settingIntValue);
        }
    };

    TEST_FIXTURE(ThreadLocalValueFixture, InstantiateNewReturnsValidPointer)
    {
        CHECK_NOT_NULL(localObject);
    }

    TEST_FIXTURE(ThreadLocalValueFixture, SetValueNormalReturnsSuccess)
    {
        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, UnityPalThreadLocalValueSetValue(localObject, &intValue));
    }

    TEST_FIXTURE(ThreadLocalValueFixture, GetValueReturnsSuccess)
    {
        UnityPalThreadLocalValueSetValue(localObject, &intValue);

        CHECK_EQUAL(il2cpp::os::kErrorCodeSuccess, UnityPalThreadLocalValueGetValue(localObject, (void**)&intPtr));
    }

    TEST_FIXTURE(ThreadLocalValueFixture, GetValueMatchedValueUsedInSet)
    {
        UnityPalThreadLocalValueSetValue(localObject, &intValue);
        UnityPalThreadLocalValueGetValue(localObject, (void**)&intPtr);

        CHECK_EQUAL(TEST_INT_VALUE, *intPtr);
    }

    TEST_FIXTURE(ThreadLocalValueFixture, ApiSetValueMatchesClassSetValue)
    {
        CHECK_EQUAL(localObject->SetValue(&intValue), UnityPalThreadLocalValueSetValue(localObject, &intValue));
    }

    TEST_FIXTURE(ThreadLocalValueFixture, ApiGetValueMatchesClassGetValue)
    {
        UnityPalThreadLocalValueSetValue(localObject, &intValue);

        CHECK_EQUAL(localObject->GetValue((void**)&intPtr), UnityPalThreadLocalValueGetValue(localObject, (void**)&intPtr));
    }

    TEST_FIXTURE(ThreadLocalValueFixture, ConfirmValueSetOnThisThreadIsDifferentFromValueOnAnotherThread)
    {
        il2cpp::os::Thread::Init();
        ThreadLocalTestData data;
        data.threadLocalVariable = localObject;
        data.valueOnAnotherThread = 0;

        il2cpp::os::Thread thread;
        thread.Run(CheckThreadLocalValueOnAnotherThread, &data);
        thread.Join();

        CHECK_NOT_EQUAL(TEST_INT_VALUE, data.valueOnAnotherThread);
    }

    TEST_FIXTURE(ThreadLocalValueFixture, ConfirmValueSetOnAnotherThreadIsDifferentFromValueSetOnThisThread)
    {
        UnityPalThreadLocalValueSetValue(localObject, &intValue);

        ThreadLocalTestData data;
        data.threadLocalVariable = localObject;
        data.valueOnAnotherThread = 42;

        il2cpp::os::Thread thread;
        thread.Run(SetThreadLocalValueOnAnotherThread, &data);
        thread.Join();

        UnityPalThreadLocalValueGetValue(localObject, (void**)&intPtr);

        CHECK_EQUAL(TEST_INT_VALUE, *intPtr);
    }
}

#endif // ENABLE_UNIT_TESTS
