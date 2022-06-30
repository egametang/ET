#pragma once

#include "CappedSemaphore.h"

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
        // For optimal performance, baselib::BinarySemaphore should be stored at a cache aligned memory location.
        class BinarySemaphore : private CappedSemaphore
        {
        public:

            // Creates a binary semaphore synchronization primitive.
            // Binary means the semaphore can at any given time have at most one token available for consummation.
            //
            // This is just an API facade for CappedSemaphore(1)
            //
            // If there are not enough system resources to create a semaphore, process abort is triggered.
            BinarySemaphore() : CappedSemaphore(1) {}

            using CappedSemaphore::Acquire;
            using CappedSemaphore::TryAcquire;
            using CappedSemaphore::TryTimedAcquire;

            // Submit token to the semaphore.
            // If threads are waiting the token is consumed before this function return.
            //
            // When successful this function is guaranteed to emit a release barrier.
            //
            // \returns          true if a token was submitted, false otherwise (meaning the BinarySemaphore already has a token)
            inline bool Release()
            {
                return CappedSemaphore::Release(1) == 1;
            }
        };
    }
}
