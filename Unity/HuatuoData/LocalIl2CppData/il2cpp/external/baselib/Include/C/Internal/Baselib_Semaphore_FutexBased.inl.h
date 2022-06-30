#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemFutex.h"
#include "../Baselib_Thread.h"

#if !PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "Only use this implementation on top of a proper futex, in all other situations us Baselib_Semaphore_SemaphoreBased.inl.h"
#endif

// Space out to different cache lines.
// the idea here is that threads waking up from sleep should not have to
// access the cache line where count is stored, and only touch wakeups.
// the only exception to that rule is if we hit a timeout.
typedef struct Baselib_Semaphore
{
    int32_t wakeups;
    char _cachelineSpacer0[PLATFORM_CACHE_LINE_SIZE - sizeof(int32_t)];
    int32_t count;
    char _cachelineSpacer2[PLATFORM_CACHE_LINE_SIZE - sizeof(int32_t)];
} Baselib_Semaphore;

BASELIB_STATIC_ASSERT(sizeof(Baselib_Semaphore) == PLATFORM_CACHE_LINE_SIZE * 2, "Baselib_Semaphore (Futex) size should match 2*cacheline size (128bytes)");
BASELIB_STATIC_ASSERT(offsetof(Baselib_Semaphore, wakeups) ==
    (offsetof(Baselib_Semaphore, count) - PLATFORM_CACHE_LINE_SIZE), "Baselib_Semaphore (Futex) wakeups and count shouldnt share cacheline");

BASELIB_INLINE_API Baselib_Semaphore Baselib_Semaphore_Create(void)
{
    Baselib_Semaphore semaphore = {0, {0}, 0, {0}};
    return semaphore;
}

BASELIB_INLINE_API bool Detail_Baselib_Semaphore_ConsumeWakeup(Baselib_Semaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->wakeups);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->wakeups, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

BASELIB_INLINE_API bool Baselib_Semaphore_TryAcquire(Baselib_Semaphore* semaphore)
{
    int32_t previousCount = Baselib_atomic_load_32_relaxed(&semaphore->count);
    while (previousCount > 0)
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&semaphore->count, &previousCount, previousCount - 1))
            return true;
    }
    return false;
}

BASELIB_INLINE_API void Baselib_Semaphore_Acquire(Baselib_Semaphore* semaphore)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return;

    while (!Detail_Baselib_Semaphore_ConsumeWakeup(semaphore))
    {
        Baselib_SystemFutex_Wait(&semaphore->wakeups, 0, UINT32_MAX);
    }
}

BASELIB_INLINE_API bool Baselib_Semaphore_TryTimedAcquire(Baselib_Semaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t previousCount = Baselib_atomic_fetch_add_32_acquire(&semaphore->count, -1);
    if (OPTIMIZER_LIKELY(previousCount > 0))
        return true;

    uint32_t timeLeft = timeoutInMilliseconds;
    const Baselib_CountdownTimer timer = Baselib_CountdownTimer_StartMs(timeoutInMilliseconds);
    do
    {
        Baselib_SystemFutex_Wait(&semaphore->wakeups, 0, timeLeft);
        if (Detail_Baselib_Semaphore_ConsumeWakeup(semaphore))
            return true;
        timeLeft = Baselib_CountdownTimer_GetTimeLeftInMilliseconds(timer);
    }
    while (timeLeft);

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
    while (!Detail_Baselib_Semaphore_ConsumeWakeup(semaphore));
    return true;
}

BASELIB_INLINE_API void Baselib_Semaphore_Release(Baselib_Semaphore* semaphore, const uint16_t _count)
{
    const int32_t count = _count;
    int32_t previousCount = Baselib_atomic_fetch_add_32_release(&semaphore->count, count);

    // This should only be possible if thousands of threads enter this function simultaneously posting with a high count.
    // See overflow protection below.
    BaselibAssert(previousCount <= (previousCount + count), "Semaphore count overflow (current: %d, added: %d).", previousCount, count);

    if (OPTIMIZER_UNLIKELY(previousCount < 0))
    {
        const int32_t waitingThreads = -previousCount;
        const int32_t threadsToWakeup = count < waitingThreads ? count : waitingThreads;
        Baselib_atomic_fetch_add_32_relaxed(&semaphore->wakeups, threadsToWakeup);
        Baselib_SystemFutex_Notify(&semaphore->wakeups, threadsToWakeup, Baselib_WakeupFallbackStrategy_OneByOne);
        return;
    }

    // overflow protection
    // we clamp count to MaxGuaranteedCount when count exceed MaxGuaranteedCount * 2
    // this way we won't have to do clamping on every iteration
    while (OPTIMIZER_UNLIKELY(previousCount > Baselib_Semaphore_MaxGuaranteedCount * 2))
    {
        const int32_t maxCount = Baselib_Semaphore_MaxGuaranteedCount;
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->count, &previousCount, maxCount))
            return;
    }
}

BASELIB_INLINE_API uint32_t Baselib_Semaphore_ResetAndReleaseWaitingThreads(Baselib_Semaphore* semaphore)
{
    const int32_t count = Baselib_atomic_exchange_32_release(&semaphore->count, 0);
    if (OPTIMIZER_LIKELY(count >= 0))
        return 0;
    const int32_t threadsToWakeup = -count;
    Baselib_atomic_fetch_add_32_relaxed(&semaphore->wakeups, threadsToWakeup);
    Baselib_SystemFutex_Notify(&semaphore->wakeups, threadsToWakeup, Baselib_WakeupFallbackStrategy_All);
    return threadsToWakeup;
}

BASELIB_INLINE_API void Baselib_Semaphore_Free(Baselib_Semaphore* semaphore)
{
    if (!semaphore)
        return;
    const int32_t count = Baselib_atomic_load_32_seq_cst(&semaphore->count);
    BaselibAssert(count >= 0, "Destruction is not allowed when there are still threads waiting on the semaphore.");
}
