#pragma once

#include <type_traits>
#include "Algorithm.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Baselib affix allocator implementation providing optional prefix and suffix memory regions in addition to requested size.
        //
        // The affix allocator purpose is to provide memory regions directly adjacent to allocated memory of requested size and alignment.
        // It is not intended to be a turn-key, general purpose solution, but rather act as a template building block for derived allocators which may extend,
        // add or ignore methods for specific needs.
        //
        // Allocation methods allocate, reallocate and deallocate are using the `Allocator` implementation for memory allocation, as are alignment properties.
        // As a rule of thumb, Allocator method calls may fail depending on their specific implementation.
        // What (if any) action is to be taken in such cases is intentionally left to be implemented by the derived class.
        //
        // No operations, synchronisation  or alignment concept are applied to the prefix or suffix memory.
        // Prefix memory address is obtained using the `prefix` function and  is always allocated memory pointer minus prefix_size (ptr - prefix_size).
        // Suffix memory address is obtained using the `suffix` function and  is always directly adjacent to the end of allocated memory (ptr + size).
        //
        // Notes on memory footprint:
        // Internally allocated memory must be large enough to hold requested allocation size, prefix_size, suffix_size and alignment padding.
        // The internally allocated size is calculated as follows: size + suffix_size + (prefix_size rounded up to alignment).
        // If alignment padding is significant, it may be preferable to use a suffix over a prefix to reduce memory footprint.
        //
        template<class Allocator, size_t prefix_size, size_t suffix_size>
        class affix_allocator : protected Allocator
        {
        public:
            // Allocated memory is guaranteed to always be aligned to at least the value of `alignment`.
            static constexpr uint32_t alignment = Allocator::alignment;

            // Allocates a memory block large enough to hold `size` number of bytes. Zero size is valid.
            //
            // \returns Address to memory block of allocated memory.
            void* allocate(size_t size)
            {
                return OffsetPtrChecked(Allocator::allocate(size + m_AffixSize), m_PrefixAlignedSize);
            }

            // Reallocates previously allocated or reallocated memory block pointer reference `ptr` from `old_size` to `new_size` number of bytes.
            // Passing `nullptr` in `ptr` yield the same result as calling `allocate`.
            // If `suffix_size` is non-zero, the suffix memory is moved to the new location.
            //
            // \returns Address to memory block of reallocated memory.
            void* reallocate(void* ptr, size_t old_size, size_t new_size)
            {
                return ptr == nullptr ? allocate(new_size) : ReallocateImpl(ptr, old_size, new_size);
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            // Passing `nullptr` in `ptr` result in a no-op.
            //
            // \returns Always returns `true` (see notes on operation failure).
            bool deallocate(void* ptr, size_t size)
            {
                return Allocator::deallocate(OffsetPtr(ptr, -m_PrefixAlignedSize), size + m_AffixSize);
            }

            // Calculate optimal allocation of size of `Allocator` allocator given `size`.
            //
            // \returns Optimal size of allocations when allocating memory given `size`.
            constexpr size_t optimal_size(size_t size) const
            {
                return Allocator::optimal_size(size);
            }

            // Get prefix memory block address of allocation pointed to by `ptr`.
            // Memory must be a valid allocation from `allocate` or `reallocate`, or result is undefined.
            //
            // \returns Prefix memory address or nullptr if `prefix_size` is zero.
            void* prefix(void* ptr) const
            {
                return prefix_size == 0 ? nullptr : OffsetPtr(ptr, -static_cast<ptrdiff_t>(prefix_size));
            }

            // Get suffix memory block address of allocation with `size` pointed to by `ptr`.
            // Memory must be a valid allocation from `allocate` or `reallocate`, or result is undefined.
            //
            // \returns Suffix memory address or nullptr if `suffix_size` is zero.
            void* suffix(void* ptr, size_t size) const
            {
                return suffix_size == 0 ? nullptr : OffsetPtr(ptr, size);
            }

        private:
            static constexpr size_t AlignSize(size_t size) { return (size + Allocator::alignment - 1) & ~(Allocator::alignment - 1); }

            static FORCE_INLINE constexpr void *OffsetPtrChecked(const void *ptr, size_t offset) { return ptr == nullptr ? nullptr : OffsetPtr(ptr, offset); }
            static FORCE_INLINE constexpr void *OffsetPtr(const void *ptr, size_t offset)
            {
                return reinterpret_cast<void *>(reinterpret_cast<uintptr_t>(ptr) + offset);
            }

            template<size_t value = suffix_size, typename std::enable_if<value == 0, bool>::type = 0>
            FORCE_INLINE void* ReallocateImpl(void* ptr, size_t old_size, size_t new_size)
            {
                return OffsetPtrChecked(Allocator::reallocate(OffsetPtr(ptr, -m_PrefixAlignedSize), old_size + m_PrefixAlignedSize, new_size + m_PrefixAlignedSize), m_PrefixAlignedSize);
            }

            template<size_t value = suffix_size, typename std::enable_if<value != 0, bool>::type = 0>
            FORCE_INLINE void* ReallocateImpl(void* ptr, size_t old_size, size_t new_size)
            {
                uint8_t tmpSuffix[m_SuffixSize];
                memcpy(tmpSuffix, suffix(ptr, old_size), m_SuffixSize);
                ptr = Allocator::reallocate(OffsetPtr(ptr, -m_PrefixAlignedSize), old_size + m_AffixSize, new_size + m_AffixSize);
                if (ptr)
                {
                    ptr = OffsetPtr(ptr, m_PrefixAlignedSize);
                    memcpy(suffix(ptr, new_size), tmpSuffix, m_SuffixSize);
                }
                return ptr;
            }

            static constexpr ptrdiff_t m_PrefixAlignedSize = AlignSize(prefix_size);
            static constexpr ptrdiff_t m_SuffixSize = suffix_size;
            static constexpr ptrdiff_t m_AffixSize = m_PrefixAlignedSize + m_SuffixSize;
        };
    }
}
