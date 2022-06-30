#include "il2cpp-config.h"

#if IL2CPP_SANITIZE_ADDRESS

#include "utils/MemoryPoolAddressSanitizer.h"

namespace il2cpp
{
namespace utils
{
    MemoryPoolAddressSanitizer::MemoryPoolAddressSanitizer()
    {
    }

    MemoryPoolAddressSanitizer::MemoryPoolAddressSanitizer(size_t initialSize)
    {
    }

    MemoryPoolAddressSanitizer::~MemoryPoolAddressSanitizer()
    {
        for (auto allocation : m_Allocations)
            free(allocation);

        m_Allocations.clear();
    }

    void* MemoryPoolAddressSanitizer::Malloc(size_t size)
    {
        void* allocation = malloc(size);
        m_Allocations.push_back(allocation);
        return allocation;
    }

    void* MemoryPoolAddressSanitizer::Calloc(size_t count, size_t size)
    {
        void* allocation = calloc(count, size);
        m_Allocations.push_back(allocation);
        return allocation;
    }
}
}

#endif
