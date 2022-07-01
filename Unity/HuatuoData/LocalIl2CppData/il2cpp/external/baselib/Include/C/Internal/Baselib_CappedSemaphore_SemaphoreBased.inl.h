#pragma once

#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemSemaphore.h"
#include "../Baselib_Thread.h"

#if PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "It's highly recommended to use Baselib_CappedSemaphore_FutexBased.inl.h on platforms which has native semaphore support"
#endif

typedef struct Baselib_CappedSemaphore
{
    int32_t count;
    const Baselib_SystemSemaphore_Handle handle;
    const int32_t cap;
    // Make the capped semaphore take a full cache line so that if the user cacheline aligned semaphore,
    // llsc operations on count will not spuriously fail.
    char _cachelineSpacer[PLATFORM_CACHE_LINE_SIZE - sizeof(int32_t) * 2 - sizeof(Baselib_SystemSemaphore_Handle)];
} Baselib_CappedSemaphore;

BASELIB_INLINE_API Baselib_CappedSemaphore Baselib_CappedSemaphore_Create(uint16_t cap)
{
    Baselib_CappedSemaphore semaphore = {0, Baselib_SystemSemaphore_Create(), cap, {0}};
    return semaphore;
}

BASELIB_INLINE_API void Baselib_CappedSemaphore_Acquire(Baselib_CappedSemaphore* semaphore)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return;

    Baselib_SystemSemaphore_Acquire(semaphore->handle);
}

BASELIB_INLINE_API bool Baselib_CappedSemaphore_TryAcquire(Baselib_CappedSemaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&semaphore->count, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

BASELIB_INLINE_API bool Baselib_CappedSemaphore_TryTimedAcquire(Baselib_CappedSemaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
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
        int32_t count = Baselib_atomic_load_32_relaxed(&semaphore->count);
        while (count < 0)
        {
            if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->count, &count, count + 1))
                return false;
        }
        // Likely a race, yield to give the release operation room to complete.
        // This includes a fully memory barrier which ensures that there is no reordering between changing/reading count and wakeup consumption.
        Baselib_Thread_YieldExecution();
    }
    while (!Baselib_SystemSemaphore_TryAcquire(semaphore->handle));
    return true;
}

BASELIB_INLINE_API uint16_t Baselib_CappedSemaphore_Release(Baselib_CappedSemaphore* semaphore, const uint16_t _count)
{
    int32_t count = _count;
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    do
    {
        if (previousCount == semaphore->cap)
            return 0;

        if (previousCount + count > semaphore->cap)
            count = semaphore->cap - previousCount;
    }
    while (!Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->count, &previousCount, previousCount + count));

    if (OPTIMIZER_UNLIKELY(previousCount < 0))
    {
        const int32_t waitingThreads = -previousCount;
        const int32_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
        Baselib_SystemSemaphore_Release(semaphore->handle, threadsToWakeup);
    }
    return count;
}

BASELIB_INLINE_API uint32_t Baselib_CappedSemaphore_ResetAndReleaseWaitingThreads(Baselib_CappedSemaphore* semaphore)
{
    const int32_t count = Baselib_atomic_exchange_32_release(&semaphore->count, 0);
    if (OPTIMIZER_LIKELY(count >= 0))
        return 0;
    const int32_t threadsToWakeup = -count;
    Baselib_SystemSemaphore_Release(semaphore->handle, threadsToWakeup);
    return threadsToWakeup;
}

BASELIB_INLINE_API void Baselib_CappedSemaphore_Free(Baselib_CappedSemaphore* semaphore)
{
    if (!semaphore)
        return;
    const int32_t count = Baselib_atomic_load_32_seq_cst(&semaphore->count);
    BaselibAssert(count >= 0, "Destruction is not allowed when there are still threads waiting on the semaphore.");
    Baselib_SystemSemaphore_Free(semaphore->handle);
}
