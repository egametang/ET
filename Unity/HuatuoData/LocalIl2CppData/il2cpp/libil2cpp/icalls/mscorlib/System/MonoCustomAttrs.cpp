#include "il2cpp-config.h"
#include <memory>
#include "icalls/mscorlib/System/MonoCustomAttrs.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
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

    static Il2CppObject* CreateCustomAttributeData(const Il2CppAssembly* assembly, const il2cpp::metadata::LazyCustomAttributeData& data)
    {
        static const MethodInfo* customAttributeDataConstructor;
        if (!customAttributeDataConstructor)
        {
            customAttributeDataConstructor = vm::Class::GetMethodFromName(il2cpp_defaults.customattribute_data_class, ".ctor", 4);
            if (customAttributeDataConstructor == NULL)
                IL2CPP_NOT_IMPLEMENTED_ICALL(MonoCustomAttrs::GetCustomAttributesDataInternal);
        }

        Il2CppObject* customAttributeData = vm::Object::New(il2cpp_defaults.customattribute_data_class);
        void* params[] =
        {
            vm::Reflection::GetMethodObject(data.ctor, data.ctor->klass),
            vm::Reflection::GetAssemblyObject(assembly),
            (void*)&data.dataStart,
            (void*)&data.dataLength
        };
        vm::Runtime::Invoke(customAttributeDataConstructor, customAttributeData, params, NULL);
        return customAttributeData;
    }

    Il2CppArray* MonoCustomAttrs::GetCustomAttributesDataInternal(Il2CppObject* obj)
    {
        metadata::CustomAttributeDataReader reader = il2cpp::vm::Reflection::GetCustomAttrsDataReader(obj);

        Il2CppArray* result = il2cpp::vm::Array::New(il2cpp_defaults.customattribute_data_class, reader.GetCount());
        uint32_t i = 0;

        bool hasError = false;
        il2cpp::metadata::LazyCustomAttributeData data;
        Il2CppException* exc = NULL;
        il2cpp::metadata::CustomAttributeDataIterator iter = reader.GetDataIterator();
        while (reader.ReadLazyCustomAttributeData(obj->klass->image, &data, &iter, &exc))
        {
            IL2CPP_ASSERT(i < reader.GetCount());
            Il2CppObject* attributeData = CreateCustomAttributeData(obj->klass->image->assembly, data);
            il2cpp_array_setref(result, i, attributeData);
            i++;
        }

        if (exc != NULL)
            vm::Exception::Raise(exc);

        return result;
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
