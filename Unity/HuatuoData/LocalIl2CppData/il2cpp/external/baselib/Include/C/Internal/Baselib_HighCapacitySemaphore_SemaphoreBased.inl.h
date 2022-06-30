#pragma once

#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemSemaphore.h"
#include "../Baselib_Thread.h"

#if PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "It's highly recommended to use Baselib_HighCapacitySemaphore_FutexBased.inl.h on platforms which has native semaphore support"
#endif

typedef struct Baselib_HighCapacitySemaphore
{
    int64_t count;
    Baselib_SystemSemaphore_Handle handle;
    char _cachelineSpacer0[PLATFORM_CACHE_LINE_SIZE - sizeof(int64_t) - sizeof(Baselib_SystemSemaphore_Handle)];
    char _systemSemaphoreData[Baselib_SystemSemaphore_PlatformSize];
} Baselib_HighCapacitySemaphore;

BASELIB_STATIC_ASSERT((offsetof(Baselib_HighCapacitySemaphore, count) + PLATFORM_CACHE_LINE_SIZE) ==
    offsetof(Baselib_HighCapacitySemaphore, _systemSemaphoreData), "count and internalData must not share cacheline");

BASELIB_INLINE_API Baselib_HighCapacitySemaphore Baselib_HighCapacitySemaphore_Create(void)
{
    Baselib_HighCapacitySemaphore semaphore = {0, {0}, {0}, {0}};
    semaphore.handle = Baselib_SystemSemaphore_CreateInplace(&semaphore._systemSemaphoreData);
    return semaphore;
}

BASELIB_INLINE_API bool Baselib_HighCapacitySemaphore_TryAcquire(Baselib_HighCapacitySemaphore* semaphore)
{
    int64_t previousCount = Baselib_atomic_load_64_relaxed(&semaphore->count);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_64_acquire_relaxed(&semaphore->count, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Acquire(Baselib_HighCapacitySemaphore* semaphore)
{
    const int64_t previousCount = Baselib_atomic_fetch_add_64_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return;

    Baselib_SystemSemaphore_Acquire(semaphore->handle);
}

BASELIB_INLINE_API bool Baselib_HighCapacitySemaphore_TryTimedAcquire(Baselib_HighCapacitySemaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int64_t previousCount = Baselib_atomic_fetch_add_64_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return true;

    if (OPTIMIZER_LIKELY(Baselib_SystemSemaphore_TryTimedAcquire(semaphore->handle, timeoutInMilliseconds)))
        return true;

    // When timeout occurs we need to make sure we do one of the following:
    // Increase count by one from a negative value (give our acquired token back) or consume a wakeup.
    //
    // If count is not negative it's likely we are racing with a release operation in which case we
    // may end up having a successful acquire operation.
    do
    {
        int64_t count = Baselib_atomic_load_64_relaxed(&semaphore->count);
        while (count < 0)
        {
            if (Baselib_atomic_compare_exchange_weak_64_relaxed_relaxed(&semaphore->count, &count, count + 1))
                return false;
        }
        // Likely a race, yield to give the release operation room to complete.
        // This includes a fully memory barrier which ensures that there is no reordering between changing/reading count and wakeup consumption.
        Baselib_Thread_YieldExecution();
    }
    while (!Baselib_SystemSemaphore_TryAcquire(semaphore->handle));
    return true;
}

BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Release(Baselib_HighCapacitySemaphore* semaphore, const uint32_t _count)
{
    const int64_t count = _count;
    int64_t previousCount = Baselib_atomic_fetch_add_64_release(&semaphore->count, count);

    // This should only be possible if millions of threads enter this function simultaneously posting with a high count.
    // See overflow protection below.
    BaselibAssert(previousCount <= (previousCount + count), "Semaphore count overflow (current: %d, added: %d).", (int32_t)previousCount, (int32_t)count);

    if (OPTIMIZER_UNLIKELY(previousCount < 0))
    {
        const int64_t waitingThreads = -previousCount;
        const int64_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
        BaselibAssert(threadsToWakeup <= (int64_t)UINT32_MAX);
        Baselib_SystemSemaphore_Release(semaphore->handle, (uint32_t)threadsToWakeup);
        return;
    }

    // overflow protection
    // we clamp count to MaxGuaranteedCount when count exceed MaxGuaranteedCount * 2
    // this way we won't have to do clamping on every iteration
    while (OPTIMIZER_UNLIKELY(previousCount > Baselib_HighCapacitySemaphore_MaxGuaranteedCount * 2))
    {
        const int64_t maxCount = Baselib_HighCapacitySemaphore_MaxGuaranteedCount;
        if (Baselib_atomic_compare_exchange_weak_64_relaxed_relaxed(&semaphore->count, &previousCount, maxCount))
            return;
    }
}

BASELIB_INLINE_API uint64_t Baselib_HighCapacitySemaphore_ResetAndReleaseWaitingThreads(Baselib_HighCapacitySemaphore* semaphore)
{
    const int64_t count = Baselib_atomic_exchange_64_release(&semaphore->count, 0);
    if (OPTIMIZER_LIKELY(count >= 0))
        return 0;
    const int64_t threadsToWakeup = -count;
    BaselibAssert(threadsToWakeup <= (int64_t)UINT32_MAX);
    Baselib_SystemSemaphore_Release(semaphore->handle, (uint32_t)threadsToWakeup);
    return threadsToWakeup;
}

BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Free(Baselib_HighCapacitySemaphore* semaphore)
{
    if (!semaphore)
        return;
    const int64_t count = Baselib_atomic_load_64_seq_cst(&semaphore->count);
    BaselibAssert(count >= 0, "Destruction is not allowed when there are still threads waiting on the semaphore.");
    Baselib_SystemSemaphore_FreeInplace(semaphore->handle);
}
