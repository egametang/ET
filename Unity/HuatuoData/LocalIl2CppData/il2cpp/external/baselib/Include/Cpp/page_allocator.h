#pragma once

#include "Internal/page_allocator.inl.h"

namespace baselib
{
    BASELIB_CPP_INTERFACE
    {
        // Page allocator implementation providing platform dependent system page allocation.
        //
        // Allocations are guaranteed to be aligned to at least the value of `default_alignment`.
        // All methods with no page state parameter input will default to `default_page_state` where applicable.
        //
        // Notes on allocation size:
        //  All sizes are by allocator standards in bytes. The page allocator internally rounds up sizes to the nearest page size value. Consider this when
        //  allocating. Use `optimal_size` to retreive number of bytes allocated given a specific size (1 to retreive the page size value).
        //  Large alignments may lead to a significantly higher use of virtual address space than the amount of memory requested.
        //  This may result in an aligned page allocation to fail where a less/non-aligned allocation would succeed.
        //  Note that this is especially common in 32bit applications but a platform may impose additional restrictions on the size of its virtual address space.
        //  Whether a page allocation is pure virtual address space or already commited memory depends on the platform and passed page state flag.
        //

        // Page state options
        typedef enum Memory_PageState
        {
            // The page are in a reserved state and any access will cause a seg-fault/access violation.
            // On some platforms that support this state this may be just a hint to the OS and there is no guarantee pages in this state behave
            // differently the `NoAccess` state.
            // The `page_allocator` implementation does a best effort and tries to ensure as best as possible that pages in this state are not commited.
            Memory_PageState_Reserved             = detail::Memory_PageState_Reserved,
            // This is a no access page and will cause a seg-fault/access violation when accessed.
            Memory_PageState_NoAccess             = detail::Memory_PageState_NoAccess,
            // The memory can only be read.
            Memory_PageState_ReadOnly             = detail::Memory_PageState_ReadOnly,
            // The memory can be read and written.
            Memory_PageState_ReadWrite            = detail::Memory_PageState_ReadWrite,
            // The memory can be used to execute code and can be read.
            Memory_PageState_ReadOnly_Executable  = detail::Memory_PageState_ReadOnly_Executable,
            // The memory can be used to execute code and can be both read and written.
            Memory_PageState_ReadWrite_Executable = detail::Memory_PageState_ReadWrite_Executable,
        } Memory_PageState;

        // Allocator
        template<uint32_t default_alignment = 4096, Memory_PageState default_page_state = Memory_PageState_ReadWrite>
        class page_allocator
        {
            static_assert((default_alignment != 0), "'default_alignment' must not be zero");
            static_assert(::baselib::Algorithm::IsPowerOfTwo(default_alignment), "'default_alignment' must be a power of two value");

            using impl = detail::page_allocator<default_alignment>;
            const impl m_Impl;

        public:
            // Allocated memory is guaranteed to always be aligned to at least the value of `alignment`.
            static constexpr uint32_t alignment = default_alignment;

            // Typedefs
            typedef Baselib_ErrorState error_state;

            // Create a new instance with system default page size.
            page_allocator() : m_Impl() {}

            // Create a new instance with `page_size` sized pages. Page size is required to be supported by the target system.
            page_allocator(size_t page_size) : m_Impl(page_size)
            {
                BaselibAssert((page_size != 0), "'page_size' must not be a zero value");
                BaselibAssert(::baselib::Algorithm::IsPowerOfTwo(page_size), "'page_size' must be a power of two value");
            }

            // Allocates number of pages required to hold `size` number of bytes, with initial page state set to `state`
            //
            // \returns Address to memory block of allocated memory or `nullptr` if allocation failed.
            void* allocate(size_t size, Memory_PageState state = default_page_state) const
            {
                error_state result = Baselib_ErrorState_Create();
                return allocate(size, state, &result);
            }

            // Allocates number of pages required to hold `size` number of bytes, with initial page state set to `state`
            //
            // If operation failed `error_state_ptr` contains one of the following error codes:
            // - Baselib_ErrorCode_InvalidPageSize:         Page size doesn't match any of the available page sizes.
            // - Baselib_ErrorCode_InvalidPageCount:        Requested number of pages is zero.
            // - Baselib_ErrorCode_UnsupportedAlignment:    Requested alignment is invalid.
            // - Baselib_ErrorCode_UnsupportedPageState:    The underlying system doesn't support the requested page state.
            // - Baselib_ErrorCode_OutOfMemory:             If there is not enough continuous address space available, or physical memory space when acquiring committed memory.
            //
            // \returns Address to memory block of allocated memory or `nullptr` if allocation failed.
            void* allocate(size_t size, Memory_PageState state, error_state *error_state_ptr) const
            {
                return m_Impl.allocate(size, state, error_state_ptr);
            }

            // Reallocate is not supported by the page allocator. The operation is a no-op.
            //
            // If `error_state_ptr` is passed it contains the following error code:
            // - Baselib_ErrorCode_NotSupported:    The operation is not supported by the underlying system.
            //
            // \returns Always returns `nullptr`.
            void* reallocate(void* ptr, size_t old_size, size_t new_size, error_state *error_state_ptr = nullptr) const
            {
                if (error_state_ptr)
                    *error_state_ptr |= RaiseError(Baselib_ErrorCode_NotSupported);
                return nullptr;
            }

            // Deallocates memory block in previously allocated or reallocated with `size` pointed to by `ptr`.
            // A single call of deallocate must encompass the size that were originally allocated with a single call of `allocate`.
            //
            // \returns True if the operation was successful.
            bool deallocate(void* ptr, size_t size) const
            {
                error_state result = Baselib_ErrorState_Create();
                return deallocate(ptr, size, &result);
            }

            // Deallocates memory block previously allocated or reallocated with `size` pointed to by `ptr`.
            // A single call of deallocate must encompass the size that were originally allocated with a single call of `allocate`.
            //
            // If operation failed `error_state_ptr` contains one of the following error codes:
            // - Baselib_ErrorCode_InvalidAddressRange: Address range was detected to not match a valid allocation.
            //                                          CAUTION: Not all platforms are able to detect this and may either raise an error or cause undefined behavior.
            //                                          Note to implementors: Raising the error is strongly preferred as it helps identifying issues in user code.
            // - Baselib_ErrorCode_InvalidPageSize:     If page size doesn't match size with previous call to `allocate` with address in `ptr`.
            //
            // \returns True if the operation was successful.
            bool deallocate(void* ptr, size_t size, error_state *error_state_ptr) const
            {
                return m_Impl.deallocate(ptr, size, error_state_ptr);
            }

            // Calculate optimal allocation size given `size`.
            // The result size is the number of bytes allocated given a specific size.
            //
            // \returns Optimal size when allocating memory given `size`.
            constexpr size_t optimal_size(size_t size) const
            {
                return m_Impl.optimal_size(size);
            }

            // Modifies the page state property of an already allocated virtual address in `ptr` of `size` to `state`.
            // It is possible to modify only some of the memory allocated by `allocate`.
            // Address is the address of the first page to modify and so must be aligned to size of page size.
            // Size is rounded up to the next multiple of page size used.
            // Passing `nullptr` or a zero page count result in a no-op.
            //
            // \returns True if the operation was successful.
            bool set_page_state(void* ptr, size_t size, Memory_PageState state) const
            {
                error_state result = Baselib_ErrorState_Create();
                return set_page_state(ptr, size, state, &result);
            }

            // Modifies the page state property of an already allocated virtual address in `ptr` of `size` to `state`.
            // It is possible to modify only some of the memory allocated by `allocate`.
            // Address is the address of the first page to modify and so must be aligned to size of page size.
            // Size is rounded up to the next multiple of page size used.
            // Passing `nullptr` or a zero page count result in a no-op.
            //
            // If operation failed `error_state_ptr` contains one of the following error codes:
            // - Baselib_ErrorCode_InvalidAddressRange:     Address range is not covered by a valid allocation.
            //                                              Platforms that emulate page allocations (e.g. Emscripten) are not able to present this error and
            //                                              will pass the function call silently.
            // - Baselib_ErrorCode_InvalidPageSize:         If page size doesn't match the previous allocation in `ptr`.
            // - Baselib_ErrorCode_UnsupportedPageState:    The underlying system doesn't support the requested page state.
            //
            // \returns True if the operation was successful.
            bool set_page_state(void* ptr, size_t size, Memory_PageState state, error_state *error_state_ptr) const
            {
                return m_Impl.set_page_state(ptr, size, state, error_state_ptr);
            }
        };
    }
}
