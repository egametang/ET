#pragma once

#include "../C/Baselib_ReentrantLock.h"
#include "Time.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // In computer science, the reentrant mutex (recursive mutex, recursive lock) is particular type of mutual exclusion (mutex) device that may be locked multiple
        // times by the same process/thread, without causing a deadlock.

        // While any attempt to perform the "lock" operation on an ordinary mutex (lock) would either fail or block when the mutex is already locked, on a recursive
        // mutex this operation will succeed if and only if the locking thread is the one that already holds the lock. Typically, a recursive mutex tracks the number
        // of times it has been locked, and requires equally many unlock operations to be performed before other threads may lock it.
        //
        // "Reentrant mutex", Wikipedia: The Free Encyclopedia
        // https://en.wikipedia.org/w/index.php?title=Reentrant_mutex&oldid=818566928
        //
        // For optimal performance, baselib::ReentrantLock should be stored at a cache aligned memory location.
        class ReentrantLock
        {
        public:
            // non-copyable
            ReentrantLock(const ReentrantLock& other) = delete;
            ReentrantLock& operator=(const ReentrantLock& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            ReentrantLock(ReentrantLock&& other) = delete;
            ReentrantLock& operator=(ReentrantLock&& other) = delete;

            // Creates a reentrant lock synchronization primitive.
            // If there are not enough system resources to create a lock, process abort is triggered.
            ReentrantLock() : m_ReentrantLockData(Baselib_ReentrantLock_Create())
            {
            }

            // Reclaim resources and memory held by lock.
            //
            // If threads are waiting on the lock, calling free may trigger an assert and may cause process abort.
            // Calling this function with a nullptr result in a no-op
            ~ReentrantLock()
            {
                Baselib_ReentrantLock_Free(&m_ReentrantLockData);
            }

            // Acquire lock.
            //
            // If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
            // to be made before the lock is released.
            // If lock is held by another thread, this function wait for lock to be released.
            //
            // This function is guaranteed to emit an acquire barrier.
            inline void Acquire()
            {
                return Baselib_ReentrantLock_Acquire(&m_ReentrantLockData);
            }

            // Try to acquire lock and return immediately.
            // If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
            // to be made before the lock is released.
            //
            // When lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // Return:          true if lock was acquired.
            COMPILER_WARN_UNUSED_RESULT
            FORCE_INLINE bool TryAcquire()
            {
                return Baselib_ReentrantLock_TryAcquire(&m_ReentrantLockData);
            }

            // Try to acquire lock.
            // If lock is already acquired by the current thread this function increase the lock count so that an equal number of calls to Baselib_ReentrantLock_Release needs
            // to be made before the lock is released.
            // If lock is held by another thread, this function wait for timeoutInMilliseconds for lock to be released.
            //
            // When lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // TryAcquire with a zero timeout differs from TryAcquire() in that TryAcquire() is guaranteed to be a user space operation
            // while TryAcquire with zero timeout may enter the kernel and cause a context switch.
            //
            // Timeout passed to this function may be subject to system clock resolution.
            // If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
            //
            // Return:          true if lock was acquired.
            COMPILER_WARN_UNUSED_RESULT
            FORCE_INLINE bool TryTimedAcquire(const timeout_ms timeoutInMilliseconds)
            {
                return Baselib_ReentrantLock_TryTimedAcquire(&m_ReentrantLockData, timeoutInMilliseconds.count());
            }

            // Release lock.
            // If lock count is still higher than zero after the release operation then lock remain in a locked state.
            // If lock count reach zero the lock is unlocked and made available to other threads
            //
            // When the lock is released this function is guaranteed to emit a release barrier.
            //
            // Calling this function from a thread that doesn't own the lock triggers an assert in debug and causes undefined behavior in release builds.
            FORCE_INLINE void Release()
            {
                return Baselib_ReentrantLock_Release(&m_ReentrantLockData);
            }

            // Acquire lock and invoke user defined function.
            // If lock is held by another thread, this function wait for lock to be released.
            //
            // When a lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // Example usage:
            //  lock.AcquireScoped([] {
            //      enteredCriticalSection++;
            //  });
            template<class FunctionType>
            FORCE_INLINE void AcquireScoped(const FunctionType& func)
            {
                ReleaseOnDestroy releaseScope(*this);
                Acquire();
                func();
            }

            // Try to acquire lock and invoke user defined function.
            // If lock is held by another thread, this function wait for timeoutInMilliseconds for lock to be released.
            // On failure to obtain lock the user defined function is not invoked.
            //
            // When lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // Example usage:
            //  lock.TryAcquireScoped([] {
            //      enteredCriticalSection++;
            //  });
            //
            // Return:          true if lock was acquired.
            template<class FunctionType>
            FORCE_INLINE bool TryAcquireScoped(const FunctionType& func)
            {
                if (TryAcquire())
                {
                    ReleaseOnDestroy releaseScope(*this);
                    func();
                    return true;
                }
                return false;
            }

            // Try to acquire lock and invoke user defined function.
            // If lock is held by another thread, this function wait for timeoutInMilliseconds for lock to be released.
            // On failure to obtain lock the user defined function is not invoked.
            //
            // When lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // Timeout passed to this function may be subject to system clock resolution.
            // If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
            //
            // Example usage:
            //  bool lockAcquired = lock.TryTimedAcquireScoped(std::chrono::minutes(1), [] {
            //      enteredCriticalSection++;
            //  });
            //  assert(lockAcquired);
            //
            // Return:          true if lock was acquired.
            template<class FunctionType>
            FORCE_INLINE bool TryTimedAcquireScoped(const timeout_ms timeoutInMilliseconds, const FunctionType& func)
            {
                if (TryTimedAcquire(timeoutInMilliseconds))
                {
                    ReleaseOnDestroy releaseScope(*this);
                    func();
                    return true;
                }
                return false;
            }

        private:
            class ReleaseOnDestroy
            {
            public:
                FORCE_INLINE ReleaseOnDestroy(ReentrantLock& lockReference) : m_LockReference(lockReference) {}
                FORCE_INLINE ~ReleaseOnDestroy() { m_LockReference.Release(); }
            private:
                ReentrantLock& m_LockReference;
            };

            Baselib_ReentrantLock   m_ReentrantLockData;
        };
    }
}
