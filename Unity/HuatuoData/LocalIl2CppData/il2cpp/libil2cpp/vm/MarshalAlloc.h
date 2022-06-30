#pragma once
#include "il2cpp-config.h"

#if _DEBUG
#include <map>
#include "os/Mutex.h"
#endif

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API MarshalAlloc
    {
    public:
        static void* Allocate(size_t size);
        static void* ReAlloc(void* ptr, size_t size);
        static void Free(void* ptr);

        static void* AllocateHGlobal(size_t size);
        static void* ReAllocHGlobal(void* ptr, size_t size);
        static void FreeHGlobal(void* ptr);

#if _DEBUG
        static void PushAllocationFrame();
        static void PopAllocationFrame();
        static bool HasUnfreedAllocations();
        static void ClearAllTrackedAllocations();
#endif
    };
} /* namespace vm */
} /* namespace il2cpp */
