#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-tabledefs.h"
#include "mono-structs.h"
#include "gc/GCHandle.h"
#include "gc/WriteBarrier.h"
#include "metadata//CustomAttributeDataReader.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/Il2CppTypeHash.h"
#include "os/ReaderWriterLock.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Event.h"
#include "vm/Exception.h"
#include "vm/Field.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Method.h"
#include "vm/Object.h"
#include "vm/Parameter.h"
#include "vm/Property.h"
#include "vm/Reflection.h"
#include "vm/String.h"
#include "vm/AssemblyName.h"
#include "utils/Il2CppHashMap.h"
#include "utils/StringUtils.h"
#include "utils/HashUtils.h"
#include "gc/AppendOnlyGCHashMap.h"


#include "gc/Allocator.h"

template<typename T>
struct ReflectionMapHash
{
    size_t operator()(const T& ea) const
    {
        return ((size_t)(intptr_t)(ea.first)) >> 3;
    }
};

template<typename T>
struct ReflectionMapLess
{
    bool operator()(const T& ea, const T& eb) const
    {
        if (ea.first < eb.first)
            return true;
        if (ea.second < eb.second)
            return true;
        return false;
    }
};


template<typename Key, typename Value>
struct ReflectionMap : public il2cpp::gc::AppendOnlyGCHashMap<Key, Value, ReflectionMapHash<Key> >
{
};

typedef ReflectionMap<std::pair<const Il2CppAssembly*, Il2CppClass*>, Il2CppReflectionAssembly*> AssemblyMap;
typedef ReflectionMap<std::pair<const FieldInfo*, Il2CppClass*>, Il2CppReflectionField*> FieldMap;
typedef ReflectionMap<std::pair<const PropertyInfo*, Il2CppClass*>, Il2CppReflectionProperty*> PropertyMap;
typedef ReflectionMap<std::pair<const EventInfo*, Il2CppClass*>, Il2CppReflectionEvent*> EventMap;
typedef ReflectionMap<std::pair<const MethodInfo*, Il2CppClass*>, Il2CppReflectionMethod*> MethodMap;
typedef ReflectionMap<std::pair<const Il2CppImage*, Il2CppClass*>, Il2CppReflectionModule*> ModuleMap;
typedef ReflectionMap<std::pair<const MethodInfo*, Il2CppClass*>, Il2CppArray*> ParametersMap;

typedef il2cpp::gc::AppendOnlyGCHashMap<const Il2CppType*, Il2CppReflectionType*, il2cpp::metadata::Il2CppTypeHash, il2cpp::metadata::Il2CppTypeEqualityComparer> TypeMap;

typedef Il2CppHashMap<Il2CppMetadataGenericParameterHandle, const MonoGenericParameterInfo*, il2cpp::utils::PassThroughHash<Il2CppMetadataGenericParameterHandle> > MonoGenericParameterMap;
typedef Il2CppHashMap<const  Il2CppAssembly*, const Il2CppMonoAssemblyName*, il2cpp::utils::PointerHash<const Il2CppAssembly> > MonoAssemblyNameMap;

// these needs to be pointers and allocated after GC is initialized since it uses GC Allocator
static AssemblyMap* s_AssemblyMap;
static FieldMap* s_FieldMap;
static PropertyMap* s_PropertyMap;
static EventMap* s_EventMap;
static MethodMap* s_MethodMap;
static ModuleMap* s_ModuleMap;
static ParametersMap* s_ParametersMap;
static TypeMap* s_TypeMap;
static MonoGenericParameterMap* s_MonoGenericParamterMap;
static MonoAssemblyNameMap* s_MonoAssemblyNameMap;

namespace il2cpp
{
namespace vm
{
    static il2cpp::os::ReaderWriterLock s_ReflectionICallsLock;

    static Il2CppClass *s_System_Reflection_Assembly;
    static Il2CppClass * s_System_Reflection_RuntimeFieldInfoKlass;
    static Il2CppClass *s_System_Reflection_Module;
    static Il2CppClass * s_System_Reflection_RuntimePropertyInfoKlass;
    static Il2CppClass * s_System_Reflection_RuntimeEventInfoKlass;
    static FieldInfo *s_DbNullValueField;
    static FieldInfo *s_ReflectionMissingField;
    static Il2CppClass *s_System_Reflection_ParameterInfo;
    static Il2CppClass *s_System_Reflection_ParameterInfo_array;
/*
 * We use the same C representation for methods and constructors, but the type
 * name in C# is different.
 */
    static Il2CppClass *s_System_Reflection_MethodInfo;
    static Il2CppClass *s_System_Reflection_ConstructorInfo;

    Il2CppReflectionAssembly* Reflection::GetAssemblyObject(const Il2CppAssembly *assembly)
    {
        Il2CppReflectionAssembly *res;

        AssemblyMap::key_type::wrapped_type key(assembly, (Il2CppClass*)NULL);
        AssemblyMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_AssemblyMap->TryGetValue(key, &value))
                return value;
        }

        res = (Il2CppReflectionAssembly*)Object::New(s_System_Reflection_Assembly);
        res->assembly = assembly;

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_AssemblyMap->TryGetValue(key, &value))
            return value;

        s_AssemblyMap->Add(key, res);
        return res;
    }

    Il2CppReflectionAssemblyName* Reflection::GetAssemblyNameObject(const Il2CppAssemblyName *assemblyName)
    {
        IL2CPP_ASSERT(il2cpp_defaults.assembly_name_class != NULL);

        std::string fullAssemblyName = vm::AssemblyName::AssemblyNameToString(*assemblyName);
        Il2CppReflectionAssemblyName* reflectionAssemblyName = (Il2CppReflectionAssemblyName*)Object::New(il2cpp_defaults.assembly_name_class);
        vm::AssemblyName::ParseName(reflectionAssemblyName, fullAssemblyName);
        return reflectionAssemblyName;
    }

    Il2CppReflectionField* Reflection::GetFieldObject(Il2CppClass *klass, FieldInfo *field)
    {
        Il2CppReflectionField *res;

        FieldMap::key_type::wrapped_type key(field, klass);
        FieldMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_FieldMap->TryGetValue(key, &value))
                return value;
        }

        res = (Il2CppReflectionField*)Object::New(s_System_Reflection_RuntimeFieldInfoKlass);
        res->klass = klass;
        res->field = field;
        IL2CPP_OBJECT_SETREF(res, name, String::New(Field::GetName(field)));
        res->attrs = field->type->attrs;
        IL2CPP_OBJECT_SETREF(res, type, GetTypeObject(field->type));

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_FieldMap->TryGetValue(key, &value))
            return value;

        s_FieldMap->Add(key, res);
        return res;
    }

    const MethodInfo* Reflection::GetMethod(const Il2CppReflectionMethod* method)
    {
        return method->method;
    }

    Il2CppReflectionMethod* Reflection::GetMethodObject(const MethodInfo *method, Il2CppClass *refclass)
    {
        Il2CppClass *klass;
        Il2CppReflectionMethod *ret;

        if (!refclass)
            refclass = method->klass;

        MethodMap::key_type::wrapped_type key(method, refclass);
        MethodMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_MethodMap->TryGetValue(key, &value))
                return value;
        }

        if (*method->name == '.' && (strcmp(method->name, ".ctor") == 0 || strcmp(method->name, ".cctor") == 0))
        {
            klass = s_System_Reflection_ConstructorInfo;
        }
        else
        {
            klass = s_System_Reflection_MethodInfo;
        }
        ret = (Il2CppReflectionMethod*)Object::New(klass);
        ret->method = method;
        IL2CPP_OBJECT_SETREF(ret, reftype, GetTypeObject(&refclass->byval_arg));

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_MethodMap->TryGetValue(key, &value))
            return value;

        s_MethodMap->Add(key, ret);
        return ret;
    }

    Il2CppReflectionModule* Reflection::GetModuleObject(const Il2CppImage *image)
    {
        Il2CppReflectionModule *res;
        //char* basename;

        ModuleMap::key_type::wrapped_type key(image, (Il2CppClass*)NULL);
        ModuleMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_ModuleMap->TryGetValue(key, &value))
                return value;
        }

        res = (Il2CppReflectionModule*)Object::New(s_System_Reflection_Module);

        res->image = image;
        IL2CPP_OBJECT_SETREF(res, assembly, Reflection::GetAssemblyObject(image->assembly));

        IL2CPP_OBJECT_SETREF(res, fqname, String::New(image->name));
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Reflection::GetModuleObject, "Missing Module fields need set");
        //basename = g_path_get_basename (image->name);
        //IL2CPP_OBJECT_SETREF (res, name, String::New (basename));
        IL2CPP_OBJECT_SETREF(res, name, String::New(image->name));
        IL2CPP_OBJECT_SETREF(res, scopename, String::New(image->nameNoExt));

        //g_free (basename);

        /*if (image->assembly->image == image) {
            res->token = mono_metadata_make_token (MONO_TABLE_MODULE, 1);
        } else {
            int i;
            res->token = 0;
            if (image->assembly->image->modules) {
                for (i = 0; i < image->assembly->image->module_count; i++) {
                    if (image->assembly->image->modules [i] == image)
                        res->token = mono_metadata_make_token (MONO_TABLE_MODULEREF, i + 1);
                }
                IL2CPP_ASSERT(res->token);
            }
        }*/

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_ModuleMap->TryGetValue(key, &value))
            return value;

        s_ModuleMap->Add(key, res);
        return res;
    }

    Il2CppReflectionProperty* Reflection::GetPropertyObject(Il2CppClass *klass, const PropertyInfo *property)
    {
        Il2CppReflectionProperty *res;

        PropertyMap::key_type::wrapped_type key(property, klass);
        PropertyMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_PropertyMap->TryGetValue(key, &value))
                return value;
        }

        res = (Il2CppReflectionProperty*)Object::New(s_System_Reflection_RuntimePropertyInfoKlass);
        res->klass = klass;
        res->property = property;

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_PropertyMap->TryGetValue(key, &value))
            return value;

        s_PropertyMap->Add(key, res);
        return res;
    }

    Il2CppReflectionEvent* Reflection::GetEventObject(Il2CppClass* klass, const EventInfo* event)
    {
        Il2CppReflectionEvent* result;

        EventMap::key_type::wrapped_type key(event, klass);
        EventMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_EventMap->TryGetValue(key, &value))
                return value;
        }

        Il2CppReflectionMonoEvent* monoEvent = reinterpret_cast<Il2CppReflectionMonoEvent*>(Object::New(s_System_Reflection_RuntimeEventInfoKlass));
        monoEvent->eventInfo = event;
        monoEvent->reflectedType = Reflection::GetTypeObject(&klass->byval_arg);
        result = reinterpret_cast<Il2CppReflectionEvent*>(monoEvent);

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_EventMap->TryGetValue(key, &value))
            return value;

        s_EventMap->Add(key, result);
        return result;
    }

    Il2CppReflectionType* Reflection::GetTypeObject(const Il2CppType *type)
    {
        Il2CppReflectionType* object = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_TypeMap->TryGetValue(type, &object))
                return object;
        }

        Il2CppReflectionType* typeObject = (Il2CppReflectionType*)Object::New(il2cpp_defaults.runtimetype_class);

        typeObject->type = type;

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_TypeMap->TryGetValue(type, &object))
            return object;

        s_TypeMap->Add(type, typeObject);
        return typeObject;
    }

    Il2CppObject* Reflection::GetDBNullObject()
    {
        Il2CppObject* valueFieldValue;

        if (!s_DbNullValueField)
        {
            s_DbNullValueField = Class::GetFieldFromName(il2cpp_defaults.dbnull_class, "Value");
            IL2CPP_ASSERT(s_DbNullValueField);
        }

        valueFieldValue = Field::GetValueObject(s_DbNullValueField, NULL);
        IL2CPP_ASSERT(valueFieldValue);
        return valueFieldValue;
    }

    static Il2CppObject* GetReflectionMissingObject()
    {
        Il2CppObject* valueFieldValue;

        if (!s_ReflectionMissingField)
        {
            Il2CppClass* klass = Image::ClassFromName(il2cpp_defaults.corlib, "System.Reflection", "Missing");
            Class::Init(klass);
            s_ReflectionMissingField = Class::GetFieldFromName(klass, "Value");
            IL2CPP_ASSERT(s_ReflectionMissingField);
        }

        valueFieldValue = Field::GetValueObject(s_ReflectionMissingField, NULL);
        IL2CPP_ASSERT(valueFieldValue);
        return valueFieldValue;
    }

    static Il2CppObject* GetObjectForMissingDefaultValue(uint32_t parameterAttributes)
    {
        if (parameterAttributes & PARAM_ATTRIBUTE_OPTIONAL)
            return GetReflectionMissingObject();
        else
            return Reflection::GetDBNullObject();
    }

    Il2CppArray* Reflection::GetParamObjects(const MethodInfo *method, Il2CppClass *refclass)
    {
        Il2CppArray *res = NULL;
        Il2CppReflectionMethod *member = NULL;

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Reflection::GetParamObjects, "Work in progress!");

        if (!method->parameters_count)
            return Array::NewSpecific(s_System_Reflection_ParameterInfo_array, 0);

        // Mono caches based on the address of the method pointer in the MethodInfo
        // since they put everything in one cache and the MethodInfo is already used as key for GetMethodObject caching
        // However, since we have distinct maps for the different types we can use MethodInfo as the key again

        ParametersMap::key_type::wrapped_type key(method, refclass);
        ParametersMap::data_type value;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_ParametersMap->TryGetValue(key, &value))
                return value;
        }

        member = GetMethodObject(method, refclass);
        res = Array::NewSpecific(s_System_Reflection_ParameterInfo_array, method->parameters_count);
        for (int i = 0; i < method->parameters_count; ++i)
        {
            Il2CppReflectionParameter* param = (Il2CppReflectionParameter*)Object::New(s_System_Reflection_ParameterInfo);
            IL2CPP_OBJECT_SETREF(param, ClassImpl, GetTypeObject(method->parameters[i]));
            IL2CPP_OBJECT_SETREF(param, MemberImpl, (Il2CppObject*)member);
            const char* parameter_name = Method::GetParamName(method, i);
            IL2CPP_OBJECT_SETREF(param, NameImpl, parameter_name ? String::New(parameter_name) : NULL);
            param->PositionImpl = i;
            param->AttrsImpl = method->parameters[i]->attrs;

            Il2CppObject* defaultValue = NULL;
            if (param->AttrsImpl & PARAM_ATTRIBUTE_HAS_DEFAULT)
            {
                bool isExplicitySetNullDefaultValue = false;
                defaultValue = Parameter::GetDefaultParameterValueObject(method, i, &isExplicitySetNullDefaultValue);
                if (defaultValue == NULL && !isExplicitySetNullDefaultValue)
                    defaultValue = GetObjectForMissingDefaultValue(param->AttrsImpl);
            }
            else
            {
                defaultValue = GetObjectForMissingDefaultValue(param->AttrsImpl);
            }

            IL2CPP_OBJECT_SETREF(param, DefaultValueImpl, defaultValue);

            il2cpp_array_setref(res, i, param);
        }

        il2cpp::os::ReaderWriterAutoLock lockExclusive(&s_ReflectionICallsLock, true);
        if (s_ParametersMap->TryGetValue(key, &value))
            return value;

        s_ParametersMap->Add(key, res);
        return res;
    }

// TODO: move this somewhere else
    bool Reflection::IsType(Il2CppObject *obj)
    {
        return (obj->klass == il2cpp_defaults.runtimetype_class);
    }

    static bool IsMethod(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "RuntimeMethodInfo") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsCMethod(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "RuntimeConstructorInfo") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsAnyMethod(Il2CppObject *obj)
    {
        return IsMethod(obj) || IsCMethod(obj);
    }

    bool Reflection::IsField(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "RuntimeFieldInfo") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsProperty(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "RuntimePropertyInfo") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsEvent(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "RuntimeEventInfo") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsParameter(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return obj->klass == il2cpp_defaults.parameter_info_class;
        return false;
    }

    static bool IsAssembly(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return obj->klass == s_System_Reflection_Assembly->klass;
        return false;
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(Il2CppClass *klass)
    {
        return MetadataCache::GenerateCustomAttributesCache(klass->image, klass->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(const MethodInfo *method)
    {
        return MetadataCache::GenerateCustomAttributesCache(method->klass->image, method->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(const PropertyInfo *property)
    {
        return MetadataCache::GenerateCustomAttributesCache(property->parent->image, property->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(FieldInfo *field)
    {
        return MetadataCache::GenerateCustomAttributesCache(field->parent->image, field->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(const EventInfo *event)
    {
        return MetadataCache::GenerateCustomAttributesCache(event->parent->image, event->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(Il2CppReflectionParameter *parameter)
    {
        Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)parameter->MemberImpl;

        if (method->method->parameters == NULL)
            return NULL;

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Reflection::GetCustomAttributesCacheFor, "-1 represents the return value. Need to emit custom attribute information for that.")
        if (parameter->PositionImpl == -1)
            return NULL;

        const MethodInfo* methodWithParameterAttributeInformation = method->method;
        if (method->method->is_inflated)
            methodWithParameterAttributeInformation = method->method->genericMethod->methodDefinition;

        return MetadataCache::GenerateCustomAttributesCache(methodWithParameterAttributeInformation->klass->image, Method::GetParameterToken(method->method, parameter->PositionImpl));
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(Il2CppClass *klass)
    {
        return MetadataCache::GetCustomAttributeDataRange(klass->image, klass->token);
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(const MethodInfo *method)
    {
        return MetadataCache::GetCustomAttributeDataRange(method->klass->image, method->token);
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(const PropertyInfo *property)
    {
        return MetadataCache::GetCustomAttributeDataRange(property->parent->image, property->token);
    }

    std::tuple<void*, void*>  Reflection::GetCustomAttributesDataRangeFor(FieldInfo *field)
    {
        return MetadataCache::GetCustomAttributeDataRange(field->parent->image, field->token);
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(const EventInfo *event)
    {
        return MetadataCache::GetCustomAttributeDataRange(event->parent->image, event->token);
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(Il2CppReflectionParameter *parameter)
    {
        Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)parameter->MemberImpl;

        if (method->method->parameters == NULL)
            return std::make_tuple<void*, void*>(NULL, NULL);

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Reflection::GetCustomAttributesDataRangeFor, "-1 represents the return value. Need to emit custom attribute information for that.")
        if (parameter->PositionImpl == -1)
            return std::make_tuple<void*, void*>(NULL, NULL);

        const MethodInfo* methodWithParameterAttributeInformation = method->method;
        if (method->method->is_inflated)
            methodWithParameterAttributeInformation = method->method->genericMethod->methodDefinition;

        return MetadataCache::GetCustomAttributeDataRange(methodWithParameterAttributeInformation->klass->image, Method::GetParameterToken(method->method, parameter->PositionImpl));
    }

    std::tuple<void*, void*> Reflection::GetCustomAttributesDataRangeFor(const Il2CppAssembly *assembly)
    {
        return MetadataCache::GetCustomAttributeDataRange(assembly->image, assembly->token);
    }

    int Reflection::GetMetadataToken(Il2CppObject* obj)
    {
        if (vm::Reflection::IsField(obj))
        {
            Il2CppReflectionField* field = (Il2CppReflectionField*)obj;
            return vm::Field::GetToken(field->field);
        }
        else if (vm::Reflection::IsAnyMethod(obj))
        {
            Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)obj;
            return vm::Method::GetToken(method->method);
        }
        else if (vm::Reflection::IsProperty(obj))
        {
            Il2CppReflectionProperty* prop = (Il2CppReflectionProperty*)obj;
            return vm::Property::GetToken(prop->property);
        }
        else if (vm::Reflection::IsEvent(obj))
        {
            Il2CppReflectionMonoEvent* eventInfo = (Il2CppReflectionMonoEvent*)obj;
            return vm::Event::GetToken(eventInfo->eventInfo);
        }
        else if (vm::Reflection::IsType(obj))
        {
            Il2CppReflectionType* type = (Il2CppReflectionType*)obj;
            return vm::Type::GetToken(type->type);
        }
        else if (IsParameter(obj))
        {
            Il2CppReflectionParameter* parameter = (Il2CppReflectionParameter*)obj;
            if (parameter->PositionImpl == -1)
                return 0x8000000; // This is what mono returns as a fixed value.

            Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)parameter->MemberImpl;
            return vm::Method::GetParameterToken(method->method, parameter->PositionImpl);
        }
        else
        {
            NOT_SUPPORTED_IL2CPP(MemberInfo::get_MetadataToken, "This icall is not supported by il2cpp.");
        }

        return 0;
    }

    bool Reflection::HasAttribute(Il2CppReflectionParameter *parameter, Il2CppClass* attribute)
    {
        Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)parameter->MemberImpl;

        if (method->method->parameters == NULL)
            return false;

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Reflection::GetCustomAttributeTypeCacheFor, "-1 represents the return value. Need to emit custom attribute information for that.")
        if (parameter->PositionImpl == -1)
            return false;

        const MethodInfo* methodWithParameterAttributeInformation = method->method;
        if (method->method->is_inflated)
            methodWithParameterAttributeInformation = method->method->genericMethod->methodDefinition;

        return MetadataCache::HasAttribute(methodWithParameterAttributeInformation->klass->image, Method::GetParameterToken(method->method, parameter->PositionImpl), attribute);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(const Il2CppAssembly *assembly)
    {
        return MetadataCache::GenerateCustomAttributesCache(assembly->image, assembly->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttrsInfo(Il2CppObject *obj)
    {
        if (IsMethod(obj) || IsCMethod(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionMethod*)obj)->method);

        if (IsProperty(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionProperty*)obj)->property);

        if (IsField(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionField*)obj)->field);

        if (IsEvent(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionMonoEvent*)obj)->eventInfo);

        if (IsParameter(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionParameter*)obj));

        if (IsAssembly(obj))
            return GetCustomAttributesCacheFor(((Il2CppReflectionAssembly*)obj)->assembly);

        Il2CppClass *klass = IsType(obj)
            ? Class::FromSystemType((Il2CppReflectionType*)obj)
            : obj->klass;

        return GetCustomAttributesCacheFor(klass);
    }

    il2cpp::metadata::CustomAttributeDataReader Reflection::GetCustomAttrsDataReader(Il2CppObject* obj)
    {
        const Il2CppImage* image;
        std::tuple<void*, void*> dataRange;

        if (IsMethod(obj) || IsCMethod(obj))
        {
            const MethodInfo* method = ((Il2CppReflectionMethod*)obj)->method;
            image = method->klass->image;
            dataRange = GetCustomAttributesDataRangeFor(method);
        }
        else if (IsProperty(obj))
        {
            const PropertyInfo* prop = ((Il2CppReflectionProperty*)obj)->property;
            image = prop->parent->image;
            dataRange = GetCustomAttributesDataRangeFor(prop);
        }
        else if (IsField(obj))
        {
            FieldInfo* field = ((Il2CppReflectionField*)obj)->field;
            image = field->parent->image;
            dataRange = GetCustomAttributesDataRangeFor(field);
        }
        else if (IsEvent(obj))
        {
            const EventInfo* eventInfo = ((Il2CppReflectionMonoEvent*)obj)->eventInfo;
            image = eventInfo->parent->image;
            dataRange = GetCustomAttributesDataRangeFor(eventInfo);
        }
        else if (IsParameter(obj))
        {
            Il2CppReflectionParameter* parameter = (Il2CppReflectionParameter*)obj;
            Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)parameter->MemberImpl;
            image = method->method->klass->image;
            dataRange = GetCustomAttributesDataRangeFor(parameter);
        }
        else if (IsAssembly(obj))
        {
            const Il2CppAssembly* assembly = ((Il2CppReflectionAssembly*)obj)->assembly;
            image = assembly->image;
            dataRange = GetCustomAttributesDataRangeFor(assembly);
        }
        else
        {
            Il2CppClass *klass = IsType(obj)
                ? Class::FromSystemType((Il2CppReflectionType*)obj)
                : obj->klass;

            image = klass->image;
            dataRange = GetCustomAttributesDataRangeFor(klass);
        }

        void* start;
        void* end;
        std::tie(start, end) = dataRange;
        return metadata::CustomAttributeDataReader(start, end);
    }

    bool Reflection::HasAttribute(Il2CppObject *obj, Il2CppClass* attribute)
    {
        if (IsMethod(obj) || IsCMethod(obj))
            return MetadataCache::HasAttribute((((Il2CppReflectionMethod*)obj)->method)->klass->image, (((Il2CppReflectionMethod*)obj)->method)->token, attribute);

        if (IsProperty(obj))
            return MetadataCache::HasAttribute((((Il2CppReflectionProperty*)obj)->property)->parent->image, (((Il2CppReflectionProperty*)obj)->property)->token, attribute);

        if (IsField(obj))
            return MetadataCache::HasAttribute((((Il2CppReflectionField*)obj)->field)->parent->image, (((Il2CppReflectionField*)obj)->field)->token, attribute);

        if (IsEvent(obj))
            return MetadataCache::HasAttribute((((Il2CppReflectionMonoEvent*)obj)->eventInfo)->parent->image, (((Il2CppReflectionMonoEvent*)obj)->eventInfo)->token, attribute);

        if (IsParameter(obj))
            return HasAttribute((Il2CppReflectionParameter*)obj, attribute);

        if (IsAssembly(obj))
            return MetadataCache::HasAttribute((((Il2CppReflectionAssembly*)obj)->assembly)->image, (((Il2CppReflectionAssembly*)obj)->assembly)->token, attribute);

        Il2CppClass *klass = IsType(obj)
            ? Class::FromSystemType((Il2CppReflectionType*)obj)
            : obj->klass;

        return MetadataCache::HasAttribute(klass->image, klass->token, attribute);
    }

    Il2CppObject* Reflection::GetCustomAttribute(Il2CppMetadataCustomAttributeHandle token, Il2CppClass* attribute)
    {
        CustomAttributesCache* cache = MetadataCache::GenerateCustomAttributesCache(token);
        if (cache == NULL)
            return NULL;

        for (int32_t i = 0; i < cache->count; i++)
        {
            Il2CppObject* obj = cache->attributes[i];
            Il2CppClass* klass = Object::GetClass(obj);

            if (Class::HasParent(klass, attribute) || (Class::IsInterface(attribute) && Class::IsAssignableFrom(attribute, klass)))
                return obj;
        }

        return NULL;
    }

    Il2CppArray* Reflection::ConstructCustomAttributes(Il2CppMetadataCustomAttributeHandle token)
    {
        CustomAttributesCache* cache = MetadataCache::GenerateCustomAttributesCache(token);
        if (cache == NULL)
            return il2cpp::vm::Array::New(il2cpp_defaults.attribute_class, 0);

        Il2CppArray* result = il2cpp::vm::Array::New(il2cpp_defaults.attribute_class, cache->count);
        for (int32_t i = 0; i < cache->count; i++)
            il2cpp_array_setref(result, i, cache->attributes[i]);

        return result;
    }

    void Reflection::Initialize()
    {
        s_AssemblyMap = new AssemblyMap();
        s_FieldMap = new FieldMap();
        s_PropertyMap = new PropertyMap();
        s_EventMap = new EventMap();
        s_MethodMap = new MethodMap();
        s_ModuleMap = new ModuleMap();
        s_ParametersMap = new ParametersMap();
        s_TypeMap = new TypeMap();
        s_MonoGenericParamterMap = new MonoGenericParameterMap();
        s_MonoAssemblyNameMap = new MonoAssemblyNameMap();

        s_System_Reflection_Assembly = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeAssembly");
        IL2CPP_ASSERT(s_System_Reflection_Assembly != NULL);
#if !IL2CPP_TINY_DEBUGGER
        s_System_Reflection_Module = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeModule");
        IL2CPP_ASSERT(s_System_Reflection_Module != NULL);

        s_System_Reflection_ConstructorInfo = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeConstructorInfo");
        IL2CPP_ASSERT(s_System_Reflection_ConstructorInfo != NULL);
        s_System_Reflection_MethodInfo = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeMethodInfo");
        IL2CPP_ASSERT(s_System_Reflection_MethodInfo != NULL);
        s_System_Reflection_ParameterInfo = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeParameterInfo");
        IL2CPP_ASSERT(s_System_Reflection_ParameterInfo != NULL);
        s_System_Reflection_ParameterInfo_array = Class::GetArrayClass(s_System_Reflection_ParameterInfo, 1);
        IL2CPP_ASSERT(s_System_Reflection_ParameterInfo_array != NULL);

        s_System_Reflection_RuntimeFieldInfoKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeFieldInfo");
        IL2CPP_ASSERT(s_System_Reflection_RuntimeFieldInfoKlass != NULL);
        s_System_Reflection_RuntimeEventInfoKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimeEventInfo");
        IL2CPP_ASSERT(s_System_Reflection_RuntimeEventInfoKlass != NULL);
        s_System_Reflection_RuntimePropertyInfoKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "RuntimePropertyInfo");
        IL2CPP_ASSERT(s_System_Reflection_RuntimePropertyInfoKlass != NULL);
#endif
    }

    bool Reflection::HasAttribute(FieldInfo *field, Il2CppClass *attribute)
    {
        return MetadataCache::HasAttribute(field->parent->image, field->token, attribute);
    }

    bool Reflection::HasAttribute(const MethodInfo *method, Il2CppClass *attribute)
    {
        return MetadataCache::HasAttribute(method->klass->image, method->token, attribute);
    }

    bool Reflection::HasAttribute(Il2CppClass *klass, Il2CppClass *attribute)
    {
        return MetadataCache::HasAttribute(klass->image, klass->token, attribute);
    }

    Il2CppClass* Reflection::TypeGetHandle(Il2CppReflectionType* ref)
    {
        if (!ref)
            return NULL;

        return Class::FromSystemType(ref);
    }

    const MonoGenericParameterInfo* Reflection::GetMonoGenericParameterInfo(Il2CppMetadataGenericParameterHandle param)
    {
        MonoGenericParameterMap::const_iterator it = s_MonoGenericParamterMap->find(param);
        if (it == s_MonoGenericParamterMap->end())
            return NULL;

        return it->second;
    }

    void Reflection::SetMonoGenericParameterInfo(Il2CppMetadataGenericParameterHandle param, const MonoGenericParameterInfo *monoParam)
    {
        s_MonoGenericParamterMap->insert(std::make_pair(param, monoParam));
    }

    const Il2CppMonoAssemblyName* Reflection::GetMonoAssemblyName(const Il2CppAssembly *assembly)
    {
        MonoAssemblyNameMap::const_iterator it = s_MonoAssemblyNameMap->find(assembly);
        if (it == s_MonoAssemblyNameMap->end())
            return NULL;

        return it->second;
    }

    void Reflection::SetMonoAssemblyName(const Il2CppAssembly *assembly, const Il2CppMonoAssemblyName *aname)
    {
        s_MonoAssemblyNameMap->insert(std::make_pair(assembly, aname));
    }

    void Reflection::ClearStatics()
    {
        s_System_Reflection_Assembly = NULL;
        s_System_Reflection_RuntimeFieldInfoKlass = NULL;
        s_System_Reflection_Module = NULL;
        s_System_Reflection_RuntimePropertyInfoKlass = NULL;
        s_System_Reflection_RuntimeEventInfoKlass = NULL;
        s_DbNullValueField = NULL;
        s_ReflectionMissingField = NULL;
        s_System_Reflection_ParameterInfo = NULL;
        s_System_Reflection_ParameterInfo_array = NULL;

        s_System_Reflection_MethodInfo = NULL;
        s_System_Reflection_ConstructorInfo = NULL;
    }
} /* namespace vm */
} /* namespace il2cpp */
