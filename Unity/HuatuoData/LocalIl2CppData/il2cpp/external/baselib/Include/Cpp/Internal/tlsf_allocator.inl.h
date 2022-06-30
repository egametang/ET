#pragma once

#include "../Lock.h"
#include "../mpmc_node_queue.h"
#include "../Algorithm.h"
#include <algorithm>
#include <type_traits>
#include <cstring>

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace detail
        {
            template<class Allocator>
            class tlsf_block_allocator
            {
                baselib::Lock m_CapacityLock;
                ALIGNED_ATOMIC(size_t) m_Capacity;
                baselib::mpmc_node_queue<baselib::mpmc_node> m_FreeBlocks;

                struct Segment
                {
                    uintptr_t data;
                    size_t size;
                    Segment *next;
                } *m_Segments;

                void LinkSegment(Segment* segment, const size_t block_size, size_t block_count)
                {
                    uintptr_t nodeData = segment->data;
                    baselib::mpmc_node* firstNode = reinterpret_cast<baselib::mpmc_node*>(nodeData);
                    baselib::mpmc_node* node = firstNode;
                    for (size_t i = 0; i < block_count; ++i)
                    {
                        node = reinterpret_cast<baselib::mpmc_node*>(nodeData);
                        nodeData += block_size;
                        node->next.obj = reinterpret_cast<baselib::mpmc_node*>(nodeData);
                    }
                    m_FreeBlocks.push_back(firstNode, node);
                }

                bool ExpandCapacity(size_t size, size_t block_size, Allocator& allocator)
                {
                    if (size == 0)
                        return true;

                    // Align to underlying allocator alignment. Size requested must also be of at least block_size
                    block_size = baselib::Algorithm::CeilAligned(block_size, alignment);
                    size = std::max(baselib::Algorithm::CeilAligned(size, alignment), block_size);

                    // Consider base allocator optimal size from required size. I.e if higher than size requested, expand using optimal size.
                    const size_t minSize = size + sizeof(Segment);
                    const size_t optimalSize = allocator.optimal_size(minSize);
                    const size_t segment_size = std::max(optimalSize, minSize);
                    const size_t block_count = size / block_size;

                    // Allocate one memory block that contains block data and Segment info.
                    uintptr_t segmentMemory = reinterpret_cast<uintptr_t>(allocator.allocate(segment_size));
                    if (segmentMemory == 0)
                        return false;

                    // Store data ptr and size information in segment header
                    Segment* segment = reinterpret_cast<Segment*>(segmentMemory + size);
                    segment->data = segmentMemory;
                    segment->size = segment_size;

                    // Link segment to existing segments and add capacity.
                    // This function is in the scope of a locked `m_CapacityLock` which has an implicit acquire (lock) release (unlock) barrier.
                    // Order of m_Segments and m_Capacity is irrelevant. Calling `allocate` from other threads may result in a successful allocation but
                    // that is not a problem since this process repeats in the case of being called from `allocate` and container is pre-emtped.
                    // The side effect of not
                    segment->next = m_Segments;
                    m_Segments = segment;
                    LinkSegment(segment, block_size, block_count);
                    baselib::atomic_fetch_add_explicit(m_Capacity, block_size * block_count, baselib::memory_order_relaxed);
                    return true;
                }

            public:
                static constexpr uint32_t alignment = Allocator::alignment;

                // non-copyable
                tlsf_block_allocator(const tlsf_block_allocator& other) = delete;
                tlsf_block_allocator& operator=(const tlsf_block_allocator& other) = delete;

                // non-movable (strictly speaking not needed but listed to signal intent)
                tlsf_block_allocator(tlsf_block_allocator&& other) = delete;
                tlsf_block_allocator& operator=(tlsf_block_allocator&& other) = delete;

                tlsf_block_allocator() : m_CapacityLock(), m_Capacity(0), m_FreeBlocks(), m_Segments(nullptr)  {}

                void* allocate()
                {
                    return m_FreeBlocks.try_pop_front();
                }

                bool deallocate(void* ptr)
                {
                    m_FreeBlocks.push_back(reinterpret_cast<baselib::mpmc_node*>(ptr));
                    return true;
                }

                bool deallocate(void* ptr_first, void* ptr_last)
                {
                    m_FreeBlocks.push_back(reinterpret_cast<baselib::mpmc_node*>(ptr_first), reinterpret_cast<baselib::mpmc_node*>(ptr_last));
                    return true;
                }

                void deallocate_segments(Allocator& allocator)
                {
                    Segment *segment = m_Segments;
                    while (segment)
                    {
                        Segment *nextSegment = segment->next;
                        allocator.deallocate(reinterpret_cast<void *>(segment->data), segment->size);
                        segment = nextSegment;
                    }
                }

                void reset_segments()
                {
                    if (m_Segments)
                    {
                        m_Segments = nullptr;
                        m_Capacity = 0;
                        m_FreeBlocks.~mpmc_node_queue<baselib::mpmc_node>();
                        new(&m_FreeBlocks) mpmc_node_queue<baselib::mpmc_node>();
                    }
                }

                bool reserve(size_t size, size_t capacity, Allocator& allocator)
                {
                    bool result;
                    m_CapacityLock.AcquireScoped([&] {
                        result = capacity > m_Capacity ? ExpandCapacity(capacity - m_Capacity, size, allocator) : true;
                    });
                    return result;
                }

                bool increase_capacity(size_t size, Allocator& allocator)
                {
                    bool result = true;
                    m_CapacityLock.AcquireScoped([&] {
                        if (m_FreeBlocks.empty())
                            result = ExpandCapacity(m_Capacity == 0 ? size : m_Capacity, size, allocator);
                    });
                    return result;
                }

                size_t capacity() const
                {
                    return baselib::atomic_load_explicit(m_Capacity, baselib::memory_order_relaxed);
                }

                static constexpr size_t optimal_size(const size_t size)
                {
                    return baselib::Algorithm::CeilAligned(size, alignment);
                }
            };

            template<size_t min_size, size_t max_size, size_t linear_subdivisions, class BaseAllocator>
            class tlsf_allocator : private BaseAllocator
            {
                using BlockAllocator = detail::tlsf_block_allocator<BaseAllocator>;

            public:
                static constexpr uint32_t alignment = BaseAllocator::alignment;

                // non-copyable
                tlsf_allocator(const tlsf_allocator& other) = delete;
                tlsf_allocator& operator=(const tlsf_allocator& other) = delete;

                // non-movable (strictly speaking not needed but listed to signal intent)
                tlsf_allocator(tlsf_allocator&& other) = delete;
                tlsf_allocator& operator=(tlsf_allocator&& other) = delete;

                tlsf_allocator() : m_Allocators() {}
                ~tlsf_allocator() { DeallocateSegmentsImpl(); }

                void* try_allocate(size_t size)
                {
                    return getAllocator(size).allocate();
                }

                void* allocate(size_t size)
                {
                    BlockAllocator& allocator = getAllocator(size);
                    do
                    {
                        void* p;
                        if (OPTIMIZER_LIKELY(p = allocator.allocate()))
                            return p;
                        if (!allocator.increase_capacity(AllocatorSize(size), static_cast<BaseAllocator&>(*this)))
                            return nullptr;
                    }
                    while (true);
                }

                void* try_reallocate(void* ptr, size_t old_size, size_t new_size)
                {
                    return ReallocateImpl<true>(ptr, old_size, new_size);
                }

                void* reallocate(void* ptr, size_t old_size, size_t new_size)
                {
                    return ReallocateImpl<false>(ptr, old_size, new_size);
                }

                bool deallocate(void* ptr, size_t size)
                {
                    return ptr == nullptr ? true : getAllocator(size).deallocate(ptr);
                }

                void deallocate_all()
                {
                    atomic_thread_fence(memory_order_acquire);
                    DeallocateSegmentsImpl();
                    for (auto& pow2Allocators : m_Allocators)
                        for (auto& blockAllocator : pow2Allocators)
                            blockAllocator.reset_segments();
                    atomic_thread_fence(memory_order_release);
                }

                bool batch_deallocate(void* ptr_first, void* ptr_last, size_t size)
                {
                    return ((ptr_first ==  nullptr) || (ptr_last == nullptr)) ? false : getAllocator(size).deallocate(ptr_first, ptr_last);
                }

                void batch_deallocate_link(void* ptr, void* ptr_next)
                {
                    reinterpret_cast<baselib::mpmc_node*>(ptr)->next = reinterpret_cast<baselib::mpmc_node*>(ptr_next);
                }

                bool reserve(size_t size, size_t capacity)
                {
                    return getAllocator(size).reserve(AllocatorSize(size), capacity, static_cast<BaseAllocator&>(*this));
                }

                size_t capacity(size_t size)
                {
                    return getAllocator(size).capacity();
                }

                static constexpr size_t optimal_size(const size_t size)
                {
                    return size == 0 ? 0 : BlockAllocator::optimal_size(AllocatorSize(size));
                }

            private:
                struct CompileTime
                {
                    static constexpr size_t Log2Base(size_t value, size_t offset) { return (value > 1) ? Log2Base(value >> (size_t)1, offset + 1) : offset; }
                    static constexpr size_t Log2Base(size_t value) { return Log2Base(value, 0); }
                    static constexpr size_t Max(size_t a, size_t b) { return a > b ? a : b; }
                };

                static constexpr size_t m_MinSize = CompileTime::Max(min_size, CompileTime::Max(CompileTime::Max(sizeof(void*), linear_subdivisions), alignment));
                static constexpr size_t m_MinSizePow2 = baselib::Algorithm::CeilPowerOfTwo(m_MinSize);
                static constexpr size_t m_MaxSizePow2 = baselib::Algorithm::CeilPowerOfTwo(CompileTime::Max(max_size, m_MinSize));
                static constexpr size_t m_MinSizeMask = static_cast<size_t>(1) << CompileTime::Log2Base(m_MinSizePow2 - 1);
                static constexpr size_t m_AllocatorCount = (CompileTime::Log2Base(m_MaxSizePow2) - CompileTime::Log2Base(m_MinSizePow2)) + 1;
                static constexpr size_t m_AllocatorBaseOffsetLog2 =  CompileTime::Log2Base(m_MinSizePow2) - 1;
                static constexpr size_t m_LinearSubdivisionsLog2 = CompileTime::Log2Base(linear_subdivisions);

                static constexpr size_t AllocatorSizeLog2(size_t size) { return baselib::Algorithm::HighestBitNonZero(size | m_MinSizeMask); }
                static constexpr size_t LinearAllocatorSizeLog2(size_t size, size_t sizeLog2) { return (size & ((size_t)1 << sizeLog2) - 1) >> (sizeLog2 - m_LinearSubdivisionsLog2); }

                template<int value = ((m_AllocatorCount == 1 && linear_subdivisions == 1) ? 1 : 2), typename std::enable_if<(value == 1), int>::type = 0>
                static constexpr FORCE_INLINE size_t AllocatorSize(size_t size)
                {
                    return m_MinSizePow2;
                }

                template<int value = ((m_AllocatorCount != 1 && linear_subdivisions == 1) ? 3 : 4), typename std::enable_if<(value == 3), int>::type = 0>
                static constexpr FORCE_INLINE size_t AllocatorSize(size_t size)
                {
                    return (size_t)1 << (AllocatorSizeLog2(size - 1) + 1);
                }

                template<int value = (linear_subdivisions == 1) ? 0 : 1, typename std::enable_if<(value), int>::type = 0>
                static FORCE_INLINE size_t AllocatorSize(size_t size)
                {
                    const size_t subDivSize = ((size_t)1 << baselib::Algorithm::HighestBitNonZero(size)) >> m_LinearSubdivisionsLog2;
                    return (size - 1 & ~(subDivSize - 1)) + subDivSize;
                }

                template<int value = ((m_AllocatorCount == 1 && linear_subdivisions == 1) ? 1 : 2), typename std::enable_if<(value == 1), int>::type = 0>
                BlockAllocator& getAllocator(size_t)
                {
                    return m_Allocators[0][0];
                }

                template<int value = ((m_AllocatorCount != 1 && linear_subdivisions == 1) ? 3 : 4), typename std::enable_if<(value == 3), int>::type = 0>
                BlockAllocator& getAllocator(const size_t size)
                {
                    return m_Allocators[AllocatorSizeLog2(size - 1) - m_AllocatorBaseOffsetLog2][0];
                }

                template<int value = ((m_AllocatorCount == 1 && linear_subdivisions != 1) ? 5 : 6), typename std::enable_if<(value == 5), int>::type = 0>
                BlockAllocator& getAllocator(size_t size)
                {
                    --size;
                    return m_Allocators[0][LinearAllocatorSizeLog2(size, AllocatorSizeLog2(size))];
                }

                template<int value = ((m_AllocatorCount != 1 && linear_subdivisions != 1) ? 7 : 8), typename std::enable_if<(value == 7), int>::type = 0>
                BlockAllocator& getAllocator(size_t size)
                {
                    --size;
                    const size_t sizeLog2 = AllocatorSizeLog2(size);
                    return m_Allocators[sizeLog2 - m_AllocatorBaseOffsetLog2][LinearAllocatorSizeLog2(size, sizeLog2)];
                }

                template<typename T> struct has_deallocate_all
                {
                    template<typename U, void (U::*)()> struct Check;
                    template<typename U> static constexpr bool test(Check<U, &U::deallocate_all> *) { return true; }
                    template<typename U> static constexpr bool test(...) { return false; }
                    static constexpr bool value = test<T>(nullptr);
                };

                template<bool value = has_deallocate_all<BaseAllocator>::value, typename std::enable_if<(value), int>::type = 0>
                void DeallocateSegmentsImpl()
                {
                    BaseAllocator::deallocate_all();
                }

                template<bool value = has_deallocate_all<BaseAllocator>::value, typename std::enable_if<(!value), int>::type = 0>
                void DeallocateSegmentsImpl()
                {
                    for (auto& pow2Allocators : m_Allocators)
                        for (auto& blockAllocator : pow2Allocators)
                            blockAllocator.deallocate_segments(static_cast<BaseAllocator&>(*this));
                }

                template<bool use_try_allocate>
                void* ReallocateImpl(void* ptr, size_t old_size, size_t new_size)
                {
                    if (ptr == nullptr)
                        return use_try_allocate ? try_allocate(new_size) : allocate(new_size);

                    BlockAllocator& oldAllocator = getAllocator(old_size);
                    BlockAllocator& newAllocator = getAllocator(new_size);
                    if (&oldAllocator == &newAllocator)
                        return ptr;

                    void* newPtr = newAllocator.allocate();
                    if ((!use_try_allocate) && (newPtr == nullptr))
                        newPtr = allocate(new_size);

                    if (newPtr)
                    {
                        std::memcpy(newPtr, ptr, std::min(new_size, old_size));
                        oldAllocator.deallocate(ptr);
                    }
                    return newPtr;
                }

                BlockAllocator m_Allocators[m_AllocatorCount][linear_subdivisions];
            };
        }
    }
}
