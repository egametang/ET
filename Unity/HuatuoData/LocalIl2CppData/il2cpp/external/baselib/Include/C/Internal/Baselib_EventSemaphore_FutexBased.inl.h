#pragma once

#include "../Baselib_CountdownTimer.h"
#include "../Baselib_Atomic_TypeSafe.h"
#include "../Baselib_SystemFutex.h"

#if !PLATFORM_FUTEX_NATIVE_SUPPORT
    #error "Only use this implementation on top of a proper futex, in all other situations us Baselib_EventSemaphore_SemaphoreBased.inl.h"
#endif

typedef struct Baselib_EventSemaphore
{
    int32_t state;
    char _cachelineSpacer1[PLATFORM_CACHE_LINE_SIZE - sizeof(int32_t)];
} Baselib_EventSemaphore;


// The futex based event semaphore is in one of *three* states:
// * ResetNoWaitingThreads: EventSemaphore blocks threads, but there aren't any blocked yet
// * Reset: EventSemaphore blocks threads and there are some already
// * Set: EventSemaphore is not blocking any acquiring threads
//
// The ResetNoWaitingThreads state is an optimization that allows us to avoid the (comparatively) costly futex notification syscalls.
//
// In addition, there is a generation counter baked into the state variable in order to prevent lock stealing.
// -> Any change in the state during acquire (other than going from ResetNoWaitingThreads to Reset) means that the thread can continue
//    (since in this case either it was set on the current generation or the generation was changed which implies an earlier release operation)
//
// Allowed state transitions:
// ResetNoWaitingThreads-Gen(X) -> Reset-Gen(X)                             == Acquire/TryTimedAcquire if no thread was waiting already
// ResetNoWaitingThreads-Gen(X) -> Set-Gen(X)                               == Set but no thread was waiting
// Reset-Gen(X)                 -> Set-Get(X+1)                             == Set if threads were waiting
// Set-Get(X)                   -> ResetNoWaitingThreads-Gen(X)             == Reset/ResetAndReleaseWaitingThreads
// Reset-Gen(X)                 -> ResetNoWaitingThreads-Gen(X+1)           == ResetAndReleaseWaitingThreads if threads were waiting
//
// Note how any state transition from Reset requires increasing the generation counter.

enum
{
    //Detail_Baselib_EventSemaphore_ResetNoWaitingThreads = 0,
    Detail_Baselib_EventSemaphore_Set     = (uint32_t)1 << 30,
    Detail_Baselib_EventSemaphore_Reset   = (uint32_t)2 << 30,
    Detail_Baselib_EventSemaphore_GenMask = ~((uint32_t)(1 | 2) << 30)
};

static FORCE_INLINE uint32_t Detail_Baselib_EventSemaphore_Generation(int32_t state)
{
    return state & Detail_Baselib_EventSemaphore_GenMask;
}

// If Detail_Baselib_EventSemaphore_ResetNoWaitingThreads is set, sets Detail_Baselib_EventSemaphore_Reset flag.
// Returns last known state of the semaphore.
// Does nothing if state changed while this function runs (that includes generation changes while attempting to set the ResetState!)
static FORCE_INLINE uint32_t Detail_Baselib_EventSemaphore_TransitionFrom_ResetNoWaitingThreadsState_To_ResetState(Baselib_EventSemaphore* semaphore)
{
    int32_t state = Baselib_atomic_load_32_acquire(&semaphore->state);
    const int32_t resetState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Reset;
    const int32_t resetNoWaitingThreadsState = Detail_Baselib_EventSemaphore_Generation(state);
    while (state == resetNoWaitingThreadsState)
    {
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->state, &state, resetState))
            return resetState;
    }
    return state;
}

BASELIB_INLINE_API Baselib_EventSemaphore Baselib_EventSemaphore_Create(void)
{
    const Baselib_EventSemaphore semaphore = { 0, {0} };
    return semaphore;
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryAcquire(Baselib_EventSemaphore* semaphore)
{
    const int32_t state = Baselib_atomic_load_32_acquire(&semaphore->state);
    return state & Detail_Baselib_EventSemaphore_Set ? true : false;
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Acquire(Baselib_EventSemaphore* semaphore)
{
    const int32_t state = Detail_Baselib_EventSemaphore_TransitionFrom_ResetNoWaitingThreadsState_To_ResetState(semaphore);
    if (state & Detail_Baselib_EventSemaphore_Set)
        return;
    do
    {
        // State is now in Detail_Baselib_EventSemaphore_Reset-Gen(X).
        Baselib_SystemFutex_Wait(&semaphore->state, state, UINT32_MAX);
        // If the state has changed in any way, it is now in either of
        // Set-Gen(X), Set-Gen(X+n), ResetNoWaitingThreads-Gen(X+n) or Reset(X+n). (with n>0)
        if (state != Baselib_atomic_load_32_relaxed(&semaphore->state))
            return;
    }
    while (true);
}

COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryTimedAcquire(Baselib_EventSemaphore* semaphore, const uint32_t timeoutInMilliseconds)
{
    const int32_t state = Detail_Baselib_EventSemaphore_TransitionFrom_ResetNoWaitingThreadsState_To_ResetState(semaphore);
    if (state & Detail_Baselib_EventSemaphore_Set)
        return true;
    uint32_t timeLeft = timeoutInMilliseconds;
    const Baselib_CountdownTimer timer = Baselib_CountdownTimer_StartMs(timeoutInMilliseconds);
    do
    {
        // State is now in Detail_Baselib_EventSemaphore_Reset-Gen(X).
        Baselib_SystemFutex_Wait(&semaphore->state, state, timeLeft);
        // If the state has changed in any way, it is now in either of
        // Set-Gen(X), Set-Gen(X+n), ResetNoWaitingThreads-Gen(X+n) or Reset(X+n). (with n>0)
        if (state != Baselib_atomic_load_32_relaxed(&semaphore->state))
            return true;
        timeLeft = Baselib_CountdownTimer_GetTimeLeftInMilliseconds(timer);
    }
    while (timeLeft);

    // The EventSemaphore looks now like there are still threads waiting even if there *might* be none!
    // This is not an issue however, since it merely means that Set/ResetAndReleaseWaitingThreads will do a potentially redundant futex notification.

    return false;
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Reset(Baselib_EventSemaphore* semaphore)
{
    int32_t state = Baselib_atomic_load_32_relaxed(&semaphore->state);
    const int32_t setState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Set;
    while (state == setState)
    {
        const int32_t resetNoWaitingThreadsState = Detail_Baselib_EventSemaphore_Generation(state);
        if (Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->state, &state, resetNoWaitingThreadsState))
            return;
    }
    Baselib_atomic_thread_fence_release();
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Set(Baselib_EventSemaphore* semaphore)
{
    int32_t state = Baselib_atomic_load_32_relaxed(&semaphore->state);
    const int32_t resetNoWaitingThreadsState = Detail_Baselib_EventSemaphore_Generation(state);
    const int32_t resetState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Reset;

    // If there is no thread waiting on the semaphore, there is no need to wake & increase the generation count.
    // Just set it to Set if it isn't already.
    while (state == resetNoWaitingThreadsState)
    {
        const int32_t setState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Set;
        if (Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->state, &state, setState))
            return;
    }
    // If this is not the case however, we do exactly that, increase the generation & wake all threads.
    while (state == resetState)
    {
        const int32_t nextGenSetState = Detail_Baselib_EventSemaphore_Generation(state + 1) | Detail_Baselib_EventSemaphore_Set;
        if (Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->state, &state, nextGenSetState))
        {
            Baselib_SystemFutex_Notify(&semaphore->state, UINT32_MAX, Baselib_WakeupFallbackStrategy_All);
            return;
        }
    }
    // EventSemaphore was already in set state.
    Baselib_atomic_thread_fence_release();
}

BASELIB_INLINE_API void Baselib_EventSemaphore_ResetAndReleaseWaitingThreads(Baselib_EventSemaphore* semaphore)
{
    // Note that doing a Baselib_EventSemaphore_Set & Baselib_EventSemaphore_Reset has the same observable effects, just slightly slower.

    int32_t state = Baselib_atomic_load_32_relaxed(&semaphore->state);
    const int32_t setState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Set;
    const int32_t resetState = Detail_Baselib_EventSemaphore_Generation(state) | Detail_Baselib_EventSemaphore_Reset;

    // If there is no thread waiting on the semaphore, there is no need to wake & increase the generation count.
    // Just set it to ResetNoWaitingThreads if it isn't already.
    while (state == setState)
    {
        const int32_t resetNoWaitingThreadsState = Detail_Baselib_EventSemaphore_Generation(state);
        if (Baselib_atomic_compare_exchange_weak_32_release_relaxed(&semaphore->state, &state, resetNoWaitingThreadsState))
            return;
    }
    // If this is not the case however, we do exactly that, increase the generation & wake all threads.
    while (state == resetState)
    {
        const int32_t nextGenPendingResetState = Detail_Baselib_EventSemaphore_Generation(state + 1);
        if (Baselib_atomic_compare_exchange_weak_32_relaxed_relaxed(&semaphore->state, &state, nextGenPendingResetState))
        {
            Baselib_SystemFutex_Notify(&semaphore->state, UINT32_MAX, Baselib_WakeupFallbackStrategy_All);
            return;
        }
    }

    // EventSemaphore was already in ResetNoWaiting threads state.
    Baselib_atomic_thread_fence_release();
}

BASELIB_INLINE_API void Baselib_EventSemaphore_Free(Baselib_EventSemaphore* semaphore)
{
}
