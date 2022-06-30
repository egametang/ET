#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"
#include "os/c-api/il2cpp-config-platforms.h"
#include "../Mutex-c-api.h"
#include "../../Mutex.h"
#include "../../Thread.h"

SUITE(Mutex)
{
    struct MutexFixture
    {
        MutexFixture()
        {
            il2cpp::os::Thread::Init();
            mutex = UnityPalMutexNew(true);
        }

        ~MutexFixture()
        {
            UnityPalMutexDelete(mutex);
        }

        UnityPalMutex* mutex;
    };

    TEST(MutexNew_ReturnsNonNullMutex)
    {
        il2cpp::os::Thread::Init();
        UnityPalMutex* mutex = UnityPalMutexNew(true);
        CHECK_NOT_NULL(mutex);
        UnityPalMutexDelete(mutex);
    }

    TEST_FIXTURE(MutexFixture, MutexTryLock)
    {
        CHECK(UnityPalMutexTryLock(mutex, 0, true));
        UnityPalMutexUnlock(mutex);
    }

    struct MutextTryLockData
    {
        UnityPalMutex* mutex;
        bool locked;
    };

    static void TryLockMutexOnAnotherThread(void* arg)
    {
        MutextTryLockData* data = static_cast<MutextTryLockData*>(arg);
        data->locked = UnityPalMutexTryLock(data->mutex, 0, true);
    }

    TEST_FIXTURE(MutexFixture, MutexTryLockFailure)
    {
        UnityPalMutexLock(mutex, true);

        MutextTryLockData result;
        result.mutex = mutex;
        result.locked = true;

        il2cpp::os::Thread thread;
        thread.Run(TryLockMutexOnAnotherThread, &result);
        thread.Join();

        CHECK(!result.locked);
        UnityPalMutexUnlock(mutex);
    }

    TEST_FIXTURE(MutexFixture, MutexTryLockCompareToCppAPI)
    {
        bool lockedCAPI = UnityPalMutexTryLock(mutex, 0, true);
        UnityPalMutexUnlock(mutex);

        bool lockedCPPAPI = mutex->TryLock(0, true);
        mutex->Unlock();

        CHECK_EQUAL(lockedCPPAPI, lockedCAPI);
    }

    struct MutexHandleFixture
    {
        MutexHandleFixture()
        {
            il2cpp::os::Thread::Init();
            handle = UnityPalMutexHandleNew(UnityPalMutexNew(true));
        }

        ~MutexHandleFixture()
        {
            UnityPalMutexHandleDelete(handle);
        }

        UnityPalMutexHandle* handle;
    };

    TEST_FIXTURE(MutexHandleFixture, MutexHandleNewDelete)
    {
        CHECK_NOT_NULL(handle);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleWaitSignal)
    {
        bool locked = UnityPalMutexHandleWait(handle);
        CHECK_EQUAL(true, locked);
        if (locked)
            UnityPalMutexHandleSignal(handle);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleWaitSignalCompareToCppAPI)
    {
        bool lockedCAPI = UnityPalMutexHandleWait(handle);
        if (lockedCAPI)
            UnityPalMutexHandleSignal(handle);

        bool lockedCPPAPI = handle->Wait();
        if (lockedCPPAPI)
            handle->Signal();
        CHECK_EQUAL(lockedCPPAPI, lockedCAPI);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleWaitSignalMs)
    {
        bool locked = UnityPalMutexHandleWaitMs(handle, 0);
        CHECK_EQUAL(true, locked);
        if (locked)
            UnityPalMutexHandleSignal(handle);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleWaitSignalMsCompareToCppAPI)
    {
        bool lockedCAPI = UnityPalMutexHandleWaitMs(handle, 0);
        if (lockedCAPI)
            UnityPalMutexHandleSignal(handle);

        bool lockedCPPAPI = handle->Wait(0U);
        if (lockedCPPAPI)
            handle->Signal();
        CHECK_EQUAL(lockedCPPAPI, lockedCAPI);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleGet)
    {
        UnityPalMutex* mutex = UnityPalMutexHandleGet(handle);
        CHECK_NOT_NULL(mutex);
    }

    TEST_FIXTURE(MutexHandleFixture, MutexHandleGetCompareToCppAPI)
    {
        UnityPalMutex* mutexCAPI = UnityPalMutexHandleGet(handle);
        UnityPalMutex* mutexCPPAPI = handle->Get();
        CHECK_EQUAL(mutexCPPAPI, mutexCAPI);
    }

    struct FastMutexFixture
    {
        FastMutexFixture()
        {
            il2cpp::os::Thread::Init();
            fastMutex = UnityPalFastMutexNew();
        }

        ~FastMutexFixture()
        {
            UnityPalFastMutexDelete(fastMutex);
        }

        UnityPalFastMutex* fastMutex;
    };

    TEST_FIXTURE(FastMutexFixture, FastMutexNewDelete)
    {
        CHECK_NOT_NULL(fastMutex);
    }

    TEST_FIXTURE(FastMutexFixture, FastMutexLockUnlock)
    {
        UnityPalFastMutexLock(fastMutex);
        UnityPalFastMutexUnlock(fastMutex);
    }

    TEST_FIXTURE(FastMutexFixture, FastMutexGetImpl)
    {
        UnityPalFastMutexImpl* impl = UnityPalFastMutexGetImpl(fastMutex);
        CHECK_NOT_NULL(impl);
    }

    TEST_FIXTURE(FastMutexFixture, FastMutexGetImplCompareToCppAPI)
    {
        UnityPalFastMutexImpl* implCAPI = UnityPalFastMutexGetImpl(fastMutex);
        UnityPalFastMutexImpl* implCPPAPI = fastMutex->GetImpl();
        CHECK_EQUAL(implCPPAPI, implCAPI);
    }
}

#endif // ENABLE_UNIT_TESTS
