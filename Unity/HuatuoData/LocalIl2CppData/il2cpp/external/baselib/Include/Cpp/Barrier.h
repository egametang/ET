#pragma once

#include "Atomic.h"
#include "Semaphore.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // In parallel computing, a barrier is a type of synchronization
        // method. A barrier for a group of threads or processes in the source
        // code means any thread/process must stop at this point and cannot
        // proceed until all other threads/processes reach this barrier.
        //
        // "Barrier (computer science)", Wikipedia: The Free Encyclopedia
        // https://en.wikipedia.org/wiki/Barrier_(computer_science)
        //
        // For optimal performance, baselib::Barrier should be stored at a
        // cache aligned memory location.
        class Barrier
        {
        public:
            // non-copyable
            Barrier(const Barrier& other) = delete;
            Barrier& operator=(const Barrier& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            Barrier(Barrier&& other) = delete;
            Barrier& operator=(Barrier&& other) = delete;

            // Creates a barrier with a set number of threads to synchronize.
            // Once a set of threads enter a Barrier, the *same* set of threads
            // must continue to use the Barrier - i.e. no additional threads may
            // enter any of the Acquires. For example, it is *not* allowed to
            // create a Barrier with threads_num=10, then send 30 threads into
            // barrier.Acquire() with the expectation 3 batches of 10 will be
            // released. However, once it is guaranteed that all threads have
            // exited all of the Acquire invocations, it is okay to reuse the
            // same barrier object with a different set of threads - for
            // example, after Join() has been called on all participating
            // threads and a new batch of threads is launched.
            //
            // \param threads_num  Wait for this number of threads before letting all proceed.
            explicit Barrier(uint16_t threads_num)
                : m_threshold(threads_num), m_count(0)
            {
            }

            // Block the current thread until the specified number of threads
            // also reach this `Acquire()`.
            void Acquire()
            {
                // If there is two Barrier::Acquire calls in a row, when the
                // first Acquire releases, one thread may jump out of the gate
                // so fast that it reaches the next Acquire and steals *another*
                // semaphore slot, continuing past the *second* Acquire, before
                // all threads have even left the first Acquire. So, we instead
                // construct two semaphores and alternate between them to
                // prevent this.

                uint16_t previous_value = m_count.fetch_add(1, memory_order_relaxed);
                BaselibAssert(previous_value < m_threshold * 2);

                // If count is in range [0, m_threshold), use semaphore A.
                // If count is in range [m_threshold, m_threshold * 2), use semaphore B.
                bool useSemaphoreB = previous_value >= m_threshold;
                Semaphore& semaphore = useSemaphoreB ? m_semaphoreB : m_semaphoreA;

                // If (count % m_threshold) == (m_threshold - 1), then we're the last thread in the group, release the semaphore.
                bool do_release = previous_value % m_threshold == m_threshold - 1;

                if (do_release)
                {
                    if (previous_value == m_threshold * 2 - 1)
                    {
                        // Note this needs to happen before the Release to avoid
                        // a race condition (if this thread yields right before
                        // the Release, but after the add, the invariant of
                        // previous_value < m_threshold * 2 may break for
                        // another thread)
                        m_count.fetch_sub(m_threshold * 2, memory_order_relaxed);
                    }
                    semaphore.Release(m_threshold - 1);
                }
                else
                {
                    semaphore.Acquire();
                }
            }

        private:
            Semaphore m_semaphoreA;
            Semaphore m_semaphoreB;
            uint16_t m_threshold;
            atomic<uint16_t> m_count;
        };
    }
}
