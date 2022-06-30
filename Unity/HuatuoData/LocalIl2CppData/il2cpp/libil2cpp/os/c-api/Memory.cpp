#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/Memory-c-api.h"
#include "os/Memory.h"

extern "C"
{
    void* UnityPalAlignedAlloc(size_t size, size_t alignment)
    {
        return il2cpp::os::Memory::AlignedAlloc(size, alignment);
    }

    void* UnityPalAlignedReAlloc(void* memory, size_t newSize, size_t alignment)
    {
        return il2cpp::os::Memory::AlignedReAlloc(memory, newSize, alignment);
    }

    void UnityPalAlignedFree(void* memory)
    {
        il2cpp::os::Memory::AlignedFree(memory);
    }
}

#endif
