#pragma once

#include "../C/Baselib_CappedSemaphore.h"
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
        // For optimal performance, baselib::CappedSemaphore should be stored at a cache aligned memory location.
        class CappedSemaphore
        {
        public:
            // non-copyable
            CappedSemaphore(const CappedSemaphore& other) = delete;
            CappedSemaphore& operator=(const CappedSemaphore& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            CappedSemaphore(CappedSemaphore&& other) = delete;
            CappedSemaphore& operator=(CappedSemaphore&& other) = delete;

            // Creates a capped counting semaphore synchronization primitive.
            // Cap is the number of tokens that can be held by the semaphore when there is no contention.
            //
            // If there are not enough system resources to create a semaphore, process abort is triggered.
            CappedSemaphore(const uint16_t cap) : m_CappedSemaphoreData(Baselib_CappedSemaphore_Create(cap))
            {
            }

            // Reclaim resources and memory held by the semaphore.
            //
            // If threads are waiting on the semaphore, destructor will trigger an assert and may cause process abort.
            ~CappedSemaphore()
            {
                Baselib_CappedSemaphore_Free(&m_CappedSemaphoreData);
            }

            // Wait for semaphore token to become available
            //
            // This function is guaranteed to emit an acquire barrier.
            inline void Acquire()
            {
                return Baselib_CappedSemaphore_Acquire(&m_CappedSemaphoreData);
            }

            // Try to consume a token and return immediately.
            //
            // When successful this function is guaranteed to emit an acquire barrier.
            //
            // Return:          true if token was consumed. false if not.
            inline bool TryAcquire()
            {
                return Baselib_CappedSemaphore_TryAcquire(&m_CappedSemaphoreData);
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
                return Baselib_CappedSemaphore_TryTimedAcquire(&m_CappedSemaphoreData, timeoutInMilliseconds.count());
            }

            // Submit tokens to the semaphore.
            // If threads are waiting an equal amount of tokens are consumed before this function return.
            //
            // When successful this function is guaranteed to emit a release barrier.
            inline uint16_t Release(const uint16_t count)
            {
                return Baselib_CappedSemaphore_Release(&m_CappedSemaphoreData, count);
            }

            // Sets the semaphore token count to zero and release all waiting threads.
            //
            // When successful this function is guaranteed to emit a release barrier.
            //
            // Return:          number of released threads.
            inline uint32_t ResetAndReleaseWaitingThreads()
            {
                return Baselib_CappedSemaphore_ResetAndReleaseWaitingThreads(&m_CappedSemaphoreData);
            }

        private:
            Baselib_CappedSemaphore   m_CappedSemaphoreData;
        };
    }
}
