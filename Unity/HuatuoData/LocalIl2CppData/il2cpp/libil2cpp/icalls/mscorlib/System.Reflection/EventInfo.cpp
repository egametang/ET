#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-api.h"
#include "EventInfo.h"
#include "vm/Class.h"
#include "vm/Reflection.h"

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
    Il2CppReflectionEvent* EventInfo::internal_from_handle_type(intptr_t handlePtr, intptr_t typePtr)
    {
        Il2CppClass *klass;

        IL2CPP_ASSERT(handlePtr);
        ::EventInfo *handle = (::EventInfo*)handlePtr;
        Il2CppType *type = (Il2CppType*)typePtr;

        if (!type)
        {
            klass = handle->parent;
        }
        else
        {
            klass = il2cpp_class_from_il2cpp_type(type);

            bool found = klass == handle->parent || il2cpp::vm::Class::HasParent(klass, handle->parent);
            if (!found)
                /* Managed code will throw an exception */
                return NULL;
        }

        Il2CppReflectionEvent *result = il2cpp::vm::Reflection::GetEventObject(klass, handle);
        return result;
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
