#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX || IL2CPP_TARGET_SWITCH && !RUNTIME_TINY

#include "os/MarshalAlloc.h"
#include <stdlib.h>

namespace il2cpp
{
namespace os
{
    void* MarshalAlloc::Allocate(size_t size)
    {
        return malloc(size);
    }

    void* MarshalAlloc::ReAlloc(void* ptr, size_t size)
    {
        return realloc(ptr, size);
    }

    void MarshalAlloc::Free(void* ptr)
    {
        free(ptr);
    }

    void* MarshalAlloc::AllocateHGlobal(size_t size)
    {
        return malloc(size);
    }

    void* MarshalAlloc::ReAllocHGlobal(void* ptr, size_t size)
    {
        return realloc(ptr, size);
    }

    void MarshalAlloc::FreeHGlobal(void* ptr)
    {
        free(ptr);
    }
} /* namespace os */
} /* namespace il2cpp*/

#endif
