#pragma once

// Baselib_ReentrantLock

// In computer science, the reentrant mutex (recursive mutex, recursive lock) is particular type of mutual exclusion (mutex) device that may be locked multiple
// times by the same process/thread, without causing a deadlock.

// While any attempt to perform the "lock" operation on an ordinary mutex (lock) would either fail or block when the mutex is already locked, on a recursive
// mutex this operation will succeed if and only if the locking thread is the one that already holds the lock. Typically, a recursive mutex tracks the number
// of times it has been locked, and requires equally many unlock operations to be performed before other threads may lock it.
//
// "Reentrant mutex", Wikipedia: The Free Encyclopedia
// https://en.wikipedia.org/w/index.php?title=Reentrant_mutex&oldid=818566928

#include "Internal/Baselib_ReentrantLock.inl.h"

// Creates a reentrant lock synchronization primitive.
//
// If there are not enough system resources to create a lock, process abort is triggered.
//
// For optimal performance, the returned Baselib_ReentrantLock should be stored at a cache aligned memory location.
//
// \returns         A struct representing a lock instance. Use Baselib_ReentrantLock_Free to free the lock.
BASELIB_INLINE_API Baselib_ReentrantLock Baselib_ReentrantLock_Create(void);


// Try to acquire lock and return immediately.
// If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
// to be made before the lock is released.
//
// When lock is acquired this function is guaranteed to emit an acquire barrier.
//
// \returns         true if lock was acquired.
COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_ReentrantLock_TryAcquire(Baselib_ReentrantLock* lock);

// Acquire lock.
//
// If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
// to be made before the lock is released.
// If lock is held by another thread, this function wait for lock to be released.
//
// This function is guaranteed to emit an acquire barrier.
BASELIB_INLINE_API void Baselib_ReentrantLock_Acquire(Baselib_ReentrantLock* lock);

// Acquire lock.
// If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
// to be made before the lock is released.
// If lock is held by another thread, this function wait for timeoutInMilliseconds for lock to be released.
//
// When a lock is acquired this function is guaranteed to emit an acquire barrier.
//
// Acquire with a zero timeout differs from TryAcquire in that TryAcquire is guaranteed to be a user space operation
// while Acquire may enter the kernel and cause a context switch.
//
// Timeout passed to this function may be subject to system clock resolution.
// If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
//
// \returns         true if lock was acquired.
COMPILER_WARN_UNUSED_RESULT
BASELIB_INLINE_API bool Baselib_ReentrantLock_TryTimedAcquire(Baselib_ReentrantLock* lock, uint32_t timeoutInMilliseconds);

// Release lock.
// If lock count is still higher than zero after the release operation then lock remain in a locked state.
// If lock count reach zero the lock is unlocked and made available to other threads
//
// When the lock is released this function is guaranteed to emit a release barrier.
//
// Calling this function from a thread that doesn't own the lock result triggers an assert in debug and causes undefined behavior in release builds.
BASELIB_INLINE_API void Baselib_ReentrantLock_Release(Baselib_ReentrantLock* lock);

// Reclaim resources and memory held by lock.
//
// If threads are waiting on the lock, calling free may trigger an assert and may cause process abort.
// Calling this function with a nullptr result in a no-op
BASELIB_INLINE_API void Baselib_ReentrantLock_Free(Baselib_ReentrantLock* lock);
