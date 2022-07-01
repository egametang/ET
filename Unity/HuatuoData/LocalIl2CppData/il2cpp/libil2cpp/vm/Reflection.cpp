#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-tabledefs.h"
#include "mono-structs.h"
#include "gc/GCHandle.h"
#include "metadata/Il2CppTypeCompare.h"
#include "metadata/Il2CppTypeHash.h"
#include "os/ReaderWriterLock.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Field.h"
#include "vm/Image.h"
#include "vm/MetadataCache.h"
#include "vm/Object.h"
#include "vm/Parameter.h"
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
    static Il2CppClass *s_MonoFieldKlass;
    static Il2CppClass *s_System_Reflection_Module;
    static Il2CppClass *s_MonoPropertyKlass;
    static Il2CppClass *s_MonoEventKlass;
    static FieldInfo *s_DbNullValueField;
    static FieldInfo *s_ReflectionMissingField;
    static Il2CppClass *s_System_Reflection_ParameterInfo;
    static Il2CppClass *s_System_Reflection_ParameterInfo_array;
    static Il2CppClass *s_System_Reflection_ConstructorInfo;
/*
 * We use the same C representation for methods and constructors, but the type
 * name in C# is different.
 */
    static Il2CppClass *System_Reflection_MonoMethod;
    static Il2CppClass *System_Reflection_MonoCMethod;

    static Il2CppClass *System_Reflection_MonoGenericCMethod;
    static Il2CppClass *System_Reflection_MonoGenericMethod;

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

        if (!s_System_Reflection_Assembly)
            s_System_Reflection_Assembly = il2cpp_defaults.mono_assembly_class;
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

        if (!s_MonoFieldKlass)
            s_MonoFieldKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoField");
        res = (Il2CppReflectionField*)Object::New(s_MonoFieldKlass);
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
            if (!System_Reflection_MonoCMethod)
                System_Reflection_MonoCMethod = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoCMethod");
            klass = System_Reflection_MonoCMethod;
        }
        else
        {
            if (!System_Reflection_MonoMethod)
                System_Reflection_MonoMethod = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoMethod");
            klass = System_Reflection_MonoMethod;
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

        if (!s_System_Reflection_Module)
        {
            s_System_Reflection_Module = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoModule");
        }
        res = (Il2CppReflectionModule*)Object::New(s_System_Reflection_Module);

        res->image = image;
        IL2CPP_OBJECT_SETREF(res, assembly, (Il2CppReflectionAssembly*)Reflection::GetAssemblyObject(image->assembly));

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

        if (!s_MonoPropertyKlass)
            s_MonoPropertyKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoProperty");
        res = (Il2CppReflectionProperty*)Object::New(s_MonoPropertyKlass);
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
        if (s_MonoEventKlass == NULL)
            s_MonoEventKlass = Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "MonoEvent");

        EventMap::key_type::wrapped_type key(event, klass);
        EventMap::data_type value = NULL;

        {
            il2cpp::os::ReaderWriterAutoLock lockShared(&s_ReflectionICallsLock);
            if (s_EventMap->TryGetValue(key, &value))
                return value;
        }

        Il2CppReflectionMonoEvent* monoEvent = reinterpret_cast<Il2CppReflectionMonoEvent*>(Object::New(s_MonoEventKlass));
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

    Il2CppClass* Reflection::GetConstructorInfo()
    {
        if (s_System_Reflection_ConstructorInfo == NULL)
            s_System_Reflection_ConstructorInfo = vm::Class::FromName(il2cpp_defaults.corlib, "System.Reflection", "ConstructorInfo");
        return s_System_Reflection_ConstructorInfo;
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

        if (!s_System_Reflection_ParameterInfo_array)
        {
            Il2CppClass *klass;

            klass = il2cpp_defaults.mono_parameter_info_class;
            //mono_memory_barrier ();
            s_System_Reflection_ParameterInfo = klass;

            klass = Class::GetArrayClass(klass, 1);
            //mono_memory_barrier ();
            s_System_Reflection_ParameterInfo_array = klass;
        }

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
            IL2CPP_OBJECT_SETREF(param, ClassImpl, GetTypeObject(method->parameters[i].parameter_type));
            IL2CPP_OBJECT_SETREF(param, MemberImpl, (Il2CppObject*)member);
            IL2CPP_OBJECT_SETREF(param, NameImpl, method->parameters[i].name ? String::New(method->parameters[i].name) : NULL);
            param->PositionImpl = i;
            param->AttrsImpl = method->parameters[i].parameter_type->attrs;

            Il2CppObject* defaultValue = NULL;
            if (param->AttrsImpl & PARAM_ATTRIBUTE_HAS_DEFAULT)
            {
                bool isExplicitySetNullDefaultValue = false;
                defaultValue = Parameter::GetDefaultParameterValueObject(method, &method->parameters[i], &isExplicitySetNullDefaultValue);
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
            return strcmp(obj->klass->name, "MonoMethod") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsCMethod(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoCMethod") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsGenericMethod(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoGenericMethod") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsGenericCMethod(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoGenericCMethod") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsAnyMethod(Il2CppObject *obj)
    {
        return IsMethod(obj) || IsCMethod(obj) || IsGenericMethod(obj) || IsGenericCMethod(obj);
    }

    bool Reflection::IsField(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoField") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsProperty(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoProperty") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    bool Reflection::IsEvent(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return strcmp(obj->klass->name, "MonoEvent") == 0 && strcmp(obj->klass->namespaze, "System.Reflection") == 0;
        return false;
    }

    static bool IsParameter(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return obj->klass == il2cpp_defaults.mono_parameter_info_class;
        return false;
    }

    static bool IsAssembly(Il2CppObject *obj)
    {
        if (obj->klass->image == il2cpp_defaults.corlib)
            return obj->klass == il2cpp_defaults.mono_assembly_class;
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

        const ::ParameterInfo* info = &methodWithParameterAttributeInformation->parameters[parameter->PositionImpl];
        return MetadataCache::GenerateCustomAttributesCache(methodWithParameterAttributeInformation->klass->image, info->token);
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

        const ::ParameterInfo* info = &methodWithParameterAttributeInformation->parameters[parameter->PositionImpl];
        return MetadataCache::HasAttribute(methodWithParameterAttributeInformation->klass->image, info->token, attribute);
    }

    CustomAttributesCache* Reflection::GetCustomAttributesCacheFor(const Il2CppAssembly *assembly)
    {
        return MetadataCache::GenerateCustomAttributesCache(assembly->image, assembly->token);
    }

    CustomAttributesCache* Reflection::GetCustomAttrsInfo(Il2CppObject *obj)
    {
        if (IsMethod(obj) || IsCMethod(obj) || IsGenericMethod(obj) || IsGenericCMethod(obj))
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

    bool Reflection::HasAttribute(Il2CppObject *obj, Il2CppClass* attribute)
    {
        if (IsMethod(obj) || IsCMethod(obj) || IsGenericMethod(obj) || IsGenericCMethod(obj))
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
        s_MonoFieldKlass = NULL;
        s_System_Reflection_Module = NULL;
        s_MonoPropertyKlass = NULL;
        s_MonoEventKlass = NULL;
        s_DbNullValueField = NULL;
        s_ReflectionMissingField = NULL;
        s_System_Reflection_ParameterInfo = NULL;
        s_System_Reflection_ParameterInfo_array = NULL;
        s_System_Reflection_ConstructorInfo = NULL;

        System_Reflection_MonoMethod = NULL;
        System_Reflection_MonoCMethod = NULL;

        System_Reflection_MonoGenericCMethod = NULL;
        System_Reflection_MonoGenericMethod = NULL;
    }
} /* namespace vm */
} /* namespace il2cpp */
