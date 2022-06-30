#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"
#include "../Semaphore-c-api.h"
#include "../../Semaphore.h"
#include "../../Event.h"
#include "../../Thread.h"

SUITE(Semaphore)
{
    struct SemaphoreFixture
    {
        SemaphoreFixture()
        {
            il2cpp::os::Thread::Init();
            semaphore = UnityPalSemaphoreNew(0, 1);
            unblockedThreadEvent = new il2cpp::os::Event();
            unblockedThreadCount = 0;
        }

        ~SemaphoreFixture()
        {
            UnityPalSemaphoreDelete(semaphore);
        }

        UnityPalSemaphore* semaphore;
        il2cpp::os::Event* unblockedThreadEvent;
        int unblockedThreadCount;

        static void WaitForSemaphoreAndIncrementCounter(void* arg)
        {
            SemaphoreFixture* data = static_cast<SemaphoreFixture*>(arg);
            UnityPalSemaphoreWait(data->semaphore, false);
            data->unblockedThreadCount++;
            data->unblockedThreadEvent->Set();
        }
    };

    TEST_FIXTURE(SemaphoreFixture, CanCreateASemaphore)
    {
        CHECK_NOT_NULL(semaphore);
    }

    TEST_FIXTURE(SemaphoreFixture, VerifyThatPostingASemaphoreAGivenNumberOfTimesReturnsThePreviousCount)
    {
        int32_t actualPreviousCount = 0;
        UnityPalSemaphorePost(semaphore, 2, &actualPreviousCount);
        CHECK_EQUAL(0, actualPreviousCount);
    }

    TEST_FIXTURE(SemaphoreFixture, VerifyThatPostWithMultipleThreadsWaitingUnblocksOnlyOneThread)
    {
        // Create two threads which each increment the counter once.
        il2cpp::os::Thread thread1;
        thread1.Run(WaitForSemaphoreAndIncrementCounter, this);

        il2cpp::os::Thread thread2;
        thread2.Run(WaitForSemaphoreAndIncrementCounter, this);

        UnityPalSemaphorePost(semaphore, 1, NULL);

        // Let one thread increment and wait for it.
        unblockedThreadEvent->Wait();

        CHECK_EQUAL(1, unblockedThreadCount);

        // Let the other thread increment and wait for it so we can exit the test cleanly.
        UnityPalSemaphorePost(semaphore, 1, NULL);
        unblockedThreadEvent->Wait();

        thread1.Join();
        thread2.Join();
    }

    struct SemaphoreHandleFixture
    {
        SemaphoreHandleFixture()
        {
            il2cpp::os::Thread::Init();
            semaphore = UnityPalSemaphoreNew(0, 1);
            semaphoreHandle = UnityPalSemaphoreHandleNew(semaphore);
            unblockedThreadEvent = new il2cpp::os::Event();
            unblockedThreadCount = 0;
        }

        ~SemaphoreHandleFixture()
        {
            UnityPalSemaphoreHandleDelete(semaphoreHandle);
        }

        UnityPalSemaphore* semaphore;
        UnityPalSemaphoreHandle* semaphoreHandle;
        il2cpp::os::Event* unblockedThreadEvent;
        int unblockedThreadCount;

        static void WaitForSemaphoreAndIncrementCounter(void* arg)
        {
            SemaphoreHandleFixture* data = static_cast<SemaphoreHandleFixture*>(arg);
            UnityPalSemaphoreHandleWait(data->semaphoreHandle);
            data->unblockedThreadCount++;
            data->unblockedThreadEvent->Set();
        }
    };

    TEST_FIXTURE(SemaphoreHandleFixture, CanCreateASemaphoreHandle)
    {
        CHECK_NOT_NULL(semaphoreHandle);
    }

    TEST_FIXTURE(SemaphoreHandleFixture, VerifyThatPostingASemaphoreHandleAGivenNumberOfTimesReturnsThePreviousCount)
    {
        int32_t actualPreviousCount = 0;
        UnityPalSemaphoreHandleSignal(semaphoreHandle);
        CHECK_EQUAL(0, actualPreviousCount);
    }

    TEST_FIXTURE(SemaphoreHandleFixture, VerifyThatPostHandleWithMultipleThreadsWaitingUnblocksOnlyOneThread)
    {
        // Create two threads which each increment the counter once.
        il2cpp::os::Thread thread1;
        thread1.Run(WaitForSemaphoreAndIncrementCounter, this);

        il2cpp::os::Thread thread2;
        thread2.Run(WaitForSemaphoreAndIncrementCounter, this);

        UnityPalSemaphoreHandleSignal(semaphoreHandle);

        // Let one thread increment and wait for it.
        unblockedThreadEvent->Wait();

        CHECK_EQUAL(1, unblockedThreadCount);

        // Let the other thread increment and wait for it so we can exit the test cleanly.
        UnityPalSemaphoreHandleSignal(semaphoreHandle);
        unblockedThreadEvent->Wait();

        thread1.Join();
        thread2.Join();
    }
}

#endif // ENABLE_UNIT_TESTS
