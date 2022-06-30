#include "il2cpp-config.h"

#if IL2CPP_GC_NULL
struct Il2CppObject;

#include <stdlib.h>
#include "il2cpp-api.h"
#include "GarbageCollector.h"
#include "utils/Memory.h"

void
il2cpp::gc::GarbageCollector::Initialize()
{
}

void il2cpp::gc::GarbageCollector::UninitializeGC()
{
}

void*
il2cpp::gc::GarbageCollector::AllocateFixed(size_t size, void *descr)
{
    return IL2CPP_MALLOC_ZERO(size);
}

void*
il2cpp::gc::GarbageCollector::MakeDescriptorForObject(size_t *bitmap, int numbits)
{
    return NULL;
}

void* il2cpp::gc::GarbageCollector::MakeDescriptorForString()
{
    return NULL;
}

void* il2cpp::gc::GarbageCollector::MakeDescriptorForArray()
{
    return NULL;
}

void il2cpp::gc::GarbageCollector::StopWorld()
{
    IL2CPP_NOT_IMPLEMENTED(il2cpp::gc::GarbageCollector::StopWorld);
}

void il2cpp::gc::GarbageCollector::StartWorld()
{
    IL2CPP_NOT_IMPLEMENTED(il2cpp::gc::GarbageCollector::StartWorld);
}

void
il2cpp::gc::GarbageCollector::RemoveWeakLink(void **link_addr)
{
    *link_addr = NULL;
}

Il2CppObject*
il2cpp::gc::GarbageCollector::GetWeakLink(void **link_addr)
{
    return (Il2CppObject*)*link_addr;
}

void
il2cpp::gc::GarbageCollector::AddWeakLink(void **link_addr, Il2CppObject *obj, bool track)
{
    *link_addr = obj;
}

bool
il2cpp::gc::GarbageCollector::RegisterThread(void *baseptr)
{
    return true;
}

bool
il2cpp::gc::GarbageCollector::UnregisterThread()
{
    return true;
}

il2cpp::gc::GarbageCollector::FinalizerCallback il2cpp::gc::GarbageCollector::RegisterFinalizerWithCallback(Il2CppObject* obj, FinalizerCallback callback)
{
    return NULL;
}

void
il2cpp::gc::GarbageCollector::FreeFixed(void* addr)
{
    IL2CPP_FREE(addr);
}

int32_t
il2cpp::gc::GarbageCollector::InvokeFinalizers()
{
    return 0;
}

bool
il2cpp::gc::GarbageCollector::HasPendingFinalizers()
{
    return false;
}

void
il2cpp::gc::GarbageCollector::Collect(int maxGeneration)
{
}

int32_t
il2cpp::gc::GarbageCollector::CollectALittle()
{
    return 0;
}

void
il2cpp::gc::GarbageCollector::StartIncrementalCollection()
{
}

void
il2cpp::gc::GarbageCollector::Enable()
{
}

void
il2cpp::gc::GarbageCollector::Disable()
{
}

void
il2cpp::gc::GarbageCollector::SetMode(Il2CppGCMode mode)
{
}

bool
il2cpp::gc::GarbageCollector::IsDisabled()
{
    return true;
}

int64_t
il2cpp::gc::GarbageCollector::GetUsedHeapSize(void)
{
    return 0;
}

int64_t
il2cpp::gc::GarbageCollector::GetAllocatedHeapSize(void)
{
    return 0;
}

int32_t
il2cpp::gc::GarbageCollector::GetMaxGeneration()
{
    return 0;
}

int32_t
il2cpp::gc::GarbageCollector::GetCollectionCount(int32_t generation)
{
    return 0;
}

void il2cpp::gc::GarbageCollector::ForEachHeapSection(void* user_data, HeapSectionCallback callback)
{
}

size_t il2cpp::gc::GarbageCollector::GetSectionCount()
{
    return 0;
}

void* il2cpp::gc::GarbageCollector::CallWithAllocLockHeld(GCCallWithAllocLockCallback callback, void* user_data)
{
    return callback(user_data);
}

int64_t
il2cpp::gc::GarbageCollector::GetMaxTimeSliceNs()
{
    return 0;
}

void
il2cpp::gc::GarbageCollector::SetMaxTimeSliceNs(int64_t maxTimeSlice)
{
}

bool
il2cpp::gc::GarbageCollector::IsIncremental()
{
    return false;
}

#endif
