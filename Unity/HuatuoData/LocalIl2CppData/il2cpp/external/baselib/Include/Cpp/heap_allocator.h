#pragma once

#include "Internal/heap_allocator.inl.h"
#include "Algorithm.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Heap allocator implementation providing platform dependent system heap allocation.
        //
        // Allocations are guaranteed to be aligned to at least the value of `default_alignment`.
        // For optimal performance, platform aligned allocation calls are only used when `default_alignment` exceeds platform minimum alignment guarantee.
        // This allocator is a stateless allocator (empty class).
        //
        // Notes on operation failure of allocator methods:
        //  Operation failures will currently trigger process abort by the underlying system.
        //  As a result the heap allocator currently will never return `nullptr`/`false` to signal failure, as is standard behaviour (nor any error state information).
        //
        template<uint32_t default_alignment = 8>
        class heap_allocator
        {
            using impl = detail::heap_allocator<default_alignment>;
            static_assert((default_alignment <= impl::max_alignment), "'default_alignment' exceeded max value");
            static_assert((default_alignment != 0), "'default_alignment' must not be a zero value");
            static_assert(::baselib::Algorithm::IsPowerOfTwo(default_alignment), "'default_alignment' must be a power of two value");

        public:
            // Allocated memory is guaranteed to always be aligned to at least the value of `alignment`.
            static constexpr uint32_t alignment = default_alignment;

            // Typedefs
            typedef Baselib_ErrorState error_state;

            // Allocates a memory block large enough to hold `size` number of bytes. Zero size is valid.
            //
            // \returns Address to memory block of allocated memory.
            void* allocate(size_t size) const
            {
                error_state result = Baselib_ErrorState_Create();
                return impl::allocate(size, &result);
            }

            // Allocates a memory block large enough to hold `size` number of bytes. Zero size is valid.
            //
            // \returns Address to memory block of allocated memory.
            void* allocate(size_t size, error_state *error_state_ptr) const
            {
                return impl::allocate(size, error_state_ptr);
            }

            // Reallocates previously allocated or reallocated memory block pointer reference `ptr` from `old_size` to `new_size` number of bytes.
            // Passing `nullptr` in `ptr` yield the same result as calling `allocate`.
            //
            // \returns Address to memory block of reallocated memory.
            void* reallocate(void* ptr, size_t old_size, size_t new_size) const
            {
                error_state result = Baselib_ErrorState_Create();
                return impl::reallocate(ptr, old_size, new_size, &result);
            }

            // Reallocates previously allocated or reallocated memory block pointer reference `ptr` from `old_size` to `new_size` number of bytes.
            // Passing `nullptr` in `ptr` yield the same result as calling `allocate`.
            //
            // \returns Address to memory block of reallocated memory.
            void* reallocate(void* ptr, size_t old_size, size_t new_size, error_state *error_state_ptr) const
            {
                return impl::reallocate(ptr, old_size, new_size, error_state_ptr);
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            // Passing `nullptr` in `ptr` result in a no-op.
            //
            // \returns Always returns `true` (see notes on operation failure).
            bool deallocate(void* ptr, size_t size) const
            {
                error_state result = Baselib_ErrorState_Create();
                return impl::deallocate(ptr, size, &result);
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            // Passing `nullptr` in `ptr` result in a no-op.
            //
            // \returns Always returns `true` (see notes on operation failure).
            bool deallocate(void* ptr, size_t size, error_state *error_state_ptr) const
            {
                return impl::deallocate(ptr, size, error_state_ptr);
            }

            // Calculate optimal allocation size given `size`.
            //
            // \returns Optimal size when allocating memory given `size`.
            constexpr size_t optimal_size(size_t size) const
            {
                return impl::optimal_size(size);
            }
        };
    }
}
