#include "il2cpp-config.h"
#include "utils/MemoryPool.h"
#include "utils/Memory.h"
#include <algorithm>
#include <limits>

namespace il2cpp
{
namespace utils
{
    const size_t kPageSize = IL2CPP_PAGE_SIZE;
    const size_t kDefaultRegionSize = 16 * 1024;
// by making all allocations a multiple of this value, we ensure the next
// allocation will always be aligned to this value
    const size_t kMemoryAlignment = 8;

    static inline size_t MakeMultipleOf(size_t size, size_t alignment)
    {
        return (size + alignment - 1) & ~(alignment - 1);
    }

    struct MemoryPool::Region
    {
        char* start;
        char* current;
        size_t size;
        size_t free;
    };

    MemoryPool::MemoryPool()
    {
        AddRegion(kDefaultRegionSize);
    }

    MemoryPool::MemoryPool(size_t initialSize)
    {
        AddRegion(initialSize);
    }

    MemoryPool::~MemoryPool()
    {
        for (RegionList::iterator iter = m_Regions.begin(); iter != m_Regions.end(); ++iter)
        {
            IL2CPP_FREE((*iter)->start);
            IL2CPP_FREE(*iter);
        }

        m_Regions.clear();
    }

    void* MemoryPool::Malloc(size_t size)
    {
        size = MakeMultipleOf(size, kMemoryAlignment);

        Region* region = m_Regions.back();
        if (region->free < size)
            region = AddRegion(size);

        IL2CPP_ASSERT(region->free >= size);

        void* value = region->current;
        region->current += size;
        region->free -= size;

        return value;
    }

    void* MemoryPool::Calloc(size_t count, size_t size)
    {
        void* ret = Malloc(count * size);
        return memset(ret, 0, count * size);
    }

    MemoryPool::Region* MemoryPool::AddRegion(size_t size)
    {
        Region* region = (Region*)IL2CPP_MALLOC(sizeof(Region));
        size_t allocationSize = std::max(kDefaultRegionSize, MakeMultipleOf(size, kPageSize));
        region->start = region->current = (char*)IL2CPP_MALLOC(allocationSize);
        region->size = region->free = allocationSize;
        m_Regions.push_back(region);

        return region;
    }
}
}
