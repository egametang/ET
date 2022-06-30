#pragma once

#include "../../C/Baselib_Memory.h"

// Internal, to enable override of default C Api implementation for unit-tests
#ifndef detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL
#define detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL 0
#endif

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace detail
        {
            // Default memory allocation methods
            struct heap_allocator_impl
            {
                static constexpr auto Baselib_Memory_Allocate = ::Baselib_Memory_Allocate;
                static constexpr auto Baselib_Memory_Reallocate = ::Baselib_Memory_Reallocate;
                static constexpr auto Baselib_Memory_Free = ::Baselib_Memory_Free;
                static constexpr auto Baselib_Memory_AlignedAllocate = ::Baselib_Memory_AlignedAllocate;
                static constexpr auto Baselib_Memory_AlignedReallocate = ::Baselib_Memory_AlignedReallocate;
                static constexpr auto Baselib_Memory_AlignedFree = ::Baselib_Memory_AlignedFree;
            };

            // Test memory allocation methods
            struct heap_allocator_impl_test
            {
                static void* Baselib_Memory_Allocate(size_t);
                static void* Baselib_Memory_Reallocate(void*, size_t);
                static void  Baselib_Memory_Free(void*);
                static void* Baselib_Memory_AlignedAllocate(size_t, size_t);
                static void* Baselib_Memory_AlignedReallocate(void*, size_t, size_t);
                static void  Baselib_Memory_AlignedFree(void*);
            };

            template<uint32_t alignment>
            class heap_allocator
            {
                // Use test memory allocation implementation if detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL is true, otherwise Baselib_Memory_*
                using BaseImpl = typename std::conditional<detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL, heap_allocator_impl_test, heap_allocator_impl>::type;

                // Memory allocation functions - alignment requirements <= Baselib_Memory_MinGuaranteedAlignment
                struct MinAlignedImpl
                {
                    static void* allocate(size_t size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        return BaseImpl::Baselib_Memory_Allocate(size);
                    }

                    static void* reallocate(void* ptr, size_t old_size, size_t new_size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        UNUSED(old_size);
                        return BaseImpl::Baselib_Memory_Reallocate(ptr, new_size);
                    }

                    static bool deallocate(void* ptr, size_t size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        UNUSED(size);
                        BaseImpl::Baselib_Memory_Free(ptr);
                        return true;
                    }
                };

                // Aligned memory allocation functions - alignment requirements > Baselib_Memory_MinGuaranteedAlignment
                struct AlignedImpl
                {
                    static void* allocate(size_t size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        return BaseImpl::Baselib_Memory_AlignedAllocate(size, alignment);
                    }

                    static void* reallocate(void* ptr, size_t old_size, size_t new_size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        UNUSED(old_size);
                        return BaseImpl::Baselib_Memory_AlignedReallocate(ptr, new_size, alignment);
                    }

                    static bool deallocate(void* ptr, size_t size, Baselib_ErrorState *error_state_ptr)
                    {
                        UNUSED(error_state_ptr);
                        UNUSED(size);
                        BaseImpl::Baselib_Memory_AlignedFree(ptr);
                        return true;
                    }
                };

                static FORCE_INLINE constexpr size_t AlignedSize(size_t size)
                {
                    return (size + alignment - 1) & ~(alignment - 1);
                }

            public:
                static constexpr size_t max_alignment = Baselib_Memory_MaxAlignment;

                static constexpr size_t optimal_size(size_t size)
                {
                    return AlignedSize(size);
                }

                // Use aligned memory allocations methods if alignment > Baselib_Memory_MinGuaranteedAlignment
                using Impl = typename std::conditional<(alignment > Baselib_Memory_MinGuaranteedAlignment), AlignedImpl, MinAlignedImpl>::type;

                static void* allocate(size_t size, Baselib_ErrorState* error_state_ptr)
                {
                    return Impl::allocate(size, error_state_ptr);
                }

                static void* reallocate(void* ptr, size_t old_size, size_t new_size, Baselib_ErrorState* error_state_ptr)
                {
                    return Impl::reallocate(ptr, old_size, new_size, error_state_ptr);
                }

                static bool deallocate(void* ptr, size_t size, Baselib_ErrorState* error_state_ptr)
                {
                    return Impl::deallocate(ptr, size, error_state_ptr);
                }
            };
        }
    }
}

#undef detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL
