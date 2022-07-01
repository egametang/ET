#include "il2cpp-config.h"

#include <stddef.h>

#include "icalls/mscorlib/System.Reflection/FieldInfo.h"

#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "vm/Class.h"
#include "vm/Exception.h"
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
    Il2CppArray* FieldInfo::GetTypeModifiers(Il2CppReflectionField* field, bool optional)
    {
        NOT_SUPPORTED_IL2CPP(FieldInfo::GetTypeModifiers, "GetOptionalCustomModifiers and GetRequiredCustomModifiers are not supported.");
        return NULL;
    }

    Il2CppReflectionMarshal* FieldInfo::GetUnmanagedMarshal(Il2CppReflectionField* field)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(FieldInfo::GetUnmanagedMarshal, "This should only be needed for types with a MarshalAsAttribute");
        return NULL;
    }

    Il2CppReflectionField* FieldInfo::internal_from_handle_type(intptr_t field_handle, intptr_t type_handle)
    {
        ::FieldInfo* fieldInfo = (::FieldInfo*)field_handle;
        Il2CppType* il2cppType = (Il2CppType*)type_handle;

        if (il2cppType == NULL)
            return vm::Reflection::GetFieldObject(fieldInfo->parent, fieldInfo);

        Il2CppClass* originalClass = vm::Class::FromIl2CppType(il2cppType);

        for (Il2CppClass* k = originalClass; k; k = k->parent)
        {
            if (k == fieldInfo->parent)
                return vm::Reflection::GetFieldObject(originalClass, fieldInfo);
        }

        return NULL;
    }

    Il2CppObject* FieldInfo::get_marshal_info(Il2CppObject* _this)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(FieldInfo::get_marshal_info, "We currently don't store marshal information in metadata");
        return NULL;
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
