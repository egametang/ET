#pragma once

#include "../C/Baselib_Lock.h"
#include "Time.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // In computer science, a lock or mutex (from mutual exclusion) is a synchronization mechanism for enforcing limits on access to a resource in an environment
        // where there are many threads of execution. A lock is designed to enforce a mutual exclusion concurrency control policy.
        //
        // "Lock (computer science)", Wikipedia: The Free Encyclopedia
        // https://en.wikipedia.org/w/index.php?title=Lock_(computer_science)&oldid=875674239
        class Lock
        {
        public:
            // non-copyable
            Lock(const Lock& other) = delete;
            Lock& operator=(const Lock& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            Lock(Lock&& other) = delete;
            Lock& operator=(Lock&& other) = delete;

            // Creates a lock synchronization primitive.
            // If there are not enough system resources to create a lock, process abort is triggered.
            Lock() : m_LockData(Baselib_Lock_Create())
            {
            }

            // Reclaim resources and memory held by lock.
            // If threads are waiting on the lock, calling free may trigger an assert and may cause process abort.
            ~Lock()
            {
                Baselib_Lock_Free(&m_LockData);
            }

            // Acquire lock.
            //
            // If lock is held, either by this or another thread, then the function wait for lock to be released.
            //
            // This function is guaranteed to emit an acquire barrier.
            inline void Acquire()
            {
                return Baselib_Lock_Acquire(&m_LockData);
            }

            // Try to acquire lock and return immediately.
            // If lock is held, either by this or another thread, then lock is not acquired and function return false.
            //
            // When a lock is acquired this function is guaranteed to emit an acquire barrier.
            //
            // Return:          true if lock was acquired.
            COMPILER_WARN_UNUSED_RESULT
            FORCE_INLINE bool TryAcquire()
            {
                return Baselib_Lock_TryAcquire(&m_LockData);
            }

            // Try to acquire lock.
            // If lock is held, either by this or another thread, then the function wait for timeoutInMilliseconds for lock to be released.
            //
            // When a lock is acquired this function is guaranteed to emit an acquire barrier.
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
                return Baselib_Lock_TryTimedAcquire(&m_LockData, timeoutInMilliseconds.count());
            }

            // Release lock and make it available to other threads.
            //
            // This function can be called from any thread, not only the thread that acquired the lock.
            // If no lock was previously held calling this function result in a no-op.
            //
            // When the lock is released this function is guaranteed to emit a release barrier.
            FORCE_INLINE void Release()
            {
                return Baselib_Lock_Release(&m_LockData);
            }

            // Acquire lock and invoke user defined function.
            // If lock is held, either by this or another thread, then the function wait for lock to be released.
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
            // If lock is held, either by this or another thread, then lock is not acquired and function return false.
            // On failure to obtain lock the user defined function is not invoked.
            //
            // When a lock is acquired this function is guaranteed to emit an acquire barrier.
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
            // If lock is held, either by this or another thread, then the function wait for timeoutInMilliseconds for lock to be released.
            // On failure to obtain lock the user defined function is not invoked.
            //
            // When a lock is acquired this function is guaranteed to emit an acquire barrier.
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
                FORCE_INLINE ReleaseOnDestroy(Lock& lockReference) : m_LockReference(lockReference) {}
                FORCE_INLINE ~ReleaseOnDestroy() { m_LockReference.Release(); }
            private:
                Lock& m_LockReference;
            };

            Baselib_Lock   m_LockData;
        };
    }
}
