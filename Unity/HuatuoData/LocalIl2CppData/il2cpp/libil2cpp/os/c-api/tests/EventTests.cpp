#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"
#include "il2cpp-config.h"
#include "../Event-c-api.h"
#include "../../Event.h"
#include "../../Thread.h"

SUITE(Event)
{
    struct EventFixture
    {
        EventFixture()
        {
            il2cpp::os::Thread::Init();
            waitableEvent = UnityPalEventNew(false, false);
            processedEvent = UnityPalEventNew(false, false);
            counter = 0;
        }

        ~EventFixture()
        {
            UnityPalEventDelete(waitableEvent);
            UnityPalEventDelete(processedEvent);
        }

        UnityPalEvent* waitableEvent;
        UnityPalEvent* processedEvent;
        int counter;

        static void WaitForEventAndIncrementCounter(void* arg)
        {
            EventFixture* data = static_cast<EventFixture*>(arg);
            UnityPalEventWait(data->waitableEvent, false);
            data->counter++;
            UnityPalEventSet(data->processedEvent);
        }
    };

    TEST_FIXTURE(EventFixture, CanCreateAnEvent)
    {
        CHECK_NOT_NULL(waitableEvent);
    }

    TEST_FIXTURE(EventFixture, EventWaitsForSignal)
    {
        // Create two threads which each increment the counter once.
        il2cpp::os::Thread thread1;
        thread1.Run(WaitForEventAndIncrementCounter, this);

        il2cpp::os::Thread thread2;
        thread2.Run(WaitForEventAndIncrementCounter, this);

        // Let one thread increment and wait for it.
        waitableEvent->Set();
        processedEvent->Wait();

        // Let the other thread increment and wait for it.
        waitableEvent->Set();
        processedEvent->Wait();

        thread1.Join();
        thread2.Join();

        CHECK_EQUAL(2, counter);
    }

    struct EventHandleFixture
    {
        EventHandleFixture()
        {
            il2cpp::os::Thread::Init();
            waitableEvent = UnityPalEventNew(false, false);
            processedEvent = UnityPalEventNew(false, false);
            waitableEventHandle = UnityPalEventHandleNew(waitableEvent);
            processedEventHandle = UnityPalEventHandleNew(processedEvent);
            counter = 0;
        }

        ~EventHandleFixture()
        {
            UnityPalEventHandleDelete(waitableEventHandle);
            UnityPalEventHandleDelete(processedEventHandle);
        }

        UnityPalEvent* waitableEvent;
        UnityPalEvent* processedEvent;
        UnityPalEventHandle* waitableEventHandle;
        UnityPalEventHandle* processedEventHandle;
        int counter;

        static void WaitForEventAndIncrementCounter(void* arg)
        {
            EventHandleFixture* data = static_cast<EventHandleFixture*>(arg);
            UnityPalEventHandleWait(data->waitableEventHandle);
            data->counter++;
            UnityPalEventHandleSignal(data->processedEventHandle);
        }
    };

    TEST_FIXTURE(EventHandleFixture, CanCreateAnEventHandle)
    {
        CHECK_NOT_NULL(waitableEventHandle);
    }

    TEST_FIXTURE(EventHandleFixture, EventHandleWaitsForSignal)
    {
        // Create two threads which each increment the counter once.
        il2cpp::os::Thread thread1;
        thread1.Run(WaitForEventAndIncrementCounter, this);

        il2cpp::os::Thread thread2;
        thread2.Run(WaitForEventAndIncrementCounter, this);

        // Let one thread increment and wait for it.
        waitableEvent->Set();
        processedEvent->Wait();

        // Let the other thread increment and wait for it.
        waitableEvent->Set();
        processedEvent->Wait();

        thread1.Join();
        thread2.Join();

        CHECK_EQUAL(2, counter);
    }
}

#endif // ENABLE_UNIT_TESTS
