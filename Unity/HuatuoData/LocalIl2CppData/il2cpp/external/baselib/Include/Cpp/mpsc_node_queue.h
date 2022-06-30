#pragma once

#include "../C/Baselib_Memory.h"
#include "mpsc_node.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // In computer science, a queue is a collection in which the entities in the collection are kept in order and the principal (or only) operations on the
        // collection are the addition of entities to the rear terminal position, known as enqueue, and removal of entities from the front terminal position, known
        // as dequeue. This makes the queue a First-In-First-Out (FIFO) data structure. In a FIFO data structure, the first element added to the queue will be the
        // first one to be removed. This is equivalent to the requirement that once a new element is added, all elements that were added before have to be removed
        // before the new element can be removed. Often a peek or front operation is also entered, returning the value of the front element without dequeuing it.
        // A queue is an example of a linear data structure, or more abstractly a sequential collection.
        //
        // "Queue (abstract data type)", Wikipedia: The Free Encyclopedia
        // https://en.wikipedia.org/w/index.php?title=Queue_(abstract_data_type)&oldid=878671332
        //

        // This implementation is a lockless node queue capable of handling multiple producers and a single consumer (exclusive access)
        //
        // Node types are required to inherit the mpsc_node class. No data from the inherited class is modified/copied, so no restrictions apply.
        // The node memory is allocated and destroyed by the user (user owned).
        // Dequeued nodes may be deleted, overwritten/discarded and/or reused.
        //
        // Notes consumer threads:
        //  Only one consumer thread will exclusively access the front node. Other consumer threads will always progress, either by failing to dequeue or
        //  successfully dequeuing the next node once the current thread thread opens access. As opposed to the parallel consumer implementation,
        //  this is significantly more performant as no DCAS-operations/loops are involved, but if the consumer thread with current exclusive access falls asleep
        //  when dequeuing, no other threads will successfully dequeue until the thread wakes up.
        //  Producer threads always progress independently.
        //
        // Notes on producer threads:
        //  A producer thread swaps the back and writes the link information in two consecutive atomic operations. If a producer thread falls asleep after the
        //  swap and before the link information has been written, the consumer thread(s) will not advance past this point since it doesn't have
        //  the information yet. Therefore the consumer threads calls will yield null until that particular producer thread wakes back up.
        //
        template<typename T>
        class alignas(sizeof(intptr_t) * 2)mpsc_node_queue
        {
        public:
            // Create a new queue instance.
            mpsc_node_queue()
            {
                m_Front.obj = 0;
                m_Back.obj = 0;
                atomic_thread_fence(memory_order_seq_cst);
            }

            // Returns true if queue is empty.
            bool empty() const
            {
                return m_Back.load(memory_order_relaxed) == 0;
            }

            // Push a node to the back of the queue.
            void push_back(T* node)
            {
                node->next.store(0, memory_order_relaxed);
                if (T* prev = m_Back.exchange(node, memory_order_release))
                    prev->next.store(node, memory_order_release);
                else
                    m_Front.store(node, memory_order_release);
            }

            // Push a linked list of nodes to the back of the queue.
            void push_back(T* first_node, T* last_node)
            {
                last_node->next.store(0, memory_order_relaxed);
                if (T* prev = m_Back.exchange(last_node, memory_order_release))
                    prev->next.store(first_node, memory_order_release);
                else
                    m_Front.store(first_node, memory_order_release);
            }

            // Try to pop frontmost node of the queue.
            //
            // Note that if null is returned, there may still be push operations in progress in a producer thread.
            // Use the "empty" function to check if a queue is empty.
            //
            // \returns front node of the queue or null.
            T* try_pop_front()
            {
                T* node, *next, *expected;

                // acquire thread exclusive access of front node, return 0 if fail or queue is empty
                intptr_t front = m_FrontIntPtr.fetch_or(1, memory_order_acquire);
                if ((front & 1) | !(front >> 1))
                    return 0;

                node = (T*)front;
                next = static_cast<T*>(node->next.load(memory_order_relaxed));
                if (!next)
                {
                    // Set to zero, assuming we got the head. Exclusive access maintained as only producer can write zero.
                    m_Front.store(0, memory_order_release);

                    // - filters incomplete nodes
                    // - check if node is back == retrigger new back
                    expected = node;
                    if (!m_Back.compare_exchange_strong(expected, 0, memory_order_acquire, memory_order_relaxed))
                    {
                        // Back progressed or node is incomplete, reset front ptr and return 0.
                        m_Front.store(node, memory_order_release);
                        return 0;
                    }

                    // Successfully got the back, so just return node.
                    return node;
                }

                // Store next (clear block) and return node
                m_Front.store(next, memory_order_release);
                return node;
            }

        private:
            // Space out atomic members to individual cache lines. Required for native LLSC operations on some architectures, others to avoid false sharing
            char _cachelineSpacer0[PLATFORM_CACHE_LINE_SIZE];
            union
            {
                atomic<T*> m_Front;
                atomic<intptr_t> m_FrontIntPtr;
            };
            char _cachelineSpacer1[PLATFORM_CACHE_LINE_SIZE - sizeof(T*)];
            atomic<T*> m_Back;
            char _cachelineSpacer2[PLATFORM_CACHE_LINE_SIZE - sizeof(T*)];

            // Verify mpsc_node is base of T
            static_assert(std::is_base_of<baselib::mpsc_node, T>::value, "Node class/struct used with baselib::mpsc_node_queue must derive from baselib::mpsc_node.");
        };
    }
}
