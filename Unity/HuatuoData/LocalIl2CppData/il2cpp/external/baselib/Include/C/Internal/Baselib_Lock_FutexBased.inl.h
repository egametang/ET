#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemFutex.h"

enum Detail_Baselib_Lock_State
{
    Detail_Baselib_Lock_UNLOCKED    = 0,
    Detail_Baselib_Lock_LOCKED      = 1,
    Detail_Baselib_Lock_CONTENDED   = 2,
};
typedef struct Baselib_Lock
{
    int32_t state;
    char _cachelineSpacer[PLATFORM_CACHE_LINE_SIZE - sizeof(int32_t)];
} Baselib_Lock;

BASELIB_INLINE_API Baselib_Lock Baselib_Lock_Create(void)
{
    Baselib_Lock lock = {Detail_Baselib_Lock_UNLOCKED, {0}};
    return lock;
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_Lock_TryAcquire(Baselib_Lock* lock)
{
    int32_t previousState = Detail_Baselib_Lock_UNLOCKED;
    do
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&lock->state, &previousState, Detail_Baselib_Lock_LOCKED))
            return true;
    }
    while (previousState == Detail_Baselib_Lock_UNLOCKED);
    return false;
}

BASELIB_INLINE_API void Baselib_Lock_Acquire(Baselib_Lock* lock)
{
    int32_t previousState = Detail_Baselib_Lock_UNLOCKED;
    do
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&lock->state, &previousState, previousState + 1))
            break;
    }
    while (previousState != Detail_Baselib_Lock_CONTENDED);

    while (OPTIMIZER_LIKELY(previousState != Detail_Baselib_Lock_UNLOCKED))
    {
        Baselib_SystemFutex_Wait(&lock->state, Detail_Baselib_Lock_CONTENDED, UINT32_MAX);
        previousState = Baselib_atomic_exchange_32_relaxed(&lock->state, Detail_Baselib_Lock_CONTENDED);
    }
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_Lock_TryTimedAcquire(Baselib_Lock* lock, const uint32_t timeoutInMilliseconds)
{
    int32_t previousState = Detail_Baselib_Lock_UNLOCKED;
    do
    {
        if (Baselib_atomic_compare_exchange_weak_32_acquire_relaxed(&lock->state, &previousState, previousState + 1))
            break;
    }
    while (previousState != Detail_Baselib_Lock_CONTENDED);

    if (OPTIMIZER_LIKELY(previousState == Detail_Baselib_Lock_UNLOCKED))
        return true;

    uint32_t timeLeft = timeoutInMilliseconds;
    const Baselib_CountdownTimer timer = Baselib_CountdownTimer_StartMs(timeoutInMilliseconds);
    do
    {
        Baselib_SystemFutex_Wait(&lock->state, Detail_Baselib_Lock_CONTENDED, timeoutInMilliseconds);
        const int32_t previousState = Baselib_atomic_exchange_32_relaxed(&lock->state, Detail_Baselib_Lock_CONTENDED);
        if (previousState == Detail_Baselib_Lock_UNLOCKED)
            return true;
        timeLeft = Baselib_CountdownTimer_GetTimeLeftInMilliseconds(timer);
    }
    while (timeLeft);
    return false;
}

BASELIB_INLINE_API void Baselib_Lock_Release(Baselib_Lock* lock)
{
    const int32_t previousState = Baselib_atomic_exchange_32_release(&lock->state, Detail_Baselib_Lock_UNLOCKED);
    if (previousState == Detail_Baselib_Lock_CONTENDED)
        Baselib_SystemFutex_Notify(&lock->state, 1, Baselib_WakeupFallbackStrategy_OneByOne);
}

BASELIB_INLINE_API void Baselib_Lock_Free(Baselib_Lock* lock)
{
}
