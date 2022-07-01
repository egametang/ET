#include "il2cpp-config.h"
#include "RuntimeFieldHandle.h"
#include "icalls/mscorlib/System.Reflection/MonoField.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Object.h"
#include "vm/Type.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    void RuntimeFieldHandle::SetValueDirect(Il2CppReflectionField* field, Il2CppObject* fieldType, Il2CppTypedRef* typedRef, Il2CppObject* value, Il2CppObject* contextType)
    {
        IL2CPP_ASSERT(field);
        IL2CPP_ASSERT(typedRef);
        IL2CPP_ASSERT(value);

        FieldInfo* f = field->field;
        if (!vm::Type::IsStruct(&f->parent->byval_arg))
        {
            std::string errorMessage = "The type ";
            errorMessage += vm::Type::GetName(&f->parent->byval_arg, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
            errorMessage += " is not struct";
            vm::Exception::Raise(vm::Exception::GetNotSupportedException(errorMessage.c_str()));
            return;
        }

        if (vm::Type::IsReference(f->type))
            vm::Field::SetValueRaw(f->type, (uint8_t*)typedRef->value + f->offset - sizeof(Il2CppObject), value, false);
        else
            vm::Field::SetValueRaw(f->type, (uint8_t*)typedRef->value + f->offset - sizeof(Il2CppObject), vm::Object::Unbox(value), false);
    }

    void RuntimeFieldHandle::SetValueInternal(Il2CppReflectionField* fi, Il2CppObject* obj, Il2CppObject* value)
    {
        // In mono's icall-def.h file, this maps to the same icall as MonoField.SetValueInternal
        // so our implementation will do the same
        Reflection::MonoField::SetValueInternal(fi, obj, value);
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
