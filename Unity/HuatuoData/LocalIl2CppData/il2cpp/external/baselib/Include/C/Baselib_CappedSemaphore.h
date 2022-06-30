#pragma once

// Baselib_CappedSemaphore

// In computer science, a semaphore is a variable or abstract data type used to control access to a common resource by multiple processes in a concurrent
// system such as a multitasking operating system. A semaphore is simply a variable. This variable is used to solve critical section problems and to achieve
// process synchronization in the multi processing environment. A trivial semaphore is a plain variable that is changed (for example, incremented or
// decremented, or toggled) depending on programmer-defined conditions.
//
// A useful way to think of a semaphore as used in the real-world system is as a record of how many units of a particular resource are available, coupled with
// operations to adjust that record safely (i.e. to avoid race conditions) as units are required or become free, and, if necessary, wait until a unit of the
// resource becomes available.
//
// "Semaphore (programming)", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Semaphore_(programming)&oldid=872408126


#if PLATFORM_FUTEX_NATIVE_SUPPORT
#include "Internal/Baselib_CappedSemaphore_FutexBased.inl.h"
#else
#include "Internal/Baselib_CappedSemaphore_SemaphoreBased.inl.h"
#endif

// Creates a capped counting semaphore synchronization primitive.
//
// Cap is the number of tokens that can be held by the semaphore when there is no contention.
// If there are not enough system resources to create a semaphore, process abort is triggered.
//
// For optimal performance, the returned Baselib_CappedSemaphore should be stored at a cache aligned memory location.
//
// \returns          A struct representing a semaphore instance. Use Baselib_CappedSemaphore_Free to free the semaphore.
BASELIB_INLINE_API Baselib_CappedSemaphore Baselib_CappedSemaphore_Create(uint16_t cap);

// Try to consume a token and return immediately.
//
// When successful this function is guaranteed to emit an acquire barrier.
//
// \returns          true if token was consumed. false if not.
BASELIB_INLINE_API bool Baselib_CappedSemaphore_TryAcquire(Baselib_CappedSemaphore* semaphore);

// Wait for semaphore token to become available
//
// This function is guaranteed to emit an acquire barrier.
BASELIB_INLINE_API void Baselib_CappedSemaphore_Acquire(Baselib_CappedSemaphore* semaphore);

// Wait for semaphore token to become available
//
// When successful this function is guaranteed to emit an acquire barrier.
//
// Acquire with a zero timeout differs from TryAcquire in that TryAcquire is guaranteed to be a user space operation
// while Acquire may enter the kernel and cause a context switch.
//
// Timeout passed to this function may be subject to system clock resolution.
// If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
//
// \param timeoutInMilliseconds       Time to wait for token to become available in milliseconds.
//
// \returns          true if token was consumed. false if timeout was reached.
BASELIB_INLINE_API bool Baselib_CappedSemaphore_TryTimedAcquire(Baselib_CappedSemaphore* semaphore, const uint32_t timeoutInMilliseconds);

// Submit tokens to the semaphore.
//
// If threads are waiting an equal amount of tokens are consumed before this function return.
//
// When successful this function is guaranteed to emit a release barrier.
//
// \returns          number of submitted tokens.
BASELIB_INLINE_API uint16_t Baselib_CappedSemaphore_Release(Baselib_CappedSemaphore* semaphore, const uint16_t count);

// Sets the semaphore token count to zero and release all waiting threads.
//
// When successful this function is guaranteed to emit a release barrier.
//
// \returns          number of released threads.
BASELIB_INLINE_API uint32_t Baselib_CappedSemaphore_ResetAndReleaseWaitingThreads(Baselib_CappedSemaphore* semaphore);

// Reclaim resources and memory held by the semaphore.
//
// If threads are waiting on the semaphore, calling free will trigger an assert and may cause process abort.
// Calling this function with a nullptr result in a no-op.
BASELIB_INLINE_API void Baselib_CappedSemaphore_Free(Baselib_CappedSemaphore* semaphore);
