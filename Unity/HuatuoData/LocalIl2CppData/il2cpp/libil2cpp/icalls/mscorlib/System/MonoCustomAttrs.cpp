#include "il2cpp-config.h"
#include <memory>
#include "icalls/mscorlib/System/MonoCustomAttrs.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "gc/GarbageCollector.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/Runtime.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppArray * MonoCustomAttrs::GetCustomAttributesInternal(Il2CppObject* obj, Il2CppReflectionType* type, bool pseudoAttrs)
    {
        IL2CPP_ASSERT(pseudoAttrs == false && "System_MonoCustomAttrs_GetCustomAttributesInternal_icall with pseudoAttrs == true has not been implemented yet");

        CustomAttributesCache *cinfo = il2cpp::vm::Reflection::GetCustomAttrsInfo(obj);

        if (!cinfo)
        {
            return il2cpp::vm::Array::New(il2cpp_defaults.object_class, 0);
        }

        if (!type)
        {
            Il2CppArray *result = il2cpp::vm::Array::New(il2cpp_defaults.object_class, cinfo->count);
            memcpy(il2cpp_array_addr(result, Il2CppObject*, 0), cinfo->attributes, sizeof(Il2CppObject*) * cinfo->count);
            gc::GarbageCollector::SetWriteBarrier((void**)il2cpp_array_addr(result, Il2CppObject*, 0), sizeof(Il2CppObject*) * cinfo->count);
            return result;
        }

        Il2CppClass* attributeClass = vm::Class::FromIl2CppType(type->type);
        int count = 0;
        for (int i = 0; i < cinfo->count; i++)
        {
            Il2CppObject* attr = cinfo->attributes[i];
            if (vm::Class::IsAssignableFrom(attributeClass, attr->klass))
                count++;
        }

        Il2CppArray *result = il2cpp::vm::Array::New(il2cpp_defaults.object_class, count);

        int index = 0;
        for (int i = 0; i < cinfo->count; i++)
        {
            Il2CppObject* attr = cinfo->attributes[i];
            if (!vm::Class::IsAssignableFrom(attributeClass, attr->klass))
                continue;

            il2cpp_array_setref(result, index, cinfo->attributes[i]);
            index++;
        }

        return result;
    }

    bool MonoCustomAttrs::IsDefinedInternal(Il2CppObject *obj, Il2CppReflectionType *attr_type)
    {
        return il2cpp::vm::Reflection::HasAttribute(obj, vm::Class::FromIl2CppType(attr_type->type));
    }

    static Il2CppObject* CreateCustomAttributeData(Il2CppObject* attribute)
    {
        static const MethodInfo* customAttributeDataConstructor;
        void *params[4];

        if (!customAttributeDataConstructor)
            customAttributeDataConstructor = vm::Class::GetMethodFromName(il2cpp_defaults.customattribute_data_class, ".ctor", 4);

        const MethodInfo* attributeConstructor = vm::Class::GetMethodFromNameFlags(attribute->klass, ".ctor", vm::Class::IgnoreNumberOfArguments, METHOD_ATTRIBUTE_PUBLIC);

        if (attributeConstructor == NULL)
            IL2CPP_NOT_IMPLEMENTED_ICALL(MonoCustomAttrs::GetCustomAttributesDataInternal);

        Il2CppObject* customAttributeData = vm::Object::New(il2cpp_defaults.customattribute_data_class);
        int argCount = 0;
        void* nullArg = NULL;
        params[0] = vm::Reflection::GetMethodObject(attributeConstructor, NULL);
        params[1] = vm::Reflection::GetAssemblyObject(attribute->klass->image->assembly);
        params[2] = &nullArg;
        params[3] = &argCount;
        vm::Runtime::Invoke(customAttributeDataConstructor, customAttributeData, params, NULL);
        return customAttributeData;
    }

    Il2CppArray* MonoCustomAttrs::GetCustomAttributesDataInternal(Il2CppObject* obj)
    {
        CustomAttributesCache *cinfo = il2cpp::vm::Reflection::GetCustomAttrsInfo(obj);

        if (!cinfo)
            return il2cpp::vm::Array::New(il2cpp_defaults.customattribute_data_class, 0);

        Il2CppArray* result = il2cpp::vm::Array::New(il2cpp_defaults.customattribute_data_class, cinfo->count);
        for (int i = 0; i < cinfo->count; ++i)
        {
            Il2CppObject* attribute = CreateCustomAttributeData(cinfo->attributes[i]);
            il2cpp_array_setref(result, i, attribute);
        }

        return result;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
