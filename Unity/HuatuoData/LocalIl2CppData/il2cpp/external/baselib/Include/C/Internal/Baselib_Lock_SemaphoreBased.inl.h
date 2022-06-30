#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_CappedSemaphore.h"

typedef struct Baselib_Lock
{
    Baselib_CappedSemaphore semaphore;
} Baselib_Lock;

BASELIB_INLINE_API Baselib_Lock Baselib_Lock_Create(void)
{
    Baselib_Lock lock = { Baselib_CappedSemaphore_Create(1) };
    uint16_t submittedTokens = Baselib_CappedSemaphore_Release(&lock.semaphore, 1);
    BaselibAssert(submittedTokens == 1, "CappedSemaphore was unable to accept our token");
    return lock;
}

BASELIB_INLINE_API void Baselib_Lock_Acquire(Baselib_Lock* lock)
{
    Baselib_CappedSemaphore_Acquire(&lock->semaphore);
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_Lock_TryAcquire(Baselib_Lock* lock)
{
    return Baselib_CappedSemaphore_TryAcquire(&lock->semaphore);
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_Lock_TryTimedAcquire(Baselib_Lock* lock, const uint32_t timeoutInMilliseconds)
{
    return Baselib_CappedSemaphore_TryTimedAcquire(&lock->semaphore, timeoutInMilliseconds);
}

BASELIB_INLINE_API void Baselib_Lock_Release(Baselib_Lock* lock)
{
    Baselib_CappedSemaphore_Release(&lock->semaphore, 1);
}

BASELIB_INLINE_API void Baselib_Lock_Free(Baselib_Lock* lock)
{
    if (!lock)
        return;
    Baselib_CappedSemaphore_Free(&lock->semaphore);
}
