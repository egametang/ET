#pragma once

#include "../C/Baselib_Semaphore.h"
#include "Time.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
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
        //
        // For optimal performance, baselib::Semaphore should be stored at a cache aligned memory location.
        class Semaphore
        {
        public:
            // non-copyable
            Semaphore(const Semaphore& other) = delete;
            Semaphore& operator=(const Semaphore& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            Semaphore(Semaphore&& other) = delete;
            Semaphore& operator=(Semaphore&& other) = delete;

            // This is the max number of tokens guaranteed to be held by the semaphore at
            // any given point in time. Tokens submitted that exceed this value may silently
            // be discarded.
            enum { MaxGuaranteedCount = Baselib_Semaphore_MaxGuaranteedCount };

            // Creates a counting semaphore synchronization primitive.
            // If there are not enough system resources to create a semaphore, process abort is triggered.
            Semaphore() : m_SemaphoreData(Baselib_Semaphore_Create())
            {
            }

            // Reclaim resources and memory held by the semaphore.
            //
            // If threads are waiting on the semaphore, destructor will trigger an assert and may cause process abort.
            ~Semaphore()
            {
                Baselib_Semaphore_Free(&m_SemaphoreData);
            }

            // Wait for semaphore token to become available
            //
            // This function is guaranteed to emit an acquire barrier.
            inline void Acquire()
            {
                return Baselib_Semaphore_Acquire(&m_SemaphoreData);
            }

            // Try to consume a token and return immediately.
            //
            // When successful this function is guaranteed to emit an acquire barrier.
            //
            // Return:          true if token was consumed. false if not.
            inline bool TryAcquire()
            {
                return Baselib_Semaphore_TryAcquire(&m_SemaphoreData);
            }

            // Wait for semaphore token to become available
            //
            // When successful this function is guaranteed to emit an acquire barrier.
            //
            // TryAcquire with a zero timeout differs from TryAcquire() in that TryAcquire() is guaranteed to be a user space operation
            // while Acquire with a zero timeout may enter the kernel and cause a context switch.
            //
            // Timeout passed to this function may be subject to system clock resolution.
            // If the system clock has a resolution of e.g. 16ms that means this function may exit with a timeout error 16ms earlier than originally scheduled.
            //
            // Arguments:
            // - timeout:       Time to wait for token to become available.
            //
            // Return:          true if token was consumed. false if timeout was reached.
            inline bool TryTimedAcquire(const timeout_ms timeoutInMilliseconds)
            {
                return Baselib_Semaphore_TryTimedAcquire(&m_SemaphoreData, timeoutInMilliseconds.count());
            }

            // Submit tokens to the semaphore.
            //
            // When successful this function is guaranteed to emit a release barrier.
            //
            // Increase the number of available tokens on the semaphore by `count`. Any waiting threads will be notified there are new tokens available.
            // If count reach `Baselib_Semaphore_MaxGuaranteedCount` this function may silently discard any overflow.
            inline void Release(uint16_t count)
            {
                return Baselib_Semaphore_Release(&m_SemaphoreData, count);
            }

            // Sets the semaphore token count to zero and release all waiting threads.
            //
            // When successful this function is guaranteed to emit a release barrier.
            //
            // Return:          number of released threads.
            inline uint32_t ResetAndReleaseWaitingThreads()
            {
                return Baselib_Semaphore_ResetAndReleaseWaitingThreads(&m_SemaphoreData);
            }

        private:
            Baselib_Semaphore   m_SemaphoreData;
        };
    }
}
