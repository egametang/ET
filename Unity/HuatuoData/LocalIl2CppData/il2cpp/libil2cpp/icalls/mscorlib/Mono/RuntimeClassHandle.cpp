#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "RuntimeClassHandle.h"
#include "vm/Class.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    intptr_t RuntimeClassHandle::GetTypeFromClass(Il2CppClass* klass)
    {
        return reinterpret_cast<intptr_t>(il2cpp::vm::Class::GetType(klass));
    }
} // namespace Mono
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
