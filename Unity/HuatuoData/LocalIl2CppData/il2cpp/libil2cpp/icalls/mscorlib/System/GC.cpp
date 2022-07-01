#include "il2cpp-config.h"

#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/GC.h"
#include "gc/GarbageCollector.h"
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
    void GC::SuppressFinalize(Il2CppObject *obj)
    {
        if (obj == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentNullException("obj"));

        il2cpp::gc::GarbageCollector::SuppressFinalizer(obj);
    }

    int32_t GC::GetGeneration(Il2CppObject* obj)
    {
        return il2cpp::gc::GarbageCollector::GetGeneration(obj);
    }

    void GC::KeepAlive(Il2CppObject* obj)
    {
        // do nothing. icall just prevents compilers from optimizing away references to object being passed in
    }

    void GC::InternalCollect(int generation)
    {
        il2cpp::gc::GarbageCollector::Collect(generation);
    }

    int32_t GC::get_MaxGeneration()
    {
        return il2cpp::gc::GarbageCollector::GetMaxGeneration();
    }

    void GC::RecordPressure(int64_t bytesAllocated)
    {
        il2cpp::gc::GarbageCollector::AddMemoryPressure(bytesAllocated);
    }

    void GC::WaitForPendingFinalizers()
    {
        il2cpp::gc::GarbageCollector::WaitForPendingFinalizers();
    }

    int64_t GC::GetTotalMemory(bool forceFullCollection)
    {
        if (forceFullCollection)
            il2cpp::gc::GarbageCollector::Collect(il2cpp::gc::GarbageCollector::GetMaxGeneration());

        return il2cpp::gc::GarbageCollector::GetUsedHeapSize();
    }

    void GC::ReRegisterForFinalize(Il2CppObject* obj)
    {
        if (obj == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentNullException("obj"));

        il2cpp::gc::GarbageCollector::RegisterFinalizer(obj);
    }

    int32_t GC::CollectionCount(int32_t generation)
    {
        return il2cpp::gc::GarbageCollector::GetCollectionCount(generation);
    }

    Il2CppObject* GC::get_ephemeron_tombstone()
    {
        return vm::Object::New(il2cpp_defaults.object_class);
    }

    void GC::register_ephemeron_array(Il2CppArray* array)
    {
        // Mono only does something in this icall when SGEN is in use.  We don't have SGEN,
        // so there is nothing to do here.
    }

    int32_t GC::GetCollectionCount(int32_t generation)
    {
        return CollectionCount(generation);
    }

    int32_t GC::GetMaxGeneration()
    {
        return get_MaxGeneration();
    }

    void GC::_ReRegisterForFinalize(Il2CppObject* o)
    {
        ReRegisterForFinalize(o);
    }

    void GC::_SuppressFinalize(Il2CppObject* o)
    {
        SuppressFinalize(o);
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
