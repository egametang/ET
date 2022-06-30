#include "il2cpp-config.h"

#include "icalls/mscorlib/System/TypedReference.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Object.h"
#include "vm/Type.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppObject* TypedReference::InternalToObject(Il2CppTypedRef* typedRef)
    {
        Il2CppObject* result = NULL;
        if (vm::Type::IsReference(typedRef->type))
        {
            Il2CppObject** obj = (Il2CppObject**)typedRef->value;
            return *obj;
        }

        result = vm::Object::Box(typedRef->klass, typedRef->value);
        return result;
    }

    void TypedReference::InternalMakeTypedReference(Il2CppTypedRef* res, Il2CppObject* target, Il2CppArray* fields, Il2CppReflectionRuntimeType* lastFieldType)
    {
        memset(res, 0, sizeof(Il2CppTypedRef));

        IL2CPP_ASSERT(fields);

        uint32_t fieldsArrayLength = vm::Array::GetLength(fields);

        Il2CppClass* klass = target->vtable->klass;

        uint8_t* value = NULL;
        const Il2CppType *ftype = NULL;
        for (uint32_t i = 0; i < fieldsArrayLength; ++i)
        {
            FieldInfo* f = il2cpp_array_get(fields, FieldInfo*, i);
            if (f == NULL)
            {
                vm::Exception::Raise(vm::Exception::GetArgumentNullException("field"));
                return;
            }

            if (i == 0)
                value = (uint8_t*)target + f->offset;
            else
                value += f->offset - sizeof(Il2CppObject);

            klass = vm::Class::FromIl2CppType(f->type);
            ftype = f->type;
        }

        res->type = ftype;
        res->klass = vm::Class::FromIl2CppType(ftype);
        res->value = value;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
