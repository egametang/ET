#pragma once

#include "../../C/Baselib_Memory.h"
#include "../../Cpp/Algorithm.h"

// Internal, to enable override of default C Api implementation for unit-tests
#ifndef detail_BASELIB_PAGE_ALLOCATOR_TEST_IMPL
#define detail_BASELIB_PAGE_ALLOCATOR_TEST_IMPL 0
#endif

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        namespace detail
        {
            // Default memory allocation methods
            struct page_allocator_impl
            {
                static constexpr auto Baselib_Memory_AllocatePages = ::Baselib_Memory_AllocatePages;
                static constexpr auto Baselib_Memory_ReleasePages = ::Baselib_Memory_ReleasePages;
                static constexpr auto Baselib_Memory_SetPageState = ::Baselib_Memory_SetPageState;
            };

            // Test memory allocation methods
            struct page_allocator_impl_test
            {
                static Baselib_Memory_PageAllocation Baselib_Memory_AllocatePages(uint64_t pageSize, uint64_t pageCount, uint64_t alignmentInMultipleOfPageSize, Baselib_Memory_PageState pageState, Baselib_ErrorState* errorState);
                static void Baselib_Memory_ReleasePages(Baselib_Memory_PageAllocation pageAllocation, Baselib_ErrorState* errorState);
                static void Baselib_Memory_SetPageState(void* addressOfFirstPage, uint64_t pageSize, uint64_t pageCount, Baselib_Memory_PageState pageState, Baselib_ErrorState* errorState);
            };

            typedef enum Memory_PageState : int
            {
                Memory_PageState_Reserved             = Baselib_Memory_PageState_Reserved,
                Memory_PageState_NoAccess             = Baselib_Memory_PageState_NoAccess,
                Memory_PageState_ReadOnly             = Baselib_Memory_PageState_ReadOnly,
                Memory_PageState_ReadWrite            = Baselib_Memory_PageState_ReadWrite,
                Memory_PageState_ReadOnly_Executable  = Baselib_Memory_PageState_ReadOnly_Executable | Baselib_Memory_PageState_ReadOnly,
                Memory_PageState_ReadWrite_Executable = Baselib_Memory_PageState_ReadWrite_Executable | Baselib_Memory_PageState_ReadWrite,
            } Memory_PageState;

            template<uint32_t alignment>
            class page_allocator
            {
                // Use test memory allocation implementation if detail_BASELIB_HEAP_ALLOCATOR_TEST_IMPL is true
                using Impl = typename std::conditional<detail_BASELIB_PAGE_ALLOCATOR_TEST_IMPL, page_allocator_impl_test, page_allocator_impl>::type;

                const size_t m_PageSize;
                const size_t m_PageSizeAligned;

                FORCE_INLINE constexpr size_t PagedCountFromSize(size_t size) const
                {
                    return (size + (m_PageSize - 1)) / m_PageSize;
                }

                FORCE_INLINE size_t DefaultPageSize() const
                {
                    Baselib_Memory_PageSizeInfo info;
                    Baselib_Memory_GetPageSizeInfo(&info);
                    return static_cast<size_t>(info.defaultPageSize);
                }

            public:
                page_allocator() : page_allocator(DefaultPageSize()) {}
                page_allocator(size_t page_size) : m_PageSize(page_size), m_PageSizeAligned(page_size > alignment ? page_size : alignment) {}

                void* allocate(size_t size, int state, Baselib_ErrorState *error_state_ptr) const
                {
                    Baselib_Memory_PageAllocation pa = Impl::Baselib_Memory_AllocatePages(m_PageSize, PagedCountFromSize(size), m_PageSizeAligned / m_PageSize, (Baselib_Memory_PageState)state, error_state_ptr);
                    return pa.ptr;
                }

                bool deallocate(void* ptr, size_t size, Baselib_ErrorState *error_state_ptr) const
                {
                    Impl::Baselib_Memory_ReleasePages({ptr, m_PageSize, PagedCountFromSize(size)}, error_state_ptr);
                    return (error_state_ptr->code == Baselib_ErrorCode_Success);
                }

                constexpr size_t optimal_size(size_t size) const
                {
                    return (size + m_PageSizeAligned - 1) & ~(m_PageSizeAligned - 1);
                }

                bool set_page_state(void* ptr, size_t size, int state, Baselib_ErrorState *error_state_ptr) const
                {
                    Impl::Baselib_Memory_SetPageState(ptr, m_PageSize, PagedCountFromSize(size), (Baselib_Memory_PageState)state, error_state_ptr);
                    return (error_state_ptr->code == Baselib_ErrorCode_Success);
                }
            };
        }
    }
}

#undef detail_BASELIB_PAGE_ALLOCATOR_TEST_IMPL
