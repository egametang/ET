#pragma once

#include "../C/Baselib_Memory.h"
#include "../C/Baselib_Atomic_LLSC.h"
#include "mpsc_node.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // In computer science, a stack is an abstract data type that serves as a collection of elements, with two principal operations:
        // * push, which adds an element to the collection, and
        // * pop, which removes the most recently added element that was not yet removed.
        // The order in which elements come off a stack gives rise to its alternative name, LIFO (last in, first out).
        // Additionally, a peek operation may give access to the top without modifying the stack.
        // The name "stack" for this type of structure comes from the analogy to a set of physical items stacked on top of each other,
        // which makes it easy to take an item off the top of the stack, while getting to an item deeper in the stack may require taking off multiple other items first.
        // Considered as a linear data structure, or more abstractly a sequential collection, the push and pop operations occur only at one end of the structure,
        // referred to as the top of the stack. This makes it possible to implement a stack as a singly linked list and a pointer to the top element.
        // A stack may be implemented to have a bounded capacity. If the stack is full and does not contain enough space to accept an entity to be pushed,
        // the stack is then considered to be in an overflow state. The pop operation removes an item from the top of the stack.
        //
        // "Stack (abstract data type)", Wikipedia: The Free Encyclopedia
        // https://en.wikipedia.org/wiki/Stack_(abstract_data_type)
        //

        // This implementation is a lockless node stack capable of handling multiple producers and a single consumer (exclusive access)
        //
        // Node types are required to inherit the mpsc_node class. No data from the inherited class is modified/copied, so no restrictions apply.
        // The node memory is allocated and destroyed by the user (user owned).
        // Popped nodes may be deleted, overwritten/discarded and/or reused.
        //
        // Notes consumer threads:
        //  Only one consumer thread will exclusively access the top node. Other consumer threads will always progress, either by failing to pop or
        //  successfully pop the next node once the current thread thread opens access i.e. if the consumer thread with current exclusive access falls asleep
        //  when popping, no other threads will successfully pop until the thread wakes up.
        //  Producer threads always progress independently.
        //
        template<typename T>
        class alignas(sizeof(intptr_t) * 2)mpsc_node_stack
        {
        public:
            // Create a new stack instance.
            mpsc_node_stack()
            {
                m_Top.obj.ptr = 0;
                m_Top.obj.idx = 0;
                m_ConsumerLock.obj = false;
                atomic_thread_fence(memory_order_seq_cst);
            }

            // Returns true if stack is empty.
            bool empty() const
            {
                return m_Top.load(memory_order_relaxed).ptr == 0;
            }

            // Push a node to the top of the stack.
            void push_back(T* node)
            {
                SequencedTopPtr newtop;
                newtop.ptr = node;
                if (PLATFORM_LLSC_NATIVE_SUPPORT)
                {
                    Baselib_atomic_llsc_ptr_acquire_release_v(&m_Top, &node->next.obj, &newtop, );
                }
                else
                {
                    SequencedTopPtr top = m_Top.load(memory_order_relaxed);
                    do
                    {
                        node->next.store(top.ptr, memory_order_relaxed);
                        newtop.idx = top.idx + 1;
                    }
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_release, memory_order_relaxed));
                }
            }

            // Push a linked list of nodes to the top of the stack.
            void push_back(T* first_node, T* last_node)
            {
                SequencedTopPtr newtop;
                newtop.ptr = first_node;
                if (PLATFORM_LLSC_NATIVE_SUPPORT)
                {
                    Baselib_atomic_llsc_ptr_acquire_release_v(&m_Top, &last_node->next.obj, &newtop, );
                }
                else
                {
                    SequencedTopPtr top = m_Top.load(memory_order_relaxed);
                    do
                    {
                        last_node->next.store(top.ptr, memory_order_relaxed);
                        newtop.idx = top.idx + 1;
                    }
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_release, memory_order_relaxed));
                }
            }

            // Try to pop node from the top of the stack.
            //
            // Note that if null can be returned if another consumer thread has exclusive read access.
            // Use the "empty" function to check if a stack is empty.
            //
            // \returns top node of the stack or null.
            T* try_pop_back()
            {
                if (m_ConsumerLock.exchange(true, memory_order_acquire))
                    return 0;
                T* node;
                SequencedTopPtr newtop;
                if (PLATFORM_LLSC_NATIVE_SUPPORT)
                {
                    Baselib_atomic_llsc_ptr_acquire_release_v(&m_Top, &node, &newtop,
                    {
                        if (!node)
                        {
                            Baselib_atomic_llsc_break();
                            break;
                        }
                        newtop.ptr = static_cast<T*>(node->next.obj);
                    });
                }
                else
                {
                    SequencedTopPtr top = m_Top.load(memory_order_relaxed);
                    do
                    {
                        node = top.ptr;
                        if (!node)
                            break;
                        newtop.ptr = static_cast<T*>(node->next.load(memory_order_relaxed));
                        newtop.idx = top.idx + 1;
                    }
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_relaxed, memory_order_relaxed));
                }
                m_ConsumerLock.store(false, memory_order_release);
                return node;
            }

            // Try to pop all nodes from the stack.
            //
            // Note that if null can be returned if another consumer thread has exclusive read access.
            // Use the "empty" function to check if a stack is empty.
            //
            // \returns linked list of nodes or null.
            T* try_pop_all()
            {
                if (m_ConsumerLock.exchange(true, memory_order_acquire))
                    return 0;
                T* node;
                SequencedTopPtr newtop;
                newtop.ptr = 0;
                if (PLATFORM_LLSC_NATIVE_SUPPORT)
                {
                    Baselib_atomic_llsc_ptr_acquire_release_v(&m_Top, &node, &newtop,
                    {
                        if (!node)
                        {
                            Baselib_atomic_llsc_break();
                            break;
                        }
                    });
                }
                else
                {
                    SequencedTopPtr top = m_Top.load(memory_order_relaxed);
                    do
                    {
                        node = top.ptr;
                        if (!node)
                            break;
                        newtop.idx = top.idx + 1;
                    }
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_relaxed, memory_order_relaxed));
                }
                m_ConsumerLock.store(false, memory_order_release);
                return node;
            }

        private:
            typedef struct
            {
                T*       ptr;
                intptr_t idx;
            } SequencedTopPtr;

            // Space out atomic members to individual cache lines. Required for native LLSC operations on some architectures, others to avoid false sharing
            char _cachelineSpacer0[PLATFORM_CACHE_LINE_SIZE];
            atomic<SequencedTopPtr> m_Top;
            char _cachelineSpacer1[PLATFORM_CACHE_LINE_SIZE - sizeof(SequencedTopPtr)];
            atomic<bool> m_ConsumerLock;
            char _cachelineSpacer2[PLATFORM_CACHE_LINE_SIZE - sizeof(bool)];

            // Verify mpsc_node is base of T
            static_assert(std::is_base_of<baselib::mpsc_node, T>::value, "Node class/struct used with baselib::mpsc_node_stack must derive from baselib::mpsc_node.");
        };
    }
}
