#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS && !(IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE)

#include "os/MarshalAlloc.h"

#include "WindowsHeaders.h"
#include "Objbase.h"

namespace il2cpp
{
namespace os
{
    void* MarshalAlloc::Allocate(size_t size)
    {
        return ::CoTaskMemAlloc(size);
    }

    void* MarshalAlloc::ReAlloc(void* ptr, size_t size)
    {
        return ::CoTaskMemRealloc(ptr, size);
    }

    void MarshalAlloc::Free(void* ptr)
    {
        ::CoTaskMemFree(ptr);
    }

    void* MarshalAlloc::AllocateHGlobal(size_t size)
    {
        return ::GlobalAlloc(GMEM_FIXED, size);
    }

    void* MarshalAlloc::ReAllocHGlobal(void* ptr, size_t size)
    {
        return ::GlobalReAlloc(ptr, size, GMEM_MOVEABLE);
    }

    void MarshalAlloc::FreeHGlobal(void* ptr)
    {
        ::GlobalFree(ptr);
    }
} /* namespace os */
} /* namespace il2cpp*/

#endif
