#include "il2cpp-config.h"
#include "mono-structs.h"
#include "RuntimeGPtrArrayHandle.h"
#include "utils/Memory.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    void RuntimeGPtrArrayHandle::GPtrArrayFree(void* value)
    {
        IL2CPP_ASSERT(value != NULL);
        free_gptr_array((MonoGPtrArray*)value);
    }
} // namespace Mono
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
