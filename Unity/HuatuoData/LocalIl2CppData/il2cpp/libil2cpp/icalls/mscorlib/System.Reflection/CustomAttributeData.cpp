#include "il2cpp-config.h"
#include "CustomAttributeData.h"
#include "gc/GarbageCollector.h"
#include "metadata/CustomAttributeDataReader.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/Exception.h"
#include "vm/MetadataCache.h"

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
    static void SetCustomAttributeTypeArgument(const MethodInfo* ctor, void* argBuffer, const il2cpp::metadata::CustomAttributeArgument* inArg)
    {
        void* params[] = {
            il2cpp::vm::Reflection::GetTypeObject(&inArg->klass->byval_arg),
            il2cpp::vm::Class::IsValuetype(inArg->klass) ? vm::Object::Box(inArg->klass, (void*)&inArg->data) : reinterpret_cast<Il2CppObject*>(inArg->data.obj)
        };
        il2cpp::vm::Runtime::Invoke(ctor, argBuffer, params, NULL);
    }

    class Visitor : public il2cpp::metadata::CustomAttributeReaderVisitor
    {
        static const MethodInfo* customAttributeTypedArgumentConstructor;
        static const MethodInfo* customAttributeNamedArgumentConstructor;

        Il2CppClass* attrClass;
        Il2CppArray** ctorArgs;
        Il2CppArray** namedArgs;
        uint32_t fieldCount;

    public:

        Visitor(Il2CppClass* attrClass, Il2CppArray** ctorArgs, Il2CppArray** namedArgs) : attrClass(attrClass), ctorArgs(ctorArgs), namedArgs(namedArgs), fieldCount(0)
        {
            if (!customAttributeTypedArgumentConstructor)
            {
                const Il2CppType* typedArgumentCtorArgTypes[] = { &il2cpp_defaults.systemtype_class->byval_arg, &il2cpp_defaults.object_class->byval_arg };
                customAttributeTypedArgumentConstructor = vm::Class::GetMethodFromNameFlagsAndSig(il2cpp_defaults.customattribute_typed_argument_class, ".ctor", 2, 0, typedArgumentCtorArgTypes);

                const Il2CppType* namedArgumentCtorArgTypes[] = { &il2cpp_defaults.member_info_class->byval_arg, &il2cpp_defaults.customattribute_typed_argument_class->byval_arg };
                customAttributeNamedArgumentConstructor = vm::Class::GetMethodFromNameFlagsAndSig(il2cpp_defaults.customattribute_named_argument_class, ".ctor", 2, 0, namedArgumentCtorArgTypes);

                if (customAttributeTypedArgumentConstructor == NULL || customAttributeNamedArgumentConstructor == NULL)
                {
                    customAttributeTypedArgumentConstructor = NULL;
                    IL2CPP_NOT_IMPLEMENTED_ICALL(MonoCustomAttrs::GetCustomAttributesDataInternal);
                }

                IL2CPP_ASSERT(il2cpp::vm::Class::IsValuetype(il2cpp_defaults.customattribute_typed_argument_class));
                IL2CPP_ASSERT(il2cpp::vm::Class::IsValuetype(il2cpp_defaults.customattribute_named_argument_class));
            }
        }

        virtual void VisitArgumentSizes(uint32_t argumentCount, uint32_t fieldCount, uint32_t propertyCount)
        {
            *ctorArgs = il2cpp::vm::Array::New(il2cpp_defaults.object_class, argumentCount);
            il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)ctorArgs);

            *namedArgs = il2cpp::vm::Array::New(il2cpp_defaults.customattribute_named_argument_class, fieldCount + propertyCount);
            il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)namedArgs);

            this->fieldCount = fieldCount;
        }

        virtual void VisitArgument(const il2cpp::metadata::CustomAttributeArgument& argument, uint32_t index)
        {
            int32_t typedArgSize = il2cpp::vm::Class::GetInstanceSize(il2cpp_defaults.customattribute_typed_argument_class) - sizeof(Il2CppObject);
            void* typedArgBuffer = alloca(typedArgSize);
            memset(typedArgBuffer, 0, typedArgSize);
            SetCustomAttributeTypeArgument(customAttributeTypedArgumentConstructor, typedArgBuffer, &argument);
            il2cpp_array_setref(*ctorArgs, index, il2cpp::vm::Object::Box(il2cpp_defaults.customattribute_typed_argument_class, typedArgBuffer));
        }

        void CreateNamedArgument(const il2cpp::metadata::CustomAttributeArgument arg, void* metadataObject, uint32_t index)
        {
            int32_t typedArgSize = il2cpp::vm::Class::GetInstanceSize(il2cpp_defaults.customattribute_typed_argument_class) - sizeof(Il2CppObject);
            int32_t namedArgSize = il2cpp::vm::Class::GetInstanceSize(il2cpp_defaults.customattribute_named_argument_class) - sizeof(Il2CppObject);
            void* namedArgBuffer = alloca(namedArgSize);
            void* typedArgBuffer = alloca(typedArgSize);

            SetCustomAttributeTypeArgument(customAttributeTypedArgumentConstructor, typedArgBuffer, &arg);
            void* params[] = {
                metadataObject,
                typedArgBuffer
            };

            il2cpp::vm::Runtime::Invoke(customAttributeNamedArgumentConstructor, namedArgBuffer, params, NULL);
            il2cpp_array_setref(*namedArgs, index, il2cpp::vm::Object::Box(il2cpp_defaults.customattribute_named_argument_class, namedArgBuffer));
        }

        virtual void VisitField(const il2cpp::metadata::CustomAttributeFieldArgument& field, uint32_t index)
        {
            CreateNamedArgument(field.arg, vm::Reflection::GetFieldObject(attrClass, const_cast<FieldInfo*>(field.field)), index);
        }

        virtual void VisitProperty(const il2cpp::metadata::CustomAttributePropertyArgument& prop, uint32_t index)
        {
            CreateNamedArgument(prop.arg, vm::Reflection::GetPropertyObject(attrClass, const_cast<PropertyInfo*>(prop.prop)), index + fieldCount);
        }
    };

    const MethodInfo* Visitor::customAttributeTypedArgumentConstructor;
    const MethodInfo* Visitor::customAttributeNamedArgumentConstructor;

    void CustomAttributeData::ResolveArgumentsInternal(Il2CppObject* ctor, Il2CppObject* assembly, intptr_t data, uint32_t data_length, Il2CppArray** ctorArgs, Il2CppArray** namedArgs)
    {
        const MethodInfo* ctorMethod = ((Il2CppReflectionMethod*)ctor)->method;
        const Il2CppImage* image = ((Il2CppReflectionAssembly*)assembly)->assembly->image;

        Visitor visitor(ctor->klass, ctorArgs, namedArgs);
        Il2CppException* exc;
        il2cpp::metadata::CustomAttributeDataReader::VisitCustomAttributeData(image, ctorMethod, (const void*)data, data_length, &visitor, &exc);

        if (exc != NULL)
            vm::Exception::Raise(exc);
    }
} // namespace Reflection
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
