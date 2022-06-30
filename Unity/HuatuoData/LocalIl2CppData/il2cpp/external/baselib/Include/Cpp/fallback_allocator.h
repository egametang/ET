#pragma once

#include <type_traits>
#include <algorithm>

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Baselib fallback allocator implementation with baselib allocators method coverage.
        // If the `Primary` allocator fail to allocate the request it's passed to the `Fallback` Allocator.
        //
        // The fallback allocator purpose is to provide a template for implementation using an allocator composition approach.
        // While providing for the baselib allocators interface(s), it's not intended to be a turn-key, general purpose solution, but rather
        // act as a template building block for derived allocators which may extend, add or ignore methods for specific needs.
        //
        // As a rule of thumb, Both Primary and Secondary allocator method calls may fail depending on their specific implementation.
        // What (if any) action is to be taken in such cases is intentionally left to be implemented by the derived class.
        //
        template<class Primary, class Fallback>
        class fallback_allocator : protected Primary, protected Fallback
        {
        public:
            // Allocations are guaranteed to always be aligned to at least the value of `alignment`
            // Alignment is the minimal value of Primary and Fallback allocator alignment, which is what can be guaranteed.
            static constexpr unsigned alignment = (Primary::alignment < Fallback::alignment) ? Primary::alignment : Fallback::alignment;

            // Allocates a memory block large enough to hold `size` number of bytes.
            //
            // \returns Address to memory block of allocated memory or nullptr if allocation failed.
            void* allocate(size_t size)
            {
                void *ptr = Primary::allocate(size);
                if (ptr == nullptr)
                    ptr = Fallback::allocate(size);
                return ptr;
            }

            // Reallocates previously allocated or reallocated memory block pointer reference `ptr` from `old_size` to `new_size` number of bytes.
            // Reallocation will fail if the ownership of the new allocation can't be preserved.
            //
            // \returns Address to memory block of reallocated memory or nullptr if reallocation failed.
            void* reallocate(void* ptr, size_t old_size, size_t new_size)
            {
                if (Primary::owns(ptr, old_size))
                    return Primary::reallocate(ptr, old_size, new_size);
                return Fallback::reallocate(ptr, old_size, new_size);
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            //
            // \returns True if the operation was successful.
            bool deallocate(void* ptr, size_t size)
            {
                if (Primary::owns(ptr, size))
                    return Primary::deallocate(ptr, size);
                return Fallback::deallocate(ptr, size);
            }

            // Calculate optimal allocation size of the primary allocator given `size`.
            //
            // \returns Optimal size of the primary allocator when allocating memory given `size`.
            constexpr size_t optimal_size(size_t size) const
            {
                return Primary::optimal_size(size);
            }

            // Checks for the ownership allocation given `ptr` and `size`
            // It is implementation defined if either or both of `ptr` and `size` are considered to determine ownership.
            //
            // \returns True if the primary allocator owns the allocation.
            bool owns(const void* ptr, size_t size) const
            {
                return Primary::owns(ptr, size);
            }
        };
    }
}
