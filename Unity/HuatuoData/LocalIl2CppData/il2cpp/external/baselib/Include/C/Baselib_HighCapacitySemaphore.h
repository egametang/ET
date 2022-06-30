#pragma once

// Baselib_HighCapacitySemaphore
// This semaphore is similar to Baselib_Semaphore but allows for a far greater token count for a price of a bit slower performance.
// This semaphore is usable for counting resources.

// This is the max number of tokens guaranteed to be held by the semaphore at
// any given point in time. Tokens submitted that exceed this value may silently be discarded.
static const int64_t Baselib_HighCapacitySemaphore_MaxGuaranteedCount = UINT64_C(1) << 61;

#if PLATFORM_FUTEX_NATIVE_SUPPORT
#include "Internal/Baselib_HighCapacitySemaphore_FutexBased.inl.h"
#else
#include "Internal/Baselib_HighCapacitySemaphore_SemaphoreBased.inl.h"
#endif

// Creates a counting semaphore synchronization primitive.
//
// If there are not enough system resources to create a semaphore, process abort is triggered.
//
// For optimal performance, the returned Baselib_HighCapacitySemaphore should be stored at a cache aligned memory location.
//
// \returns          A struct representing a semaphore instance. Use Baselib_HighCapacitySemaphore_Free to free the semaphore.
BASELIB_INLINE_API Baselib_HighCapacitySemaphore Baselib_HighCapacitySemaphore_Create(void);

// Wait for semaphore token to become available
//
// This function is guaranteed to emit an acquire barrier.
// Returns if token was consumed or was woken up by Baselib_HighCapacitySemaphore_ResetAndReleaseWaitingThreads.
BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Acquire(Baselib_HighCapacitySemaphore* semaphore);

// Try to consume a token and return immediately.
//
// When successful this function is guaranteed to emit an acquire barrier.
//
// \returns          true if token was consumed. false if not.
BASELIB_INLINE_API bool Baselib_HighCapacitySemaphore_TryAcquire(Baselib_HighCapacitySemaphore* semaphore);

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
// \param timeout   Time to wait for token to become available.
//
// \returns          true if token was consumed or was woken up by Baselib_HighCapacitySemaphore_ResetAndReleaseWaitingThreads. false if timeout was reached.
BASELIB_INLINE_API bool Baselib_HighCapacitySemaphore_TryTimedAcquire(Baselib_HighCapacitySemaphore* semaphore, const uint32_t timeoutInMilliseconds);

// Submit tokens to the semaphore.
//
// When successful this function is guaranteed to emit a release barrier.
//
// Increase the number of available tokens on the semaphore by `count`. Any waiting threads will be notified there are new tokens available.
// If count reach `Baselib_HighCapacitySemaphore_MaxGuaranteedCount` this function may silently discard any overflow.
BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Release(Baselib_HighCapacitySemaphore* semaphore, const uint32_t count);

// If threads are waiting on Baselib_HighCapacitySemaphore_Acquire / Baselib_HighCapacitySemaphore_TryTimedAcquire,
// releases enough tokens to wake them up. Otherwise consumes all available tokens.
//
// When successful this function is guaranteed to emit a release barrier.
//
// \returns          number of released threads.
BASELIB_INLINE_API uint64_t Baselib_HighCapacitySemaphore_ResetAndReleaseWaitingThreads(Baselib_HighCapacitySemaphore* semaphore);

// Reclaim resources and memory held by the semaphore.
//
// If threads are waiting on the semaphore, calling free will trigger an assert and may cause process abort.
// Calling this function with a nullptr result in a no-op
BASELIB_INLINE_API void Baselib_HighCapacitySemaphore_Free(Baselib_HighCapacitySemaphore* semaphore);
