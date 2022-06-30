#pragma once

// Baselib_EventSemaphore

// In computer science, an event (also called event semaphore) is a type of synchronization mechanism that is used to indicate to waiting processes when a
// particular condition has become true.
// An event is an abstract data type with a boolean state and the following operations:
// * wait - when executed, causes the suspension of the executing process until the state of the event is set to true. If the state is already set to true has no effect.
// * set - sets the event's state to true, release all waiting processes.
// * clear - sets the event's state to false.
//
// "Event (synchronization primitive)", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Event_(synchronization_primitive)&oldid=781517732


#if PLATFORM_FUTEX_NATIVE_SUPPORT
    #include "Internal/Baselib_EventSemaphore_FutexBased.inl.h"
#else
    #include "Internal/Baselib_EventSemaphore_SemaphoreBased.inl.h"
#endif

// Creates an event semaphore synchronization primitive. Initial state of event is unset.
//
// If there are not enough system resources to create a semaphore, process abort is triggered.
//
// For optimal performance, the returned Baselib_EventSemaphore should be stored at a cache aligned memory location.
//
// \returns     A struct representing a semaphore instance. Use Baselib_EventSemaphore_Free to free the semaphore.
BASELIB_INLINE_API Baselib_EventSemaphore Baselib_EventSemaphore_Create(void);

// Try to acquire semaphore.
//
// When semaphore is acquired this function is guaranteed to emit an acquire barrier.
//
// \returns true if event is set, false other wise.
COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryAcquire(Baselib_EventSemaphore* semaphore);

// Acquire semaphore.
//
// This function is guaranteed to emit an acquire barrier.
BASELIB_INLINE_API void Baselib_EventSemaphore_Acquire(Baselib_EventSemaphore* semaphore);

// Try to acquire semaphore.
//
// If event is set this function return true, otherwise the thread will wait for event to be set or for release to be called.
//
// When semaphore is acquired this function is guaranteed to emit an acquire barrier.
//
// Acquire with a zero timeout differs from TryAcquire in that TryAcquire is guaranteed to be a user space operation
// while Acquire may enter the kernel and cause a context switch.
//
// Timeout passed to this function may be subject to system clock resolution.
// If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
//
// \returns     true if semaphore was acquired.
COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_EventSemaphore_TryTimedAcquire(Baselib_EventSemaphore* semaphore, const uint32_t timeoutInMilliseconds);

// Sets the event
//
// Setting the event will cause all waiting threads to wakeup. And will let all future acquiring threads through until Baselib_EventSemaphore_Reset is called.
// It is guaranteed that any thread waiting previously on the EventSemaphore will be woken up, even if the semaphore is immediately reset. (no lock stealing)
//
// Guaranteed to emit a release barrier.
BASELIB_INLINE_API void Baselib_EventSemaphore_Set(Baselib_EventSemaphore* semaphore);

// Reset event
//
// Resetting the event will cause all future acquiring threads to enter a wait state.
// Has no effect if the EventSemaphore is already in a reset state.
//
// Guaranteed to emit a release barrier.
BASELIB_INLINE_API void Baselib_EventSemaphore_Reset(Baselib_EventSemaphore* semaphore);

// Reset event and release all waiting threads
//
// Resetting the event will cause all future acquiring threads to enter a wait state.
// If there were any threads waiting (i.e. the EventSemaphore was already in a release state) they will be released.
//
// Guaranteed to emit a release barrier.
BASELIB_INLINE_API void Baselib_EventSemaphore_ResetAndReleaseWaitingThreads(Baselib_EventSemaphore* semaphore);

// Reclaim resources and memory held by the semaphore.
//
// If threads are waiting on the semaphore, calling free may trigger an assert and may cause process abort.
// Calling this function with a nullptr result in a no-op
BASELIB_INLINE_API void Baselib_EventSemaphore_Free(Baselib_EventSemaphore* semaphore);
