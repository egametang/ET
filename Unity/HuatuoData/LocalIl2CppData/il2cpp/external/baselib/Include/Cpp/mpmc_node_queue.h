#pragma once

#include "../C/Baselib_Memory.h"
#include "../C/Baselib_Atomic_LLSC.h"
#include "mpmc_node.h"

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

        // This implementation is a lockless node queue capable of handling multiple concurrent producers and consumers
        //
        // Node types are required to inherit the mpmc_node class. No data from the inherited class is modified/copied, so no restrictions apply.
        // The node memory is allocated and destroyed by the user (user owned).
        // Dequeued nodes may be overwritten/discarded and/or reused.
        // Dequeued nodes may not be deleted (released from user space memory) while any consumer thread is in the scope of a deque call.
        //
        // Notes consumer threads:
        //  While dequeued nodes may be reused and/or overwritten they must however remain in application readable memory (user space memory) until it can be
        //  guaranteed no consumer thread is still processing the node i.e. not within the scope of a dequeue call.
        //  Even though the value is ignored (discarded by version check) any consumer thread may still read the node link information.
        //  Consumer threads are concurrently attempting to dequeue the front in a DCAS loop and the first to succeed will update the queue front and other
        //  threads continue processing the next front node in the queue. Threads are garuanteed to progress dequeuing nodes even if another consumer
        //  thread falls asleep during a dequeue, but may fail to dequeue in the combination of the queue getting pre-emptied and the thread resetting the
        //  state (reload back) falls asleep while swapping the back (between 2x consecutive CAS operations).
        //  This is usually an extremely infrequent occurence due to the combination required (can not happen unless there's exactly one item in the queue).
        //  Producer threads always progress independently.
        //
        // Notes on producer threads:
        //  A producer thread swaps the back and writes the link information in two consecutive atomic operations. If a producer thread falls asleep after the
        //  swap and before the link information has been written, the consumer thread(s) will not advance past this point since it doesn't have
        //  the information yet. Therefore the consumer threads calls will yield null until that particular producer thread wakes back up.
        //
        template<typename T>
        class alignas(sizeof(intptr_t) * 2)mpmc_node_queue
        {
        public:
            // Create a new queue instance.
            mpmc_node_queue()
            {
                m_FrontIntPtr = 1;
                m_Front.obj.idx = 1;
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
                {
                    prev->next.store(node, memory_order_release);
                }
                else
                {
                    // store the new front (reload) and add one which will put idx back to an
                    // even number, releasing the consumer threads (ptr is always null and idx odd at this point).
                    if (PLATFORM_LLSC_NATIVE_SUPPORT)
                    {
                        m_FrontPair.ptr.store(node, memory_order_release);
                    }
                    else
                    {
                        m_FrontPair.ptr.store(node, memory_order_relaxed);
                        m_FrontPair.idx.fetch_add(1, memory_order_release);
                    }
                }
            }

            // Push a linked list of nodes to the back of the queue.
            void push_back(T* first_node, T* last_node)
            {
                last_node->next.store(0, memory_order_relaxed);
                if (T* prev = m_Back.exchange(last_node, memory_order_release))
                {
                    prev->next.store(first_node, memory_order_release);
                }
                else
                {
                    if (PLATFORM_LLSC_NATIVE_SUPPORT)
                    {
                        m_FrontPair.ptr.store(first_node, memory_order_release);
                    }
                    else
                    {
                        m_FrontPair.ptr.store(first_node, memory_order_relaxed);
                        m_FrontPair.idx.fetch_add(1, memory_order_release);
                    }
                }
            }

            // Try to pop frontmost node of the queue.
            //
            // Note that if null is returned, there may still be push operations in progress in a producer thread.
            // Use the "empty" function to check if a queue is empty.
            //
            // \returns front node of the queue or null.
            T* try_pop_front()
            {
                T* node, *next;
                if (PLATFORM_LLSC_NATIVE_SUPPORT)
                {
                    intptr_t value;
                    Baselib_atomic_llsc_ptr_acquire_release_v(&m_Front, &node, &next,
                    {
                        // If front bit 0 is set, queue back is being reloaded or queue is empty.
                        value = reinterpret_cast<intptr_t>(node);
                        if (value & 1)
                        {
                            Baselib_atomic_llsc_break();
                            return 0;
                        }

                        // Fetch next node. If zero, node is the current backnode. LLSC Monitor is internally cleared by subsequent cmpxchg.
                        if (!(next = static_cast<T*>(node->next.obj)))
                            goto BackNode;
                    });
                    return node;

                BackNode:
                    // - filters obsolete nodes
                    // - Exclusive access (re-entrant block)
                    T * front = node;
                    if (!m_FrontPair.ptr.compare_exchange_strong(front, reinterpret_cast<T*>(value | 1), memory_order_acquire, memory_order_relaxed))
                        return 0;

                    // - filters incomplete nodes
                    // - check if node is back == retrigger new back
                    if (!m_Back.compare_exchange_strong(front, 0, memory_order_acquire, memory_order_relaxed))
                    {
                        // Back progressed or node is incomplete, restore access and return 0
                        m_FrontIntPtr.fetch_and(~1, memory_order_release);
                        return 0;
                    }

                    // Success, back == front node, back was set to zero above and index / access is restored by producers, so we return the back node.
                    // LLSC monitors invalidates any obsolete nodes still in process in other threads.
                    return node;
                }
                else
                {
                    SequencedFrontPtr front, value;

                    // Get front node. The DCAS while operation will update front on retry
                    front = m_Front.load(memory_order_acquire);
                    do
                    {
                        // If front idx bit 0 is set, queue back is being reloaded or queue is empty.
                        if (front.idx & 1)
                            return 0;

                        // Fetch next node. If zero, node is the current backnode
                        node = front.ptr;
                        if (!(next = static_cast<T*>(node->next.load(memory_order_relaxed))))
                            goto BackNodeDCAS;

                        // On success, replace the current with the next node and return node. On fail, retry with updated front.
                        value.ptr = next;
                        value.idx = front.idx + 2;
                    }
                    while (!m_Front.compare_exchange_strong(front, value, memory_order_acquire, memory_order_relaxed));
                    return node;

                BackNodeDCAS:
                    // - filters obsolete nodes
                    // - Exclusive access (re-entrant block)
                    value.ptr = front.ptr;
                    value.idx = front.idx | 1;
                    if (!m_Front.compare_exchange_strong(front, value, memory_order_acquire, memory_order_relaxed))
                        return 0;

                    // - filters incomplete nodes
                    // - check if node is back == retrigger new back
                    value.ptr = node;
                    if (!m_Back.compare_exchange_strong(value.ptr, 0, memory_order_acquire, memory_order_relaxed))
                    {
                        // Back progressed or node is incomplete, restore access and return 0
                        m_FrontPair.idx.fetch_and(~1, memory_order_release);
                        return 0;
                    }

                    // Success, back == front node, back was set to zero above and index / access is restored by producers, so we return the back node.
                    // Version check invalidates any obsolete nodes in still in process in other threads.
                    return node;
                }
            }

        private:
            typedef struct
            {
                T*       ptr;
                intptr_t idx;
            } SequencedFrontPtr;

            typedef struct
            {
                atomic<T*>       ptr;
                atomic<intptr_t> idx;
            } FrontPair;

            // Space out atomic members to individual cache lines. Required for native LLSC operations on some architectures, others to avoid false sharing
            char _cachelineSpacer0[PLATFORM_CACHE_LINE_SIZE];
            union
            {
                atomic<intptr_t> m_FrontIntPtr;
                FrontPair m_FrontPair;
                atomic<SequencedFrontPtr> m_Front;
            };
            char _cachelineSpacer1[PLATFORM_CACHE_LINE_SIZE - sizeof(SequencedFrontPtr)];
            atomic<T*> m_Back;
            char _cachelineSpacer2[PLATFORM_CACHE_LINE_SIZE - sizeof(T*)];

            // FrontPair is atomic reflections of the SequencedFront fields used for CAS vs DCAS ops. They must match in size and layout.
            // Do note that we can not check layout (offsetof) as the template class is incomplete!
            static_assert(sizeof(mpmc_node_queue::m_FrontPair) == sizeof(mpmc_node_queue::m_Front), "SequencedFrontPtr and FrontPair must be of equal size");

            // Verify mpmc_node is base of T
            static_assert(std::is_base_of<baselib::mpmc_node, T>::value, "Node class/struct used with baselib::mpmc_node_queue must derive from baselib::mpmc_node.");
        };
    }
}
