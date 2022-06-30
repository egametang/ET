#pragma once

#include "../C/Baselib_Memory.h"
#include "../C/Baselib_Atomic_LLSC.h"
#include "mpmc_node.h"

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

        // This implementation is a lockless node stack capable of handling multiple concurrent producers and consumers
        //
        // Node types are required to inherit the mpmc_node class. No data from the inherited class is modified/copied, so no restrictions apply.
        // The node memory is allocated and destroyed by the user (user owned).
        // Popped nodes may be overwritten/discarded and/or reused.
        // Popped nodes may not be deleted (released from user space memory) while any consumer thread is in the scope of a pop call.
        //
        // Notes consumer threads:
        //  While popped nodes may be reused and/or overwritten they must however remain in application readable memory (user space memory) until it can be
        //  guaranteed no consumer thread is still processing the node i.e. not within the scope of a pop call.
        //  Even though the value is ignored (discarded by version check) any consumer thread may still read the node link information.
        //  Consumer threads are concurrently attempting to pop the top of the stack in a DCAS loop and the first to succeed will update the stack top and other
        //  threads continue processing the next top node in the stack. Threads are garuanteed to progress to pop nodes even if another consumer
        //  thread falls asleep during a pop call
        //
        template<typename T>
        class alignas(sizeof(intptr_t) * 2)mpmc_node_stack
        {
        public:
            // Create a new stack instance.
            mpmc_node_stack()
            {
                m_Top.obj.ptr = 0;
                m_Top.obj.idx = 0;
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
            // \returns top node of the stack or null if the stack is empty.
            T* try_pop_back()
            {
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
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_acquire, memory_order_relaxed));
                }
                return node;
            }

            // Try to pop all nodes from the stack.
            //
            // \returns linked list of nodes or null if the stack is empty.
            T* try_pop_all()
            {
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
                    while (!m_Top.compare_exchange_strong(top, newtop, memory_order_acquire, memory_order_relaxed));
                }
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

            // Verify mpmc_node is base of T
            static_assert(std::is_base_of<baselib::mpmc_node, T>::value, "Node class/struct used with baselib::mpmc_node_stack must derive from baselib::mpmc_node.");
        };
    }
}
