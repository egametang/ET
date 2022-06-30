#pragma once

#include "il2cpp-config.h"
struct Il2CppGenericClass;
struct Il2CppGenericMethod;

namespace il2cpp
{
namespace vm
{
    void MetadataAllocInitialize();
    void MetadataAllocCleanup();
// These allocators assume the g_MetadataLock lock is held
    void* MetadataMalloc(size_t size);
    void* MetadataCalloc(size_t count, size_t size);
// These metadata structures have their own locks, since they do lightweight initialization
    Il2CppGenericClass* MetadataAllocGenericClass();
    Il2CppGenericMethod* MetadataAllocGenericMethod();
} // namespace vm
} // namespace il2cpp
