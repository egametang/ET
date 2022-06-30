#include "il2cpp-config.h"
#include "RuntimePropertyInfo.h"
#include "gc/WriteBarrier.h"
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
    int32_t RuntimePropertyInfo::get_metadata_token(Il2CppObject* monoProperty)
    {
        return vm::Reflection::GetMetadataToken(monoProperty);
    }

    Il2CppObject* RuntimePropertyInfo::get_default_value(Il2CppObject* prop)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimePropertyInfo::get_default_value);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppReflectionProperty* RuntimePropertyInfo::internal_from_handle_type(intptr_t handlePtr, intptr_t typePtr)
    {
        Il2CppClass *klass;

        IL2CPP_ASSERT(handlePtr);

        ::PropertyInfo *handle = (::PropertyInfo*)handlePtr;
        Il2CppType *type = (Il2CppType*)typePtr;

        if (!type)
        {
            klass = handle->parent;
        }
        else
        {
            klass = vm::Class::FromIl2CppType(type);

            bool found = klass == handle->parent || il2cpp::vm::Class::HasParent(klass, handle->parent);
            if (!found)
                /* Managed code will throw an exception */
                return NULL;
        }

        Il2CppReflectionProperty *result = il2cpp::vm::Reflection::GetPropertyObject(klass, handle);
        return result;
    }

    Il2CppArray* RuntimePropertyInfo::GetTypeModifiers(Il2CppObject* prop, bool optional)
    {
        NOT_SUPPORTED_IL2CPP(MonoPropertyInfo::GetTypeModifiers, "This icall is not supported by il2cpp.");

        return 0;
    }

    void RuntimePropertyInfo::get_property_info(Il2CppReflectionProperty *property, Il2CppPropertyInfo *info, PInfo req_info)
    {
        if ((req_info & PInfo_ReflectedType) != 0)
            IL2CPP_STRUCT_SETREF(info, parent, vm::Reflection::GetTypeObject(&property->klass->byval_arg));
        else if ((req_info & PInfo_DeclaringType) != 0)
            IL2CPP_STRUCT_SETREF(info, declaringType, vm::Reflection::GetTypeObject(&property->property->parent->byval_arg));

        if ((req_info & PInfo_Name) != 0)
            IL2CPP_STRUCT_SETREF(info, name, vm::String::New(property->property->name));

        if ((req_info & PInfo_Attributes) != 0)
            info->attrs = property->property->attrs;

        if ((req_info & PInfo_GetMethod) != 0)
            IL2CPP_STRUCT_SETREF(info, get, property->property->get ?
                vm::Reflection::GetMethodObject(property->property->get, property->klass) : NULL);

        if ((req_info & PInfo_SetMethod) != 0)
            IL2CPP_STRUCT_SETREF(info, set, property->property->set ?
                vm::Reflection::GetMethodObject(property->property->set, property->klass) : NULL);
        /*
         * There may be other methods defined for properties, though, it seems they are not exposed
         * in the reflection API
         */
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
