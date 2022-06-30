#include "il2cpp-config.h"

#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/GC.h"
#include "gc/GarbageCollector.h"
#include "vm/Domain.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Array.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    int32_t GC::GetCollectionCount(int32_t generation)
    {
        return il2cpp::gc::GarbageCollector::GetCollectionCount(generation);
    }

    int32_t GC::GetGeneration(Il2CppObject* obj)
    {
        return il2cpp::gc::GarbageCollector::GetGeneration(obj);
    }

    int32_t GC::GetMaxGeneration()
    {
        return il2cpp::gc::GarbageCollector::GetMaxGeneration();
    }

    int64_t GC::GetAllocatedBytesForCurrentThread()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(GC::GetAllocatedBytesForCurrentThread);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    int64_t GC::GetTotalMemory(bool forceFullCollection)
    {
        if (forceFullCollection)
            il2cpp::gc::GarbageCollector::Collect(il2cpp::gc::GarbageCollector::GetMaxGeneration());

        return il2cpp::gc::GarbageCollector::GetUsedHeapSize();
    }

    Il2CppObject* GC::get_ephemeron_tombstone()
    {
        return il2cpp::vm::Domain::GetCurrent()->ephemeron_tombstone;
    }

    void GC::_ReRegisterForFinalize(Il2CppObject* obj)
    {
        if (obj == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentNullException("obj"));

        il2cpp::gc::GarbageCollector::RegisterFinalizer(obj);
    }

    void GC::_SuppressFinalize(Il2CppObject* obj)
    {
        if (obj == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentNullException("obj"));

        il2cpp::gc::GarbageCollector::SuppressFinalizer(obj);
    }

    void GC::InternalCollect(int32_t generation)
    {
        il2cpp::gc::GarbageCollector::Collect(generation);
    }

    void GC::RecordPressure(int64_t bytesAllocated)
    {
        il2cpp::gc::GarbageCollector::AddMemoryPressure(bytesAllocated);
    }

    void GC::register_ephemeron_array(Il2CppArray* array)
    {
        il2cpp::gc::GarbageCollector::EphemeronArrayAdd((Il2CppObject*)array);
    }

    void GC::WaitForPendingFinalizers()
    {
        il2cpp::gc::GarbageCollector::WaitForPendingFinalizers();
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
