#pragma once

namespace il2cpp
{
namespace os
{
namespace Memory
{
    void* AlignedAlloc(size_t size, size_t alignment);
    void* AlignedReAlloc(void* memory, size_t newSize, size_t alignment);
    void AlignedFree(void* memory);
}
}
}
