#include "il2cpp-config.h"
#include "MarshalAlloc.h"
#include "os/MarshalAlloc.h"
#include "os/ThreadLocalValue.h"
#include "vm/Exception.h"
#include <deque>

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace vm
{
#if _DEBUG
    static os::ThreadLocalValue s_Allocations;

    static baselib::ReentrantLock s_AllocationStorageMutex;
    static std::deque<std::vector<std::map<void*, size_t> > > s_AllocationStorage;

    static std::vector<std::map<void*, size_t> >& GetAllocationsForCurrentThread()
    {
        std::vector<std::map<void*, size_t> >* ptr = NULL;
        s_Allocations.GetValue(reinterpret_cast<void**>(&ptr));
        if (ptr == NULL)
        {
            os::FastAutoLock lock(&s_AllocationStorageMutex);
            s_AllocationStorage.push_back(std::vector<std::map<void*, size_t> >());
            ptr = &s_AllocationStorage.back();
            s_Allocations.SetValue(ptr);
        }

        return *ptr;
    }

    static std::map<void*, size_t>* GetAllocationsForCurrentFrame()
    {
        std::vector<std::map<void*, size_t> >& currentThreadAllocations = GetAllocationsForCurrentThread();
        if (currentThreadAllocations.size() > 0)
            return &currentThreadAllocations.back();

        return NULL;
    }

#endif

    void* MarshalAlloc::Allocate(size_t size)
    {
        void* ptr = os::MarshalAlloc::Allocate(size);

#if _DEBUG
        std::map<void*, size_t>* allocations = GetAllocationsForCurrentFrame();
        if (allocations != NULL)
            (*allocations)[ptr] = size;
#endif

        return ptr;
    }

    void* MarshalAlloc::ReAlloc(void* ptr, size_t size)
    {
        void* realloced = os::MarshalAlloc::ReAlloc(ptr, size);

#if _DEBUG
        std::map<void*, size_t>* allocations = GetAllocationsForCurrentFrame();
        if (allocations != NULL)
        {
            if (ptr != NULL && ptr != realloced)
            {
                std::map<void*, size_t>::iterator found = allocations->find(ptr);
                IL2CPP_ASSERT(found != allocations->end() && "Invalid call to MarshalAlloc::ReAlloc. The pointer is not in the allocation list.");
                allocations->erase(found);
            }

            (*allocations)[realloced] = size;
        }
#endif

        return realloced;
    }

    void MarshalAlloc::Free(void* ptr)
    {
#if _DEBUG
        std::map<void*, size_t>* allocations = GetAllocationsForCurrentFrame();
        if (allocations != NULL)
        {
            std::map<void*, size_t>::iterator found = allocations->find(ptr);
            if (found != allocations->end()) // It might not be necessarily allocated by us, e.g. we might be freeing memory that's returned from native P/Invoke call
                allocations->erase(found);
        }
#endif

        os::MarshalAlloc::Free(ptr);
    }

    void* MarshalAlloc::AllocateHGlobal(size_t size)
    {
        return os::MarshalAlloc::AllocateHGlobal(size);
    }

    void* MarshalAlloc::ReAllocHGlobal(void* ptr, size_t size)
    {
        return os::MarshalAlloc::ReAllocHGlobal(ptr, size);
    }

    void MarshalAlloc::FreeHGlobal(void* ptr)
    {
        os::MarshalAlloc::FreeHGlobal(ptr);
    }

#if _DEBUG

    void MarshalAlloc::PushAllocationFrame()
    {
        GetAllocationsForCurrentThread().push_back(std::map<void*, size_t>());
    }

    void MarshalAlloc::PopAllocationFrame()
    {
        GetAllocationsForCurrentThread().pop_back();
    }

    bool MarshalAlloc::HasUnfreedAllocations()
    {
        std::map<void*, size_t>* allocations = GetAllocationsForCurrentFrame();
        return allocations != NULL && allocations->size() > 0;
    }

    void MarshalAlloc::ClearAllTrackedAllocations()
    {
        std::map<void*, size_t>* allocations = GetAllocationsForCurrentFrame();
        if (allocations != NULL)
            allocations->clear();
    }

#endif
} /* namespace vm */
} /* namespace il2cpp */
