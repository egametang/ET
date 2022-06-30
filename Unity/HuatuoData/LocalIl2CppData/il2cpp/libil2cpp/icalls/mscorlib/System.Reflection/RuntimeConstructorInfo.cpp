#include "il2cpp-config.h"
#include "RuntimeConstructorInfo.h"
#include "vm/Reflection.h"
#include "RuntimeMethodInfo.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    int32_t RuntimeConstructorInfo::get_metadata_token(Il2CppObject* method)
    {
        return vm::Reflection::GetMetadataToken(method);
    }

    Il2CppObject* RuntimeConstructorInfo::InternalInvoke(Il2CppReflectionMethod* method, Il2CppObject* thisPtr, Il2CppArray* params, Il2CppException** exc)
    {
        return RuntimeMethodInfo::InternalInvoke(method, thisPtr, params, exc);
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
