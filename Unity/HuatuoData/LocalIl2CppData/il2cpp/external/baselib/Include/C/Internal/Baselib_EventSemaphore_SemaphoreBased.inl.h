#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemSemaphore.h"

#if PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "It's highly recommended to use Baselib_EventSemaphore_FutexBased.inl.h on platforms which has native semaphore support"
#endif

typedef union BASELIB_ALIGN_AS (8) Detail_Baselib_EventSemaphore_State
{
    struct
    {
        // Can be changed without checking for changes in numWaitingForSetInProgress (use 32bit cmpex)
        int32_t numWaitingForSetAndStateFlags;
        // Typically not changed without checking numWaitingForSetAndStateFlags (use 64bit cmpex)
        int32_t numWaitingForSetInProgress;
    } parts;
    int64_t stateInt64;
} Detail_Baselib_EventSemaphore_State;

enum
{
    // If this flag is set, threads are still waking up from a previous Set or ResetAndReleaseWaitingThreads call.
    // While this is set, any thread entering an Acquire method (that doesn't see Detail_Baselib_EventSemaphore_SetFlag),
    // will wait until it is cleared before proceeding with normal operations.
    Detail_Baselib_EventSemaphore_SetInProgressFlag    = (uint32_t)1 << 30,

    // If this flag is set, threads acquiring the semaphore succeed immediately.
    Detail_Baselib_EventSemaphore_SetFlag              = (uint32_t)2 << 30,

    Detail_Baselib_EventSemaphore_NumWaitingForSetMask = ~((uint32_t)(1 | 2) << 30)
};

typedef struct Baselib_EventSemaphore
{
    Detail_Baselib_EventSemaphore_State state;
    Baselib_SystemSemaphore_Handle setSemaphore;
    Baselib_SystemSemaphore_Handle setInProgressSemaphore;
} Baselib_EventSemaphore;

// How (Timed)Acquire works for the SemaphoreBased EventSemaphore:
//
// If there is a set pending (Detail_Baselib_EventSemaphore_SetInProgressFlag is set),
// it means that not all threads from the previous wakeup call (either via Set or ResetAndReleaseWaitingThreads) have been woken up.
// If we would just continue, we might steal the wakeup tokens of those threads! So instead we wait until they are done.
//
// This is different from the FutexBased version, however there is no way for a user to distinguish that from
// a "regular (but lengthy)" preemption at the start of the function.
// Meaning that we don't care how often the semaphore got set and reset in the meantime!
//
//
// Invariants:
//
// Allowed flag state transitions:
// 0                     -> Set | SetInProgress
// Set | SetInProgress  <-> Set
// Set | SetInProgress  <-> SetInProgress
// Set                   -> 0
// SetInProgress         -> 0
//
// Additionally:
// * numWaitingForSetInProgress can only grow if SetInProgress is set.
// * numWaitingForSet           can only grow if Set is set

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

BASELIB_API void Detail_Baselib_EventSemaphore_SemaphoreBased_AcquireNonSet(int32_t initialNumWaitingForSetAndStateFlags, Baselib_EventSemaphore* semaphore);
COMPILER_WARN_UNUSED_RESULT
BASELIB_API bool Detail_Baselib_EventSemaphore_SemaphoreBased_TryTimedAcquireNonSet(int32_t initialNumWaitingForSetAndStateFlags, Baselib_EventSemaphore* semaphore, uint32_t timeoutInMilliseconds);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif


static FORCE_INLINE bool Detail_Baselib_EventSemaphore_IsSet(int32_t numWaitingForSetAndStateFlags)
{
    return (numWaitingForSetAndStateFlags & Detail_Baselib_EventSemaphore_SetFlag) ? true : false;
}

static FORCE_INLINE bool Detail_Baselib_EventSemaphore_IsSetInProgress(int32_t numWaitingForSetAndStateFlags)
{
    return (numWaitingForSetAndStateFlags & Detail_Baselib_EventSemaphore_SetInProgressFlag) ? true : false;
}

static FORCE_INLINE int32_t Detail_Baselib_EventSemaphore_GetWaitingForSetCount(int32_t numWaitingForSetAndStateFlags)
{
    return numWaitingForSetAndStateFlags & Detail_Baselib_EventSemaphore_NumWaitingForSetMask;
}

// Changes WaitingForSet count without affecting state flags
static FORCE_INLINE int32_t Detail_Baselib_EventSemaphore_SetWaitingForSetCount(int32_t currentNumWaitingForSetAndStateFlags, int32_t newNumWaitingForSet)
{
    return newNumWaitingForSet | (currentNumWaitingForSetAndStateFlags & (~Detail_Baselib_EventSemaphore_NumWaitingForSetMask));
}

BASELIB_INLINE_API Baselib_EventSemaphore Baselib_EventSemaphore_Create(void)
{
    const Baselib_EventSemaphore semaphore =
    {
        {{0, 0}},
        Baselib_SystemSemaphore_Create(),
        Baselib_SystemSemaphore_Create()
    };
    return semaphore;
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryAcquire(Baselib_EventSemaphore* semaphore)
{
    const int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_acquire(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    return Detail_Baselib_EventSemaphore_IsSet(numWaitingForSetAndStateFlags);
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Acquire(Baselib_EventSemaphore* semaphore)
{
    const int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_acquire(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    if (!Detail_Baselib_EventSemaphore_IsSet(numWaitingForSetAndStateFlags))
        Detail_Baselib_EventSemaphore_SemaphoreBased_AcquireNonSet(numWaitingForSetAndStateFlags, semaphore);
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryTimedAcquire(Baselib_EventSemaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_acquire(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    if (!Detail_Baselib_EventSemaphore_IsSet(numWaitingForSetAndStateFlags))
        return Detail_Baselib_EventSemaphore_SemaphoreBased_TryTimedAcquireNonSet(numWaitingForSetAndStateFlags, semaphore, timeoutInMilliseconds);
    return true;
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Reset(Baselib_EventSemaphore* semaphore)
{
    int32_t resetNumWaitingForSetAndStateFlags;
    int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_relaxed(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    do
    {
        resetNumWaitingForSetAndStateFlags = numWaitingForSetAndStateFlags & (~Detail_Baselib_EventSemaphore_SetFlag);
    }
    while (!Baselib_atomic_compare_exchange_weak_32_release_relaxed(
        &semaphore->state.parts.numWaitingForSetAndStateFlags,
        &numWaitingForSetAndStateFlags,
        resetNumWaitingForSetAndStateFlags));
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Set(Baselib_EventSemaphore* semaphore)
{
    int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_relaxed(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    int32_t numWaitingForSetAndStateFlagsSet, numWaitingForSet;

    do
    {
        numWaitingForSetAndStateFlagsSet = numWaitingForSetAndStateFlags | Detail_Baselib_EventSemaphore_SetFlag;
        numWaitingForSet = Detail_Baselib_EventSemaphore_GetWaitingForSetCount(numWaitingForSetAndStateFlags);
        BaselibAssert(numWaitingForSet >= 0, "There needs to be always a non-negative amount of threads waiting for Set");
        if (numWaitingForSet)
            numWaitingForSetAndStateFlagsSet |= Detail_Baselib_EventSemaphore_SetInProgressFlag;
    }
    while (!Baselib_atomic_compare_exchange_weak_32_release_relaxed(
        &semaphore->state.parts.numWaitingForSetAndStateFlags,
        &numWaitingForSetAndStateFlags,
        numWaitingForSetAndStateFlagsSet));

    if (!Detail_Baselib_EventSemaphore_IsSetInProgress(numWaitingForSetAndStateFlags) && numWaitingForSet)
        Baselib_SystemSemaphore_Release(semaphore->setSemaphore, numWaitingForSet);
}

BASELIB_INLINE_API void Baselib_EventSemaphore_ResetAndReleaseWaitingThreads(Baselib_EventSemaphore* semaphore)
{
    // Note that doing a Baselib_EventSemaphore_Set & Baselib_EventSemaphore_Reset has the same observable effects, just slightly slower.

    int32_t numWaitingForSetAndStateFlags = Baselib_atomic_load_32_relaxed(&semaphore->state.parts.numWaitingForSetAndStateFlags);
    int32_t resetNumWaitingForSetAndStateFlags, numWaitingForSet;
    do
    {
        resetNumWaitingForSetAndStateFlags = numWaitingForSetAndStateFlags & (~Detail_Baselib_EventSemaphore_SetFlag);
        numWaitingForSet = Detail_Baselib_EventSemaphore_GetWaitingForSetCount(numWaitingForSetAndStateFlags);
        BaselibAssert(numWaitingForSet >= 0, "There needs to be always a non-negative amount of threads waiting for Set");
        if (numWaitingForSet)
            resetNumWaitingForSetAndStateFlags |= Detail_Baselib_EventSemaphore_SetInProgressFlag;
    }
    while (!Baselib_atomic_compare_exchange_weak_32_release_relaxed(
        &semaphore->state.parts.numWaitingForSetAndStateFlags,
        &numWaitingForSetAndStateFlags,
        resetNumWaitingForSetAndStateFlags));

    if (!Detail_Baselib_EventSemaphore_IsSetInProgress(numWaitingForSetAndStateFlags) && numWaitingForSet)
        Baselib_SystemSemaphore_Release(semaphore->setSemaphore, numWaitingForSet);
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Free(Baselib_EventSemaphore* semaphore)
{
    if (!semaphore)
        return;
    Baselib_SystemSemaphore_Free(semaphore->setSemaphore);
    Baselib_SystemSemaphore_Free(semaphore->setInProgressSemaphore);
}
