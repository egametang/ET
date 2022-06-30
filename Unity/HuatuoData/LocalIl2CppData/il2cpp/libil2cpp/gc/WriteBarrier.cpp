#include "il2cpp-config.h"
#include "gc/WriteBarrier.h"
#include "gc/GarbageCollector.h"

namespace il2cpp
{
namespace gc
{
    void WriteBarrier::GenericStore(void** ptr, void* value)
    {
        *ptr = value;
        GarbageCollector::SetWriteBarrier((void**)ptr);
    }
} /* gc */
} /* il2cpp */
