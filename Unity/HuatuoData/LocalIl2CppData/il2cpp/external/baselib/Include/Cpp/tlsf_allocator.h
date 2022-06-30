#pragma once

#include "Internal/tlsf_allocator.inl.h"
#include "heap_allocator.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // tlsf_allocator (Two-Level Segregated Fit)
        // Lockless, dynamic-sized allocator capable of handling multiple concurrent allocations and deallocations (unless otherwise stated).
        // The cost (in processor instructions) allocating from the pool is O(1).
        // Allocating from the pool is lockless, except when capacity is required to increase, in which case the capacity is doubled for the a size range
        // of the particular allocation size request (see details below).
        //
        // Strict segregated fit allocation mechanism is applied, a requested size is rounded up to the next allocator provided size.
        // The granularity of provided sizes (size ranges) are defined by the `min_size`, `max_size` and `linear_subdivisions` parameters provided.
        // `optimal_size` can be called to obtain the actual/best fit size of an allocation for a certain requested size.
        //
        // A two-Level segregated fit allocator can be said to have two dimensions, or levels.
        // The first level provides size ranges of pow2 segments.
        // The second level provides size ranges of the first level pow2 segments size range divided by the `linear_subdivisions` parameter value.
        // Size range of a given size is be calculated as follows:
        //
        // int invSizeMask = ((1 << (int)log2(size)) / linear_subdivisions) - 1;  // Inverse subdivision mask based on Pow2 of `size`, which is effectively range
        // int lowerBound = (size - 1 & ~invSizeMask) + 1;
        // int upperBound = lowerBound + invSizeMask;
        //
        // As an example, the (internal) size allocated for a requested size of 1500 with linear_subdivisions of 16 is 1536 range(1473-1536).
        //
        // Notes on performance/memory requirements:
        //
        // - This implementation is a segregated storage algorithm and does not, unlike a segregated fit algorithm (aka buddy allocator) split and coalesce
        // memory blocks. A segregated fit is well suited for a single threaded/lock-based implementation but would require multiple atomic operations to split
        // or coalesce blocks.
        //
        // - All allocators share a common base instance of the backing allocator `Allocator`, which is used for allocation when the capacity is required to
        // increase. Memory is only freed up when the tlsf allocator `deallocate_all` or destructor is invoked.
        // Furthermore, `deallocate_all` is optional to declare in the backing allocator `Allocator` and is if so invoked (once) instead of multiple `deallocate`
        // calls when `deallocate_all` (or destructor) is invoked on the tlsf allocator.
        //
        // - The allocator is constructed with only as many block allocators required for the selected min-max range with linear_subdivisions.
        // I.e. one allocator with (min_size, max_size, linear_subdivisions) 32,1024,8 has the same memory footprint as two 32,512,8 and 513,1024,8.
        // If either level allocator only requires a single allocator providing a range, code for calculating allocator indices is optimized away by template
        // construction. Additionally, if size is known at compile-time (const or sizeof) lookup can be optimized away by the compiler.
        //
        // - No overhead per allocation (no header information).
        //
        // - Internally, all memory block sizes must be rounded up to a multiple of alignment. I.e if alignment is 64, buckets containing 96 byte size allocations
        // will in internally use 128 byte blocks. Additionally, smallest size allocated will always be greater than or equal to `linear_subdivisions`.
        //
        // - The allocator relies on that the free memory pool must be persisted and read/write accessible as link information of free memory blocks are
        // read/written to by the allocator operations.
        //
        // Examples:
        // Range is within a single pow2 range, no subdivisions. No lookup code needed.
        // using BlockAllocator = tlsf_allocator<17, 32, 1>;
        //
        // Range is within a single pow2 range with 8 subdivisions, so in this case with linear increments (128/8=16) of bucket sizes. Second level lookup only.
        // using SegregatedFitAllocatorLinear = tlsf_allocator<129, 256, 8>;
        //
        // Range is several pow2 ranges, no subdivisions so pow2 size increments of bucket sizes.
        // using SegregatedFitAllocatorPow2 = tlsf_allocator<129, 2048, 1>;
        //
        // Range is several pow2 ranges, with 32 subdivisions each, so pow2 size increments where each pow2 contains an array of buckets with linear size
        // increments (pow2sz/32) of bucket sizes.
        // using TLSFAllocator = tlsf_allocator<129, 2048, 32>;
        //
        //
        // tlsf_allocator<size_t min_size, size_t max_size, size_t linear_subdivisions = 1, class Allocator = baselib::heap_allocator<>>
        //
        // min_size             - valid minimum size of allocations.
        // max_size             - valid maximum size of allocations. Must be less or equal to the size addressable by integral type `size_t` divided by two plus 1.
        // linear_subdivisions  - number of linear subdivisions of second level allocators (defaults to 1). Must be a power of two and less or equal to `min_size`
        // Allocator            - Backing memory allocator. Defaults to baselib heap_allocator.
        //
        template<size_t min_size, size_t max_size, size_t linear_subdivisions = 1, class Allocator = baselib::heap_allocator<> >
        class tlsf_allocator : protected detail::tlsf_allocator<min_size, max_size, linear_subdivisions, Allocator>
        {
            using Impl = detail::tlsf_allocator<min_size, max_size, linear_subdivisions, Allocator>;

            static_assert(min_size <= max_size, "min_size > max_size");
            static_assert(min_size >= linear_subdivisions, "min_size < linear_subdivisions");
            static_assert(max_size <= std::numeric_limits<size_t>::max() / 2 + 1, "max_size > std::numeric_limits<size_t>::max() / 2 + 1");
            static_assert(baselib::Algorithm::IsPowerOfTwo(linear_subdivisions), "linear_subdivisions != pow2");

        public:
            // non-copyable
            tlsf_allocator(const tlsf_allocator& other) = delete;
            tlsf_allocator& operator=(const tlsf_allocator& other) = delete;

            // non-movable (strictly speaking not needed but listed to signal intent)
            tlsf_allocator(tlsf_allocator&& other) = delete;
            tlsf_allocator& operator=(tlsf_allocator&& other) = delete;

            // Allocated memory is guaranteed to always be aligned to at least the value of `alignment`.
            static constexpr uint32_t alignment = Impl::alignment;

            // Creates a new instance
            tlsf_allocator()
            {
                atomic_thread_fence(memory_order_seq_cst);
            }

            // Destroy allocator, deallocates any memory allocated.
            //
            // If there are other threads currently accessing the allocator behavior is undefined.
            ~tlsf_allocator() {}

            // Allocates a memory block large enough to hold `size` number of bytes if allocation does not require increasing capacity.
            //
            // \returns Address to memory block of allocated memory or nullptr if failed or outside of size range.
            void* try_allocate(size_t size)
            {
                return owns(nullptr, size) ? Impl::try_allocate(size) : nullptr;
            }

            // Allocates a memory block large enough to hold `size` number of bytes.
            //
            // \returns Address to memory block of allocated memory or nullptr if failed or outside of size range
            void* allocate(size_t size)
            {
                return owns(nullptr, size) ? Impl::allocate(size) : nullptr;
            }

            // Reallocates previously allocated or reallocated memory pointed to by `ptr` from `old_size` to `new_size` number of bytes if reallocation does not
            // require increasing capacity. Passing `nullptr` in `ptr` yield the same result as calling `try_allocate`.
            //
            // \returns Address to memory block of reallocated memory or nullptr if failed or if `new_size` is outside of size range.
            void* try_reallocate(void* ptr, size_t old_size, size_t new_size)
            {
                return owns(nullptr, new_size) ? Impl::try_reallocate(ptr, old_size, new_size) : nullptr;
            }

            // Reallocates previously allocated or reallocated memory pointed to by `ptr` from `old_size` to `new_size` number of bytes.
            // Passing `nullptr` in `ptr` yield the same result as calling `allocate`.
            //
            // \returns Address to memory block of reallocated memory or nullptr if failed or if `new_size` is outside of size range
            void* reallocate(void* ptr, size_t old_size, size_t new_size)
            {
                return owns(nullptr, new_size) ? Impl::reallocate(ptr, old_size, new_size) : nullptr;
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            // Passing `nullptr` in `ptr` result in a no-op.
            //
            // \returns Always returns `true`
            bool deallocate(void* ptr, size_t size)
            {
                return Impl::deallocate(ptr, size);
            }

            // Free a linked list of allocations created using `batch_deallocate_link` with `size`.
            // `first` to `last` is first and last allocation of a `batch_deallocate_link` series of calls.
            //
            // \returns Always returns `true`
            bool batch_deallocate(void* ptr_first, void* ptr_last, size_t size)
            {
                return Impl::batch_deallocate(ptr_first, ptr_last, size);
            }

            // Link previously allocated memory of `size` to another.
            //
            // Use to create a linked list of allocations for use with `batch_deallocate(first, last, size)`
            // Size of linked allocations are required to be equal to `size`.
            // `nullptr` is a valid argument for `ptr_next`, but is not needed to terminate a linked list.
            // This is implicit transfer of ownership of the memory back to the allocator.
            // Memory of the allocation must not be accessed/modified once linked.
            void batch_deallocate_link(void* ptr, void* ptr_next, size_t size)
            {
                Impl::batch_deallocate_link(ptr, ptr_next);
            }

            // Release all resources and set capacity to zero
            //
            // Calling this function invalidates any currently allocated memory
            // If there are other threads currently accessing the allocator behavior is undefined.
            void deallocate_all()
            {
                Impl::deallocate_all();
            }

            // Requests that the allocator capacity be at least enough to contain `capacity` for allocations of `size`.
            //
            // If `capacity` is less or equal to current capacity for allocations of `size`, the capacity is not affected.
            // Note that internally, `capacity` is rounded up to the nearest optimal allocation size based on `Allocator` attributes.
            //
            // \returns true if successful.
            bool reserve(size_t size, size_t capacity)
            {
                return owns(nullptr, size) ? Impl::reserve(size, capacity) : false;
            }

            // Get the current capacity of allocations with `size`.
            size_t capacity(size_t size)
            {
                return owns(nullptr, size) ? Impl::capacity(size) : 0;
            }

            // Calculate optimal allocation size given `size`.
            //
            // \returns Optimal size when allocating memory given `size` or zero if outside size range.
            static constexpr size_t optimal_size(const size_t size)
            {
                return owns(nullptr, size) ? Impl::optimal_size(size) : 0;
            }

            // Checks for the ownership allocation given `ptr` and `size`
            // It is implementation defined if either or both of `ptr` and `size` are considered to determine ownership.
            // This allocator does not consider `ptr`.
            //
            // \returns True if the allocator owns the allocation.
            static constexpr bool owns(const void *, size_t size)
            {
                return size - min_size <= max_size - min_size;
            }
        };
    }
}
