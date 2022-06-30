#pragma once
#include "il2cpp-config.h"

#if !IL2CPP_SANITIZE_ADDRESS
#error MemoryPoolAddressSanitizer should only be used when the address sanitizer is enabled
#endif

#include <vector>

namespace il2cpp
{
namespace utils
{
    // Use system allocators with the address sanitizer, so that it can catch
    // problems with memory access that might happen when our memory pool is
    // used incorrectly.
    class MemoryPoolAddressSanitizer
    {
    public:
        MemoryPoolAddressSanitizer();
        MemoryPoolAddressSanitizer(size_t initialSize);
        ~MemoryPoolAddressSanitizer();
        void* Malloc(size_t size);
        void* Calloc(size_t count, size_t size);
    private:
        std::vector<void*> m_Allocations;
    };
} /* namespace utils */
} /* namespace il2cpp */
