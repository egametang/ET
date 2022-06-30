#pragma once

#include <stdint.h>
#include "os/Mutex.h"
struct Il2CppClass;

namespace il2cpp
{
namespace metadata
{
    class ArrayMetadata
    {
    public:
        static Il2CppClass* GetBoundedArrayClass(Il2CppClass* elementClass, uint32_t rank, bool bounded);

        typedef void(*ArrayTypeWalkCallback)(Il2CppClass* type, void* context);
        static void WalkSZArrays(ArrayTypeWalkCallback callback, void* context);
        static void WalkArrays(ArrayTypeWalkCallback callback, void* context);

        // called as part of Class::Init with lock held
        static void SetupArrayInterfaces(Il2CppClass* klass, const il2cpp::os::FastAutoLock& lock);
        static void SetupArrayVTable(Il2CppClass* klass, const il2cpp::os::FastAutoLock& lock);

        static void Clear();
    };
} /* namespace vm */
} /* namespace il2cpp */
