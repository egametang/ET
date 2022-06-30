#include "CustomAttributeCreator.h"
#include "gc/WriteBarrier.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Property.h"
#include "vm/Runtime.h"

namespace il2cpp
{
namespace metadata
{
    static void* ConvertArgumentValue(const Il2CppType* targetType, const CustomAttributeArgument* arg)
    {
        // If the argument target target is a value type, then just pass a pointer to the data
        if (targetType->valuetype)
            return (void*)&arg->data;

        // Our target isn't value type, but our data is, we need to box
        if (il2cpp::vm::Class::IsValuetype(arg->klass))
            return il2cpp::vm::Object::Box(arg->klass, (void*)&arg->data);

        // Storing reference type data in a reference type field, just get the pointer to the object
        return arg->data.obj;
    }

    void CustomAttributeCreator::VisitCtor(const MethodInfo* ctor, CustomAttributeArgument args[], uint32_t argumentCount)
    {
        attr = il2cpp::vm::Object::New(ctor->klass);

        void** ctorArgs = (void**)alloca(argumentCount * sizeof(void*));
        for (uint32_t i = 0; i < argumentCount; i++)
            ctorArgs[i] = ConvertArgumentValue(ctor->parameters[i], args + i);
        il2cpp::vm::Runtime::Invoke(ctor, attr, ctorArgs, &exc);

        if (exc != NULL)
            attr = NULL;
    }

    void CustomAttributeCreator::VisitField(const CustomAttributeFieldArgument& field, uint32_t index)
    {
        if (exc != NULL)
            return;

        IL2CPP_ASSERT(attr);
        il2cpp::vm::Field::SetValue(attr, field.field, ConvertArgumentValue(field.field->type, &field.arg));
    }

    void CustomAttributeCreator::VisitProperty(const CustomAttributePropertyArgument& prop, uint32_t index)
    {
        if (exc != NULL)
            return;

        IL2CPP_ASSERT(attr);

        const MethodInfo* setMethod = il2cpp::vm::Property::GetSetMethod(prop.prop);
        IL2CPP_ASSERT(setMethod->parameters_count == 1);
        void* param = ConvertArgumentValue(setMethod->parameters[0], &prop.arg);

        il2cpp::vm::Runtime::Invoke(setMethod, attr, &param, &exc);
    }

    Il2CppObject* CustomAttributeCreator::GetAttribute(Il2CppException** exc)
    {
        *exc = this->exc;
        return attr;
    }
}     /* namespace vm */
} /* namespace il2cpp */
