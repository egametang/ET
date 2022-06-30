#pragma once

#include "Atomic.h"
#include "heap_allocator.h"
#include "../C/Baselib_Memory.h"

#include <algorithm>

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

        // This implementation is a fixed size queue capable of handling multiple concurrent producers and consumers
        //
        // Implementation of the queue is lockfree in the sense that one thread always progress. Either by inserting an element or failing to insert an element.
        // Not though, that the data structure in it self is not lock free. In theory if a thread writing an element gets pre-emptied that thread may block reads
        // from proceeding past that point until the writer thread wake up and complete it's operation.
        template<typename value_type, bool cacheline_aligned = true>
        class mpmc_fixed_queue
        {
        public:
            // Create a new queue instance capable of holding at most `capacity` number of elements.
            // `buffer` is an optional user defined memory block large enough to hold the queue data structure.
            // The size required is obtained by `buffer_size`, alignment requirements by `buffer_alignment`.
            // If `buffer` is not set (default), the queue will internally allocate memory using baselib heap_allocator.
            mpmc_fixed_queue(uint32_t capacity, void *buffer = nullptr)
                : m_SlotAllocator()
                , m_Slot(static_cast<Slot*>(buffer ? buffer : m_SlotAllocator.allocate(buffer_size(capacity))))
                , m_UserAllocatedSlots(buffer ? nullptr : m_Slot)
                , m_NumberOfSlots(capacity ? capacity : 2)
                , m_Capacity(capacity)
                , m_ReadPos(0)
                , m_WritePos(0)
            {
                // a zero sized queue uses two slots - the first indicating the queue is empty, the other indicating it is full.
                if (capacity == 0)
                {
                    m_Slot[0].checksum.store(WriteableChecksum(0), baselib::memory_order_relaxed);
                    m_Slot[1].checksum.store(ReadableChecksumPrevGen(1), baselib::memory_order_relaxed);
                    m_WritePos = 1; // Point at the second slot which indicates a full queue
                }
                else
                {
                    // fill queue with 'writable slots'
                    for (uint32_t pos = 0; pos < capacity; ++pos)
                        m_Slot[pos].checksum.store(WriteableChecksum(pos), baselib::memory_order_relaxed);
                }

                baselib::atomic_thread_fence(baselib::memory_order_seq_cst);
            }

            // Destroy queue, guaranteed to also destroy any elements held by the queue.
            //
            // If there are other threads currently accessing the queue behavior is undefined.
            ~mpmc_fixed_queue()
            {
                for (;;)
                {
                    const uint32_t pos = m_ReadPos.fetch_add(1, baselib::memory_order_relaxed);
                    Slot& slot = m_Slot[SlotIndex(pos)];
                    if (slot.checksum.load(baselib::memory_order_acquire) != ReadableChecksum(pos))
                        break;
                    slot.value.~value_type();
                }
                m_SlotAllocator.deallocate(m_UserAllocatedSlots, buffer_size(static_cast<uint32_t>(m_Capacity)));
                baselib::atomic_thread_fence(baselib::memory_order_seq_cst);
            }

            // Try to pop front most element off the queue
            //
            // Note that if several push operations are executed in parallel, the one returning first might not have pushed a new head.
            // Which means that for the user it seems there is a new element in the queue, whereas for the queue the still non-present head will block the removal of any entries.
            //
            // \returns true if element was popped, false if queue was empty
            COMPILER_WARN_UNUSED_RESULT
            bool try_pop_front(value_type& value)
            {
                while (true)
                {
                    // Load current position and checksum.
                    uint32_t pos = m_ReadPos.load(baselib::memory_order_relaxed);
                    Slot* slot = &m_Slot[SlotIndex(pos)];
                    uint32_t checksum = slot->checksum.load(baselib::memory_order_acquire);

                    // As long as it looks like we can read from this slot.
                    while (checksum == ReadableChecksum(pos))
                    {
                        // Try to acquire it and read slot on success.
                        if (m_ReadPos.compare_exchange_weak(pos, pos + 1, baselib::memory_order_relaxed, baselib::memory_order_relaxed))
                        {
                            value = std::move(slot->value);
                            slot->value.~value_type();
                            slot->checksum.store(WriteableChecksumNextGen(pos), baselib::memory_order_release);
                            return true;
                        }
                        // Reload checksum and try again (compare_exchange already reloaded the position)
                        else
                        {
                            slot = &m_Slot[SlotIndex(pos)];
                            checksum = slot->checksum.load(baselib::memory_order_acquire);
                        }
                    }

                    // Is queue empty?
                    if (checksum == WriteableChecksum(pos))
                        return false;
                }
            }

            // Try to append a new element to the end of the queue.
            //
            // Note that if several pop operations are executed in parallel, the one returning first might not have popped the head.
            // Which means that for the user it seems there is a new free slot in the queue, whereas for the queue the still present head will block the addition of new entries.
            //
            // \returns true if element was appended, false if queue was full.
            template<class ... Args>
            COMPILER_WARN_UNUSED_RESULT
            bool try_emplace_back(Args&& ... args)
            {
                while (true)
                {
                    // Load current position and checksum.
                    uint32_t pos = m_WritePos.load(baselib::memory_order_relaxed);
                    Slot* slot = &m_Slot[SlotIndex(pos)];
                    uint32_t checksum = slot->checksum.load(baselib::memory_order_acquire);

                    // As long as it looks like we can write to this slot.
                    while (checksum == WriteableChecksum(pos))
                    {
                        // Try to acquire it and write slot on success.
                        if (m_WritePos.compare_exchange_weak(pos, pos + 1, baselib::memory_order_relaxed, baselib::memory_order_relaxed))
                        {
                            new(&slot->value) value_type(std::forward<Args>(args)...);
                            slot->checksum.store(ReadableChecksum(pos), baselib::memory_order_release);
                            return true;
                        }
                        // Reload checksum and try again (compare_exchange already reloaded the position)
                        else
                        {
                            slot = &m_Slot[SlotIndex(pos)];
                            checksum = slot->checksum.load(baselib::memory_order_acquire);
                        }
                    }

                    // Is queue full?
                    if (checksum == ReadableChecksumPrevGen(pos))
                        return false;
                }
            }

            // Try to push an element to the end of the queue.
            //
            // Note that if several pop operations are executed in parallel, the one returning first might not have popped the head.
            // Which means that for the user it seems there is a new free slot in the queue, whereas for the queue the still present head will block the addition of new entries.
            //
            // \returns  true if element was pushed, false if queue was full.
            COMPILER_WARN_UNUSED_RESULT
            bool try_push_back(const value_type& value)
            {
                return try_emplace_back(value);
            }

            // Try to push an element to the end of the queue.
            //
            // Note that if several pop operations are executed in parallel, the one returning first might not have popped the head.
            // Which means that for the user it seems there is a new free slot in the queue, whereas for the queue the still present head will block the addition of new entries.
            //
            // \returns true if element was pushed, false if queue was full.
            COMPILER_WARN_UNUSED_RESULT
            bool try_push_back(value_type&& value)
            {
                return try_emplace_back(std::forward<value_type>(value));
            }

            // \returns the number of elements that can fit in the queue.
            size_t capacity() const
            {
                return m_Capacity;
            }

            // Calculate the size in bytes of an memory buffer required to hold `capacity` number of elements.
            //
            // \returns Buffer size in bytes.
            static constexpr size_t buffer_size(uint32_t capacity)
            {
                return sizeof(Slot) * (capacity ? capacity : 2);
            }

            // Calculate the required alignment for a memory buffer containing `value_type` elements.
            //
            // \returns Alignment requirement
            static constexpr size_t buffer_alignment()
            {
                return SlotAlignment;
            }

        private:
            static constexpr uint32_t MinTypeAlignment = alignof(value_type) > sizeof(void*) ? alignof(value_type) : sizeof(void*);
            static constexpr uint32_t SlotAlignment = cacheline_aligned && PLATFORM_CACHE_LINE_SIZE > MinTypeAlignment ? PLATFORM_CACHE_LINE_SIZE : MinTypeAlignment;
            static constexpr uint32_t ReadableBit = (uint32_t)1 << 31;
            static constexpr uint32_t WritableMask = ~ReadableBit;
            static constexpr uint32_t WriteableChecksum(uint32_t pos)       { return pos & WritableMask; }
            static constexpr uint32_t ReadableChecksum(uint32_t pos)        { return pos | ReadableBit; }
            constexpr uint32_t WriteableChecksumNextGen(uint32_t pos) const { return (pos + m_NumberOfSlots) & WritableMask; }
            constexpr uint32_t ReadableChecksumPrevGen(uint32_t pos) const  { return (pos - m_NumberOfSlots) | ReadableBit; }

            constexpr uint32_t SlotIndex(uint32_t pos) const           { return pos % m_NumberOfSlots; }

            const baselib::heap_allocator<SlotAlignment> m_SlotAllocator;

            struct alignas(SlotAlignment) Slot
            {
                value_type value;
                baselib::atomic<uint32_t> checksum;
            };
            Slot *const m_Slot;
            void *const m_UserAllocatedSlots;

            // benchmarks show using uint32_t gives ~3x perf boost on 64bit platforms compared to size_t (uint64_t)
            const uint32_t m_NumberOfSlots;
            const size_t   m_Capacity;

            alignas(PLATFORM_CACHE_LINE_SIZE) baselib::atomic<uint32_t> m_ReadPos;
            alignas(PLATFORM_CACHE_LINE_SIZE) baselib::atomic<uint32_t> m_WritePos;
        };
    }
}
