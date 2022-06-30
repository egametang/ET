#include "il2cpp-config.h"
#include "RuntimeEventInfo.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Reflection.h"
#include "vm/String.h"

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
    int32_t RuntimeEventInfo::get_metadata_token(Il2CppObject* monoEvent)
    {
        return vm::Reflection::GetMetadataToken(monoEvent);
    }

    void RuntimeEventInfo::get_event_info(Il2CppReflectionMonoEvent* event, Il2CppReflectionMonoEventInfo* eventInfo)
    {
        eventInfo->declaringType = vm::Reflection::GetTypeObject(&event->eventInfo->parent->byval_arg);
        eventInfo->reflectedType = event->reflectedType;
        eventInfo->name = vm::String::New(event->eventInfo->name);

        Il2CppClass* reflectedTypeInfo = vm::Class::FromIl2CppType(event->reflectedType->type);
        eventInfo->addMethod = event->eventInfo->add != NULL ? vm::Reflection::GetMethodObject(event->eventInfo->add, reflectedTypeInfo) : NULL;
        eventInfo->removeMethod = event->eventInfo->remove != NULL ? vm::Reflection::GetMethodObject(event->eventInfo->remove, reflectedTypeInfo) : NULL;
        eventInfo->raiseMethod = event->eventInfo->raise != NULL ? vm::Reflection::GetMethodObject(event->eventInfo->raise, reflectedTypeInfo) : NULL;
        eventInfo->eventAttributes = event->eventInfo->eventType->attrs;
        eventInfo->otherMethods = vm::Array::NewCached(il2cpp_defaults.method_info_class, 0); // Empty for now
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
