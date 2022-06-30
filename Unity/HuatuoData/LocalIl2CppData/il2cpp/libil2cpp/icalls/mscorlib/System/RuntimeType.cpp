#include "il2cpp-config.h"

#include "il2cpp-runtime-metadata.h"
#include "RuntimeType.h"
#include "Type.h"
#include "metadata/GenericMetadata.h"
#include "vm/Array.h"
#include "vm/Type.h"
#include "vm/Class.h"
#include "vm/Field.h"
#include "vm/MetadataCache.h"
#include "vm/Method.h"
#include "vm/String.h"
#include "vm/GenericClass.h"
#include "vm/Reflection.h"
#include "vm/ClassInlines.h"
#include "utils/Functional.h"
#include "utils/dynamic_array.h"
#include "utils/Il2CppHashSet.h"
#include "utils/StringUtils.h"
#include "il2cpp-api.h"
#include "il2cpp-tabledefs.h"
#include "mono-structs.h"
#include "RuntimeTypeHandle.h"
#include "vm-utils/VmStringUtils.h"
#include <vm/Reflection.h>

#include <vector>
#include <set>

typedef int32_t BindingFlags;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    struct Il2CppEventInfoHash
    {
        size_t operator()(const EventInfo* eventInfo) const
        {
            return il2cpp::utils::StringUtils::Hash(eventInfo->name);
        }
    };

    struct Il2CppEventInfoCompare
    {
        bool operator()(const EventInfo* event1, const EventInfo* event2) const
        {
            // You can't overload events
            return strcmp(event1->name, event2->name) == 0;
        }
    };

    typedef Il2CppHashMap<const EventInfo*, Il2CppClass*, Il2CppEventInfoHash, Il2CppEventInfoCompare> EventMap;

    struct PropertyPair
    {
        const PropertyInfo *property;
        Il2CppClass* originalType;

        PropertyPair(const PropertyInfo *property, Il2CppClass* originalType) : property(property), originalType(originalType)
        {
        }
    };

    typedef std::vector<PropertyPair> PropertyPairVector;

    static bool PropertyEqual(const PropertyInfo* prop1, const PropertyInfo* prop2)
    {
        // Name check is not enough, property can be overloaded
        if (strcmp(prop1->name, prop2->name) != 0)
            return false;

        return vm::Method::IsSameOverloadSignature(prop1, prop2);
    }

    static bool PropertyPairVectorContains(const PropertyPairVector& properties, const PropertyInfo* property)
    {
        for (PropertyPairVector::const_iterator it = properties.begin(), end = properties.end(); it != end; ++it)
            if (PropertyEqual(it->property, property))
                return true;

        return false;
    }

    int32_t RuntimeType::GetGenericParameterPosition(Il2CppReflectionRuntimeType* type)
    {
        if (RuntimeTypeHandle::IsGenericVariable(type))
            return vm::Type::GetGenericParameterInfo(type->type.type).num;
        return -1;
    }

    static inline bool IsPublic(const FieldInfo* field)
    {
        return (field->type->attrs & FIELD_ATTRIBUTE_FIELD_ACCESS_MASK) == FIELD_ATTRIBUTE_PUBLIC;
    }

    static inline bool IsPrivate(const FieldInfo* field)
    {
        return (field->type->attrs & FIELD_ATTRIBUTE_FIELD_ACCESS_MASK) == FIELD_ATTRIBUTE_PRIVATE;
    }

    static inline bool IsStatic(const FieldInfo* field)
    {
        return (field->type->attrs & FIELD_ATTRIBUTE_STATIC) != 0;
    }

    static inline bool IsPublic(const PropertyInfo* property)
    {
        if (property->get != NULL && (property->get->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            return true;

        if (property->set != NULL && (property->set->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            return true;

        return false;
    }

    static inline bool IsPrivate(const PropertyInfo* property)
    {
        if (property->get != NULL && (property->get->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PRIVATE)
            return false;

        if (property->set != NULL && (property->set->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PRIVATE)
            return false;

        return true;
    }

    static inline bool IsStatic(const PropertyInfo* property)
    {
        if (property->get != NULL)
            return (property->get->flags & METHOD_ATTRIBUTE_STATIC) != 0;

        if (property->set != NULL)
            return (property->set->flags & METHOD_ATTRIBUTE_STATIC) != 0;

        return false;
    }

    static inline bool IsPublic(const MethodInfo* method)
    {
        return (method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC;
    }

    static inline bool IsPrivate(const MethodInfo* method)
    {
        return (method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PRIVATE;
    }

    static inline bool IsStatic(const MethodInfo* method)
    {
        return (method->flags & METHOD_ATTRIBUTE_STATIC) != 0;
    }

    // From MSDN: An event is considered public to reflection if it has at least one method or accessor that is public.
    static inline bool IsPublic(const EventInfo* event)
    {
        if (event->add != NULL && (event->add->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            return true;

        if (event->remove != NULL && (event->remove->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            return true;

        if (event->raise != NULL && (event->raise->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            return true;

        return false;
    }

    static inline bool IsPrivate(const EventInfo* event)
    {
        if (event->add != NULL && (event->add->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PRIVATE)
            return false;

        if (event->remove != NULL && (event->remove->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PRIVATE)
            return false;

        if (event->raise != NULL && (event->raise->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) != METHOD_ATTRIBUTE_PRIVATE)
            return false;

        return true;
    }

    static inline bool IsStatic(const EventInfo* event)
    {
        if (event->add != NULL)
            return (event->add->flags & METHOD_ATTRIBUTE_STATIC) != 0;

        if (event->remove != NULL)
            return (event->remove->flags & METHOD_ATTRIBUTE_STATIC) != 0;

        if (event->raise != NULL)
            return (event->raise->flags & METHOD_ATTRIBUTE_STATIC) != 0;

        return false;
    }

    template<typename MemberInfo, typename NameFilter>
    static bool CheckMemberMatch(const MemberInfo* member, const Il2CppClass* type, const Il2CppClass* originalType, int32_t bindingFlags, const NameFilter& nameFilter)
    {
        uint32_t accessBindingFlag = IsPublic(member) ? BFLAGS_Public : BFLAGS_NonPublic;

        if ((bindingFlags & accessBindingFlag) == 0)
            return false;

        if (type != originalType && IsPrivate(member)) // Private members are not part of derived class
            return false;

        if (IsStatic(member))
        {
            if ((bindingFlags & BFLAGS_Static) == 0)
                return false;

            if ((bindingFlags & BFLAGS_FlattenHierarchy) == 0 && type != originalType)
                return false;
        }
        else if ((bindingFlags & BFLAGS_Instance) == 0)
        {
            return false;
        }

        if (!nameFilter(member->name))
            return false;

        return true;
    }

    template<typename NameFilter>
    static inline void CollectTypeEvents(Il2CppClass* type, Il2CppClass* const originalType, int32_t bindingFlags, EventMap& events, const NameFilter& nameFilter)
    {
        void* iter = NULL;
        while (const EventInfo* event = vm::Class::GetEvents(type, &iter))
        {
            if (CheckMemberMatch(event, type, originalType, bindingFlags, nameFilter))
            {
                if (events.find(event) != events.end())
                    continue;

                events[event] = originalType;
            }
        }
    }

    static inline bool ValidBindingFlagsForGetMember(uint32_t bindingFlags)
    {
        return (bindingFlags & BFLAGS_Static) != 0 || (bindingFlags & BFLAGS_Instance) != 0;
    }

    template<typename NameFilter>
    static inline Il2CppArray* GetEventsImpl(Il2CppReflectionType* type, int listType, Il2CppReflectionType* reflectedType, const NameFilter& nameFilter)
    {
        if (type->type->byref)
            return vm::Array::New(il2cpp_defaults.event_info_class, 0);

        EventMap events;
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type->type);

        CollectTypeEvents(typeInfo, typeInfo, BFLAGS_MatchAll, events, nameFilter);

        Il2CppClass* const originalType = typeInfo;
        typeInfo = vm::Class::GetParent(typeInfo);

        while (typeInfo != NULL)
        {
            CollectTypeEvents(typeInfo, originalType, BFLAGS_MatchAll, events, nameFilter);
            typeInfo = vm::Class::GetParent(typeInfo);
        }

        int i = 0;
        Il2CppArray* result = vm::Array::NewCached(il2cpp_defaults.event_info_class, (il2cpp_array_size_t)events.size());

        for (EventMap::const_iterator iter = events.begin(); iter != events.end(); iter++)
        {
            il2cpp_array_setref(result, i, vm::Reflection::GetEventObject(iter->second, iter->first.key));
            i++;
        }

        return result;
    }

    static Il2CppArray* GetEventsByName(Il2CppReflectionType* _this, Il2CppString* name, int listType, Il2CppReflectionType* reflectedType)
    {
        if (name == NULL)
            return GetEventsImpl(_this, listType, reflectedType, utils::functional::TrueFilter());

        if (listType == MLISTTYPE_CaseInsensitive)
        {
            return GetEventsImpl(_this, listType, reflectedType, utils::functional::Filter<std::string, utils::VmStringUtils::CaseInsensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
        }

        return GetEventsImpl(_this, listType, reflectedType, utils::functional::Filter<std::string, utils::VmStringUtils::CaseSensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
    }

    intptr_t RuntimeType::GetConstructors_native(Il2CppReflectionRuntimeType* thisPtr, int32_t bindingAttr)
    {
        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        VoidPtrArray res_array;
        res_array.reserve(4);

        Il2CppClass* startklass, * klass;
        const MethodInfo* method;
        int match;
        void* iter = NULL;

        klass = startklass = vm::Class::FromIl2CppType(thisPtr->type.type);

        iter = NULL;
        while ((method = vm::Class::GetMethods(klass, &iter)))
        {
            match = 0;
            if (strcmp(method->name, ".ctor") && strcmp(method->name, ".cctor"))
                continue;
            if ((method->flags & METHOD_ATTRIBUTE_MEMBER_ACCESS_MASK) == METHOD_ATTRIBUTE_PUBLIC)
            {
                if (bindingAttr & BFLAGS_Public)
                    match++;
            }
            else
            {
                if (bindingAttr & BFLAGS_NonPublic)
                    match++;
            }
            if (!match)
                continue;
            match = 0;
            if (method->flags & METHOD_ATTRIBUTE_STATIC)
            {
                if (bindingAttr & BFLAGS_Static)
                    if ((bindingAttr & BFLAGS_FlattenHierarchy) || (klass == startklass))
                        match++;
            }
            else
            {
                if (bindingAttr & BFLAGS_Instance)
                    match++;
            }

            if (!match)
                continue;

            res_array.push_back((void*)method);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetEvents_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t listType)
    {
        Il2CppReflectionMonoEvent *event;
        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        res_array.reserve(4);

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* events = GetEventsByName(&thisPtr->type, nameStr, listType, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(events); i++)
        {
            event = il2cpp_array_get(events, Il2CppReflectionMonoEvent*, i);
            res_array.push_back((EventInfo*)event->eventInfo);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    template<typename NameFilter>
    static inline void CollectTypeFields(Il2CppClass* type, const Il2CppClass* const originalType, int32_t bindingFlags, std::vector<FieldInfo*>& fields, const NameFilter& nameFilter)
    {
        void* iterator = NULL;
        FieldInfo* field = NULL;
        while ((field = vm::Class::GetFields(type, &iterator)) != NULL)
        {
            if (CheckMemberMatch(field, type, originalType, bindingFlags, nameFilter))
                fields.push_back(field);
        }
    }

    template<typename NameFilter>
    static inline Il2CppArray* GetFieldsImpl(Il2CppReflectionType* _this, int bindingFlags, Il2CppReflectionType* reflectedType, const NameFilter& nameFilter)
    {
        if (reflectedType->type->byref || !ValidBindingFlagsForGetMember(bindingFlags))
            return vm::Array::New(il2cpp_defaults.field_info_class, 0);

        std::vector<FieldInfo*> fields;
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(reflectedType->type);
        Il2CppClass* const originalType = typeInfo;

        CollectTypeFields(typeInfo, typeInfo, bindingFlags, fields, nameFilter);

        if ((bindingFlags & BFLAGS_DeclaredOnly) == 0)
        {
            typeInfo = typeInfo->parent;

            while (typeInfo != NULL)
            {
                CollectTypeFields(typeInfo, originalType, bindingFlags, fields, nameFilter);
                typeInfo = typeInfo->parent;
            }
        }

        size_t fieldCount = fields.size();
        Il2CppArray* result = vm::Array::NewCached(il2cpp_defaults.field_info_class, (il2cpp_array_size_t)fieldCount);

        for (size_t i = 0; i < fieldCount; i++)
        {
            il2cpp_array_setref(result, i, vm::Reflection::GetFieldObject(originalType, fields[i]));
        }

        return result;
    }

    static Il2CppArray* GetFieldsByName(Il2CppReflectionType* _this, Il2CppString* name, int bindingFlags, Il2CppReflectionType* reflectedType)
    {
        if (name == NULL)
            return GetFieldsImpl(_this, bindingFlags, reflectedType, utils::functional::TrueFilter());

        if (bindingFlags & BFLAGS_IgnoreCase)
        {
            return GetFieldsImpl(_this, bindingFlags, reflectedType, utils::functional::Filter<std::string, utils::VmStringUtils::CaseInsensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
        }

        return GetFieldsImpl(_this, bindingFlags, reflectedType, utils::functional::Filter<std::string, utils::VmStringUtils::CaseSensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
    }

    intptr_t RuntimeType::GetFields_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType)
    {
        Il2CppReflectionField *field;

        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        res_array.reserve(16);

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* fields = GetFieldsByName(&thisPtr->type, nameStr, bindingAttr, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(fields); i++)
        {
            field = il2cpp_array_get(fields, Il2CppReflectionField*, i);
            res_array.push_back(field->field);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    template<typename NameFilter>
    void CollectTypeMethods(Il2CppClass* type, const Il2CppClass* originalType, uint32_t bindingFlags, const NameFilter& nameFilter, std::vector<const MethodInfo*>& methods, bool(&filledSlots)[65535])
    {
        void* iter = NULL;
        while (const MethodInfo* method = vm::Class::GetMethods(type, &iter))
        {
            if ((method->flags & METHOD_ATTRIBUTE_RT_SPECIAL_NAME) != 0 && (strcmp(method->name, ".ctor") == 0 || strcmp(method->name, ".cctor") == 0))
                continue;

            if (CheckMemberMatch(method, type, originalType, bindingFlags, nameFilter))
            {
                if ((method->flags & METHOD_ATTRIBUTE_VIRTUAL) != 0)
                {
                    if (filledSlots[method->slot])
                        continue;

                    filledSlots[method->slot] = true;
                }

                methods.push_back(method);
            }
        }
    }

    template<typename NameFilter>
    static Il2CppArray* GetMethodsByNameImpl(const Il2CppType* type, uint32_t bindingFlags, const NameFilter& nameFilter)
    {
        std::vector<const MethodInfo*> methods;
        bool filledSlots[65535] = { 0 };

        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type);
        Il2CppClass* const originalTypeInfo = typeInfo;

        CollectTypeMethods(typeInfo, typeInfo, bindingFlags, nameFilter, methods, filledSlots);

        if ((bindingFlags & BFLAGS_DeclaredOnly) == 0)
        {
            for (typeInfo = vm::Class::GetParent(typeInfo); typeInfo != NULL; typeInfo = vm::Class::GetParent(typeInfo))
            {
                CollectTypeMethods(typeInfo, originalTypeInfo, bindingFlags, nameFilter, methods, filledSlots);
            }
        }

        size_t methodCount = methods.size();
        Il2CppArray* result = vm::Array::NewCached(il2cpp_defaults.method_info_class, (il2cpp_array_size_t)methodCount);

        for (size_t i = 0; i < methodCount; i++)
        {
            Il2CppReflectionMethod* method = vm::Reflection::GetMethodObject(methods[i], originalTypeInfo);
            il2cpp_array_setref(result, i, method);
        }

        return result;
    }

    static Il2CppArray* GetMethodsByName(Il2CppReflectionType* _this, Il2CppString* name, int32_t bindingFlags, int listType, Il2CppReflectionType* type)
    {
        if (type->type->byref || !ValidBindingFlagsForGetMember(bindingFlags))
            return vm::Array::NewCached(il2cpp_defaults.property_info_class, 0);

        if (name != NULL)
        {
            if (bindingFlags & BFLAGS_IgnoreCase || listType == MLISTTYPE_CaseInsensitive)
            {
                return GetMethodsByNameImpl(type->type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseInsensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
            }

            return GetMethodsByNameImpl(type->type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseSensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
        }

        return GetMethodsByNameImpl(type->type, bindingFlags, utils::functional::TrueFilter());
    }

    intptr_t RuntimeType::GetMethodsByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t namePtr, int32_t bindingAttr, int32_t listType)
    {
        VoidPtrArray res_array;
        Il2CppReflectionMethod *method;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        const char *utf8_name = reinterpret_cast<const char*>(namePtr);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* methods = GetMethodsByName(&thisPtr->type, nameStr, bindingAttr, listType, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(methods); i++)
        {
            method = il2cpp_array_get(methods, Il2CppReflectionMethod*, i);
            res_array.push_back((MethodInfo*)method->method);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    static inline bool CheckNestedTypeMatch(Il2CppClass* nestedType, BindingFlags bindingFlags)
    {
        uint32_t accessFlag = (nestedType->flags & TYPE_ATTRIBUTE_VISIBILITY_MASK) == TYPE_ATTRIBUTE_NESTED_PUBLIC ? BFLAGS_Public : BFLAGS_NonPublic;
        return (accessFlag & bindingFlags) != 0;
    }

    template<typename NameFilter>
    static Il2CppArray* GetNestedTypesImpl(Il2CppReflectionType* type, int32_t bindingFlags, const NameFilter& nameFilter)
    {
        bool validBindingFlags = (bindingFlags & BFLAGS_NonPublic) != 0 || (bindingFlags & BFLAGS_Public) != 0;

        if (type->type->byref || !validBindingFlags)
            return vm::Array::New(il2cpp_defaults.monotype_class, 0);

        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type->type);

        // nested types are always generic type definitions, even for inflated types. As such we only store/retrieve them on
        // type definitions and generic type definitions. If we are a generic instance, use our generic type definition instead.
        if (typeInfo->generic_class)
            typeInfo = vm::GenericClass::GetTypeDefinition(typeInfo->generic_class);

        std::vector<Il2CppClass*> nestedTypes;

        void* iter = NULL;
        while (Il2CppClass* nestedType = vm::Class::GetNestedTypes(typeInfo, &iter))
        {
            if (CheckNestedTypeMatch(nestedType, bindingFlags) && nameFilter(nestedType->name))
                nestedTypes.push_back(nestedType);
        }

        size_t nestedTypeCount = nestedTypes.size();
        Il2CppArray* result = vm::Array::New(il2cpp_defaults.monotype_class, (il2cpp_array_size_t)nestedTypeCount);

        for (size_t i = 0; i < nestedTypeCount; i++)
        {
            il2cpp_array_setref(result, i, vm::Reflection::GetTypeObject(&nestedTypes[i]->byval_arg));
        }

        return result;
    }

    static Il2CppArray* GetNestedTypesByName(Il2CppReflectionType* type, Il2CppString* name, int32_t bindingFlags)
    {
        if (name == NULL)
            return GetNestedTypesImpl(type, bindingFlags, utils::functional::TrueFilter());

        if (bindingFlags & BFLAGS_IgnoreCase)
            return GetNestedTypesImpl(type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseInsensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));

        return GetNestedTypesImpl(type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseSensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
    }

    intptr_t RuntimeType::GetNestedTypes_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType)
    {
        Il2CppReflectionType *nested;
        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* nestedTypes = GetNestedTypesByName(&thisPtr->type, nameStr, bindingAttr);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(nestedTypes); i++)
        {
            nested = il2cpp_array_get(nestedTypes, Il2CppReflectionType*, i);
            res_array.push_back((Il2CppType*)nested->type);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    template<typename NameFilter>
    static void CollectTypeProperties(Il2CppClass* type, uint32_t bindingFlags, const NameFilter& nameFilter, Il2CppClass* const originalType, PropertyPairVector& properties)
    {
        void* iter = NULL;
        while (const PropertyInfo* property = vm::Class::GetProperties(type, &iter))
        {
            if (CheckMemberMatch(property, type, originalType, bindingFlags, nameFilter))
            {
                if (PropertyPairVectorContains(properties, property))
                    continue;

                properties.push_back(PropertyPair(property, originalType));
            }
        }
    }

    template<typename NameFilter>
    static Il2CppArray* GetPropertiesByNameImpl(const Il2CppType* type, uint32_t bindingFlags, const NameFilter& nameFilter)
    {
        PropertyPairVector properties;
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type);
        Il2CppClass* const originalTypeInfo = typeInfo;

        properties.reserve(typeInfo->property_count);
        CollectTypeProperties(typeInfo, bindingFlags, nameFilter, originalTypeInfo, properties);

        if ((bindingFlags & BFLAGS_DeclaredOnly) == 0)
        {
            for (typeInfo = typeInfo->parent; typeInfo != NULL; typeInfo = typeInfo->parent)
            {
                CollectTypeProperties(typeInfo, bindingFlags, nameFilter, originalTypeInfo, properties);
            }
        }

        int i = 0;
        Il2CppArray* res = vm::Array::NewCached(il2cpp_defaults.property_info_class, (il2cpp_array_size_t)properties.size());

        for (PropertyPairVector::const_iterator iter = properties.begin(); iter != properties.end(); iter++)
        {
            il2cpp_array_setref(res, i, vm::Reflection::GetPropertyObject(iter->originalType, iter->property));
            i++;
        }

        return res;
    }

    static Il2CppArray* GetPropertiesByName(Il2CppReflectionType* _this, Il2CppString* name, uint32_t bindingFlags, bool ignoreCase, Il2CppReflectionType* type)
    {
        if (type->type->byref || !ValidBindingFlagsForGetMember(bindingFlags))
            return vm::Array::NewCached(il2cpp_defaults.property_info_class, 0);

        if (name != NULL)
        {
            if (ignoreCase)
            {
                return GetPropertiesByNameImpl(type->type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseInsensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
            }

            return GetPropertiesByNameImpl(type->type, bindingFlags, utils::functional::Filter<std::string, utils::VmStringUtils::CaseSensitiveComparer>(utils::StringUtils::Utf16ToUtf8(name->chars)));
        }

        return GetPropertiesByNameImpl(type->type, bindingFlags, utils::functional::TrueFilter());
    }

    intptr_t RuntimeType::GetPropertiesByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, int32_t listType)
    {
        Il2CppReflectionProperty *prop;
        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        res_array.reserve(8);

        bool ignoreCase = listType == MLISTTYPE_CaseInsensitive;

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* properties = GetPropertiesByName(&thisPtr->type, nameStr, bindingAttr, ignoreCase, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(properties); i++)
        {
            prop = il2cpp_array_get(properties, Il2CppReflectionProperty*, i);
            res_array.push_back((PropertyInfo*)prop->property);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    Il2CppObject* RuntimeType::CreateInstanceInternal(Il2CppReflectionType* type)
    {
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type->type);

        if (typeInfo == NULL || il2cpp::vm::Class::IsNullable(typeInfo))
            return NULL;

        il2cpp::vm::Class::Init(typeInfo);

        // You could think "hey, shouldn't we call the constructor here?" but we don't because this path is only hit for value
        // types, and they cannot have default constructors.  for reference types with constructors, the c# side of CreateInstance()
        // actually takes care of its own business by using reflection to create the object and invoke the constructor.
        return il2cpp_object_new(typeInfo);
    }

    Il2CppObject* RuntimeType::GetCorrespondingInflatedConstructor(Il2CppReflectionRuntimeType* thisPtr, Il2CppObject* generic)
    {
        NOT_SUPPORTED_IL2CPP(MonoType::GetCorrespondingInflatedConstructor, "This icall is only used by System.Reflection.Emit.TypeBuilder.");
        return 0;
    }

    Il2CppObject* RuntimeType::get_DeclaringMethod(Il2CppReflectionRuntimeType* thisPtr)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimeType::get_DeclaringMethod);
        IL2CPP_UNREACHABLE;
        return NULL;
    }

    Il2CppObject* RuntimeType::GetCorrespondingInflatedMethod(Il2CppReflectionRuntimeType* thisPtr, Il2CppObject* generic)
    {
        NOT_SUPPORTED_IL2CPP(MonoType::GetCorrespondingInflatedMethod, "This icall is only used by System.Reflection.Emit.TypeBuilder.");
        return 0;
    }

    Il2CppString* RuntimeType::get_Name(Il2CppReflectionRuntimeType* _type)
    {
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(_type->type.type);

        if (_type->type.type->byref)
        {
            std::string n = il2cpp::utils::StringUtils::Printf("%s&", typeInfo->name);
            return vm::String::New(n.c_str());
        }
        else
        {
            return il2cpp::vm::String::NewWrapper(typeInfo->name);
        }
    }

    Il2CppString* RuntimeType::get_Namespace(Il2CppReflectionRuntimeType* _type)
    {
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(_type->type.type);

        while (Il2CppClass* declaringType = vm::Class::GetDeclaringType(typeInfo))
            typeInfo = declaringType;

        if (typeInfo->namespaze[0] == '\0')
            return NULL;
        else
            return il2cpp::vm::String::NewWrapper(typeInfo->namespaze);
    }

    Il2CppString* RuntimeType::getFullName(Il2CppReflectionRuntimeType* _type, bool full_name, bool assembly_qualified)
    {
        Il2CppTypeNameFormat format;

        if (full_name)
            format = assembly_qualified ?
                IL2CPP_TYPE_NAME_FORMAT_ASSEMBLY_QUALIFIED :
                IL2CPP_TYPE_NAME_FORMAT_FULL_NAME;
        else
            format = IL2CPP_TYPE_NAME_FORMAT_REFLECTION;

        std::string name(vm::Type::GetName(_type->type.type, format));
        if (name.empty())
            return NULL;

        if (full_name && (_type->type.type->type == IL2CPP_TYPE_VAR || _type->type.type->type == IL2CPP_TYPE_MVAR))
        {
            return NULL;
        }

        return il2cpp::vm::String::NewWrapper(name.c_str());
    }

    Il2CppReflectionType* RuntimeType::get_DeclaringType(Il2CppReflectionRuntimeType* _this)
    {
        return vm::Type::GetDeclaringType(_this->type.type);
    }

    void validate_make_array_type_inputs(Il2CppReflectionType* type, int32_t rank)
    {
        // Per MSDN: http://msdn.microsoft.com/en-us/library/w0ykk2sw(v=vs.110).aspx
        if (rank > 32)
        {
            std::string message;
            message = vm::Type::GetName(type->type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME) + " with rank " + utils::StringUtils::Printf("%d", rank) + " has too many dimensions.";
            il2cpp_raise_exception(vm::Exception::GetTypeLoadException(message.c_str()));
        }

        if (type->type->byref)
        {
            std::string message;
            message = "Could not create array type '" + vm::Type::GetName(type->type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME) + "'.";
            il2cpp_raise_exception(vm::Exception::GetTypeLoadException(message.c_str()));
        }

        const Il2CppClass* klass = vm::Class::FromIl2CppType(type->type);

        if ((strcmp(klass->namespaze, "System") == 0 && strcmp(klass->name, "TypedReference") == 0))
        {
            std::string message;
            message + "Could not create array type '" + klass->namespaze + "." + klass->name + "[]'.";
            il2cpp_raise_exception(vm::Exception::GetTypeLoadException(message.c_str()));
        }
    }

    Il2CppReflectionType* RuntimeType::make_array_type(Il2CppReflectionRuntimeType* _type, int32_t rank)
    {
        validate_make_array_type_inputs(&_type->type, rank);

        Il2CppClass* arrayClass;

        Il2CppClass* klass = il2cpp_class_from_il2cpp_type(_type->type.type);
        if (rank == 0) //single dimentional array
            arrayClass = il2cpp_array_class_get(klass, 1);
        else
            arrayClass = il2cpp_bounded_array_class_get(klass, rank, true);

        return arrayClass != NULL ? vm::Reflection::GetTypeObject(&arrayClass->byval_arg) : NULL;
    }

    Il2CppReflectionType* RuntimeType::make_byref_type(Il2CppReflectionRuntimeType* _type)
    {
        Il2CppClass* klass;

        klass = vm::Class::FromIl2CppType(_type->type.type);

        return il2cpp::vm::Reflection::GetTypeObject(&klass->this_arg);
    }

    static std::string FormatExceptionMessageForNonConstructableGenericType(const Il2CppType* type, const Il2CppType** genericArguments, int genericArgumentCount)
    {
        std::string message;
        message += "Failed to construct generic type '";
        message += il2cpp::vm::Type::GetName(type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        message += "' with generic arguments [";
        for (int i = 0; i < genericArgumentCount; i++)
        {
            if (i != 0)
                message += ", ";
            message += il2cpp::vm::Type::GetName(genericArguments[i], IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        }
        message += "] at runtime.";

        return message;
    }

    Il2CppReflectionType * RuntimeType::MakeGenericType(Il2CppReflectionType* type, Il2CppArray* genericArgumentTypes)
    {
        const Il2CppType* genericTypeDefinitionType = type->type;
        Il2CppClass* genericTypeDefinitionClass = vm::Class::FromIl2CppType(genericTypeDefinitionType);
        IL2CPP_ASSERT(vm::Class::IsGeneric(genericTypeDefinitionClass));

        uint32_t arrayLength = vm::Array::GetLength(genericArgumentTypes);
        const Il2CppType** genericArguments = (const Il2CppType**)alloca(arrayLength * sizeof(Il2CppType*));

        for (uint32_t i = 0; i < arrayLength; i++)
        {
            Il2CppReflectionType* genericArgumentType = il2cpp_array_get(genericArgumentTypes, Il2CppReflectionType*, i);
            genericArguments[i] = genericArgumentType->type;
        }

        const Il2CppGenericInst* inst = vm::MetadataCache::GetGenericInst(genericArguments, arrayLength);
        Il2CppGenericClass* genericClass = metadata::GenericMetadata::GetGenericClass(genericTypeDefinitionClass, inst);
        Il2CppClass* genericInstanceTypeClass = vm::GenericClass::GetClass(genericClass);

        if (!genericInstanceTypeClass)
        {
            vm::Exception::Raise(vm::Exception::GetNotSupportedException(FormatExceptionMessageForNonConstructableGenericType(genericTypeDefinitionType, genericArguments, arrayLength).c_str()));
            return NULL;
        }

        return vm::Reflection::GetTypeObject(&genericInstanceTypeClass->byval_arg);
    }

    Il2CppReflectionType* RuntimeType::MakePointerType(Il2CppReflectionType* type)
    {
        Il2CppClass* pointerType = vm::Class::GetPtrClass(type->type);

        return vm::Reflection::GetTypeObject(&pointerType->byval_arg);
    }

    Il2CppArray* RuntimeType::GetGenericArgumentsInternal(Il2CppReflectionRuntimeType* _this, bool runtimeArray)
    {
        return vm::Type::GetGenericArgumentsInternal(&_this->type, runtimeArray);
    }

    Il2CppArray* RuntimeType::GetInterfaces(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass = vm::Class::FromIl2CppType(type->type.type);
        typedef std::set<Il2CppClass*> InterfaceVector;
        InterfaceVector itfs;

        Il2CppClass* currentType = klass;
        while (currentType != NULL)
        {
            void* iter = NULL;
            while (Il2CppClass* itf = vm::Class::GetInterfaces(currentType, &iter))
                itfs.insert(itf);

            currentType = vm::Class::GetParent(currentType);
        }

        Il2CppArray* res = vm::Array::New(il2cpp_defaults.systemtype_class, (il2cpp_array_size_t)itfs.size());
        int i = 0;
        for (InterfaceVector::const_iterator iter = itfs.begin(); iter != itfs.end(); ++iter, ++i)
            il2cpp_array_setref(res, i, vm::Reflection::GetTypeObject(&(*iter)->byval_arg));

        return res;
    }

    /* System.TypeCode */
    typedef enum
    {
        TYPECODE_EMPTY,
        TYPECODE_OBJECT,
        TYPECODE_DBNULL,
        TYPECODE_BOOLEAN,
        TYPECODE_CHAR,
        TYPECODE_SBYTE,
        TYPECODE_BYTE,
        TYPECODE_INT16,
        TYPECODE_UINT16,
        TYPECODE_INT32,
        TYPECODE_UINT32,
        TYPECODE_INT64,
        TYPECODE_UINT64,
        TYPECODE_SINGLE,
        TYPECODE_DOUBLE,
        TYPECODE_DECIMAL,
        TYPECODE_DATETIME,
        TYPECODE_STRING = 18
    } TypeCode;

    int32_t RuntimeType::GetTypeCodeImplInternal(Il2CppReflectionType* type)
    {
        int t = type->type->type;

        if (type->type->byref)
            return TYPECODE_OBJECT;

    handle_enum:
        switch (t)
        {
            case IL2CPP_TYPE_VOID:
                return TYPECODE_OBJECT;
            case IL2CPP_TYPE_BOOLEAN:
                return TYPECODE_BOOLEAN;
            case IL2CPP_TYPE_U1:
                return TYPECODE_BYTE;
            case IL2CPP_TYPE_I1:
                return TYPECODE_SBYTE;
            case IL2CPP_TYPE_U2:
                return TYPECODE_UINT16;
            case IL2CPP_TYPE_I2:
                return TYPECODE_INT16;
            case IL2CPP_TYPE_CHAR:
                return TYPECODE_CHAR;
            case IL2CPP_TYPE_PTR:
            case IL2CPP_TYPE_U:
            case IL2CPP_TYPE_I:
                return TYPECODE_OBJECT;
            case IL2CPP_TYPE_U4:
                return TYPECODE_UINT32;
            case IL2CPP_TYPE_I4:
                return TYPECODE_INT32;
            case IL2CPP_TYPE_U8:
                return TYPECODE_UINT64;
            case IL2CPP_TYPE_I8:
                return TYPECODE_INT64;
            case IL2CPP_TYPE_R4:
                return TYPECODE_SINGLE;
            case IL2CPP_TYPE_R8:
                return TYPECODE_DOUBLE;
            case IL2CPP_TYPE_VALUETYPE:
            {
                if (vm::Type::IsEnum(type->type))
                {
                    t = vm::Class::GetEnumBaseType(vm::Type::GetClass(type->type))->type;
                    goto handle_enum;
                }
                else
                {
                    if (vm::Type::IsSystemDecimal(type->type))
                        return TYPECODE_DECIMAL;
                    else if (vm::Type::IsSystemDateTime(type->type))
                        return TYPECODE_DATETIME;
                }
                return TYPECODE_OBJECT;
            }
            case IL2CPP_TYPE_STRING:
                return TYPECODE_STRING;
            case IL2CPP_TYPE_SZARRAY:
            case IL2CPP_TYPE_ARRAY:
            case IL2CPP_TYPE_OBJECT:
            case IL2CPP_TYPE_VAR:
            case IL2CPP_TYPE_MVAR:
            case IL2CPP_TYPE_TYPEDBYREF:
                return TYPECODE_OBJECT;
            case IL2CPP_TYPE_CLASS:
            {
                if (vm::Type::IsSystemDBNull(type->type))
                    return TYPECODE_DBNULL;
            }
                return TYPECODE_OBJECT;
            case IL2CPP_TYPE_GENERICINST:
                return TYPECODE_OBJECT;
            default:
                abort();
        }
        return false;
    }

    void RuntimeType::GetInterfaceMapData(Il2CppReflectionType* type, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods)
    {
        Il2CppClass* klass = il2cpp_class_from_il2cpp_type(type->type);
        Il2CppClass* iklass = il2cpp_class_from_il2cpp_type(iface->type);

        void* iter = NULL;

        int32_t numberOfMethods = (int32_t)vm::Class::GetNumMethods(iklass);
        int32_t numberOfVirtualMethods = 0;
        for (int i = 0; i < numberOfMethods; ++i)
        {
            const MethodInfo* method = il2cpp_class_get_methods(iklass, &iter);
            if (method->flags & METHOD_ATTRIBUTE_VIRTUAL)
                numberOfVirtualMethods++;
        }


        *targets = il2cpp_array_new(il2cpp_defaults.method_info_class, numberOfVirtualMethods);
        *methods = il2cpp_array_new(il2cpp_defaults.method_info_class, numberOfVirtualMethods);

        if (numberOfVirtualMethods == 0)
            return;

        vm::Class::Init(klass);
        const VirtualInvokeData* invokeDataStart;

        // So this part is tricky. GetInterfaceInvokeDataFromVTable takes an object pointer in order to support
        // COM peculiarities, like being able to return invoke data for an interface only if native side implements it
        // So here we create a fake object of the class we want to query and pass that to GetInterfaceInvokeDataFromVTable
        // It is safe because the only fields GetInterfaceInvokeDataFromVTable accesses are the klass and identity fields
        if (!klass->is_import_or_windows_runtime)
        {
            Il2CppObject fakeObject = {};
            fakeObject.klass = klass;
            invokeDataStart = &vm::ClassInlines::GetInterfaceInvokeDataFromVTable(&fakeObject, iklass, 0);
        }
        else
        {
            Il2CppComObject fakeComObject;
            memset(&fakeComObject, 0, sizeof(fakeComObject));
            fakeComObject.klass = klass;

            // This makes GetInterfaceInvokeDataFromVTable believe that the COM object is dead,
            // thus making it skip asking native side whether a particular interface is supported
            fakeComObject.identity = NULL;

            invokeDataStart = &vm::ClassInlines::GetInterfaceInvokeDataFromVTable(&fakeComObject, iklass, 0);
        }

        iter = NULL;
        int virtualMethodIndex = 0;
        for (int i = 0; i < numberOfMethods; ++i)
        {
            const MethodInfo* method = il2cpp_class_get_methods(iklass, &iter);
            if (method->flags & METHOD_ATTRIBUTE_VIRTUAL)
            {
                Il2CppReflectionMethod* member = il2cpp_method_get_object(method, iklass);
                il2cpp_array_setref(*methods, virtualMethodIndex, member);

                const MethodInfo* targetMethod = invokeDataStart[i].method;

                if (vm::Method::IsAmbiguousMethodInfo(targetMethod) || vm::Method::IsEntryPointNotFoundMethodInfo(targetMethod))
                {
                    // Method is an ambiguous default interface method (more than one DIM matched)
                    // Or there is no valid method in this slot
                    member = NULL;
                }
                else if (vm::Class::IsInterface(targetMethod->klass))
                {
                    // We found a default interface method

                    if (targetMethod->methodPointer == NULL)
                    {
                        // The method was re-abstracted
                        member = NULL;
                    }
                    else
                    {
                        // Normal DIM case
                        member = il2cpp_method_get_object(targetMethod, targetMethod->klass);
                    }
                }
                else
                {
                    // Normal interface implementation
                    member = il2cpp_method_get_object(targetMethod, klass);
                }
                il2cpp_array_setref(*targets, virtualMethodIndex, member);
                virtualMethodIndex++;
            }
        }
    }

    void RuntimeType::GetPacking(Il2CppReflectionType* type, int32_t* packing, int32_t* size)
    {
        const Il2CppType* runtimeType = vm::Type::IsGenericInstance(type->type) ? vm::Type::GetGenericTypeDefintion(type->type) : type->type;
        Il2CppMetadataTypeHandle handle = il2cpp::vm::MetadataCache::GetTypeHandleFromType(runtimeType);

        if (vm::MetadataCache::StructLayoutPackIsDefault(handle))
            *packing = 8;
        else
            *packing = vm::MetadataCache::StructLayoutPack(handle);

        if (vm::MetadataCache::StructLayoutSizeIsDefault(handle))
            *size = 0;
        else
            *size = vm::Class::FromIl2CppType(runtimeType)->native_size;
    }

    void RuntimeType::GetGUID(Il2CppReflectionType* type, Il2CppArray* guid_result)
    {
        IL2CPP_ASSERT(vm::Array::GetLength(guid_result) == sizeof(Il2CppGuid));
        if (type == NULL)
            return;

        Il2CppClass* klass = vm::Class::FromIl2CppType(type->type);

        if (klass->interopData != nullptr)
        {
            uint8_t* guid = il2cpp_array_addr_with_size(guid_result, 1, 0);
            memcpy(guid, klass->interopData->guid, sizeof(Il2CppGuid));
        }
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
