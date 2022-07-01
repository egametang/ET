#include "il2cpp-config.h"

#include "Activator.h"
#include "il2cpp-runtime-metadata.h"
#include "MonoType.h"
#include "RuntimeType.h"
#include "Type.h"
#include "vm/Array.h"
#include "vm/Type.h"
#include "vm/Class.h"
#include "vm/Field.h"
#include "vm/String.h"
#include "vm/GenericClass.h"
#include "utils/dynamic_array.h"
#include "utils/Il2CppHashSet.h"
#include "utils/StringUtils.h"
#include "il2cpp-api.h"
#include "icalls/mscorlib/System/MonoType.h"
#include "il2cpp-tabledefs.h"
#include "mono-structs.h"


namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool RuntimeType::IsTypeExportedToWindowsRuntime(Il2CppObject* type)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimeType::IsTypeExportedToWindowsRuntime);
        IL2CPP_UNREACHABLE;
        return false;
    }

    bool RuntimeType::IsWindowsRuntimeObjectType(Il2CppObject* type)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimeType::IsWindowsRuntimeObjectType);
        IL2CPP_UNREACHABLE;
        return false;
    }

    int32_t RuntimeType::get_core_clr_security_level(Il2CppObject* _this)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimeType::get_core_clr_security_level);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    int32_t RuntimeType::GetGenericParameterPosition(Il2CppReflectionRuntimeType* _this)
    {
        return Type::GetGenericParameterPosition(&_this->type);
    }

    Il2CppObject* RuntimeType::CreateInstanceInternal(Il2CppReflectionType* type)
    {
        return Activator::CreateInstanceInternal(type);
    }

    int32_t RuntimeType::GetGenericParameterAttributes(Il2CppReflectionRuntimeType* _this)
    {
        return Type::GetGenericParameterAttributes(&_this->type);
    }

    Il2CppObject* RuntimeType::get_DeclaringMethod(Il2CppObject* _this)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(RuntimeType::get_DeclaringMethod);
        IL2CPP_UNREACHABLE;
        return NULL;
    }

    Il2CppArray* RuntimeType::GetConstructors_internal(Il2CppReflectionRuntimeType* _this, int32_t bindingAttr, Il2CppReflectionType* reflected_type)
    {
        return MonoType::GetConstructors_internal(&_this->type, bindingAttr, reflected_type);
    }

    Il2CppArray* RuntimeType::GetEvents_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, Il2CppReflectionType* reflected_type)
    {
        return MonoType::GetEventsByName(&_this->type, name, bindingAttr, reflected_type);
    }

    Il2CppArray* RuntimeType::GetFields_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, Il2CppReflectionType* reflected_type)
    {
        return MonoType::GetFieldsByName(&_this->type, name, bindingAttr, reflected_type);
    }

    Il2CppArray* RuntimeType::GetMethodsByName(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, bool ignoreCase, Il2CppReflectionType* reflected_type)
    {
        return MonoType::GetMethodsByName(&_this->type, name, bindingAttr, ignoreCase, reflected_type);
    }

    Il2CppArray* RuntimeType::GetPropertiesByName(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingAttr, bool icase, Il2CppReflectionType* reflected_type)
    {
        return MonoType::GetPropertiesByName(&_this->type, name, bindingAttr, icase, reflected_type);
    }

    Il2CppArray* RuntimeType::GetNestedTypes_internal(Il2CppReflectionRuntimeType* _this, Il2CppString* name, int32_t bindingFlags)
    {
        return MonoType::GetNestedTypesByName(&_this->type, name, bindingFlags);
    }

    Il2CppString* RuntimeType::get_Name(Il2CppReflectionRuntimeType* _this)
    {
        return MonoType::get_Name(&_this->type);
    }

    Il2CppString* RuntimeType::get_Namespace(Il2CppReflectionRuntimeType* _this)
    {
        return MonoType::get_Namespace(&_this->type);
    }

    Il2CppString* RuntimeType::getFullName(Il2CppReflectionRuntimeType* _this, bool full_name, bool assembly_qualified)
    {
        return MonoType::getFullName(&_this->type, full_name, assembly_qualified);
    }

    Il2CppReflectionType* RuntimeType::get_DeclaringType(Il2CppReflectionRuntimeType* _this)
    {
        return vm::Type::GetDeclaringType(_this->type.type);
    }

    Il2CppReflectionType* RuntimeType::make_array_type(Il2CppReflectionRuntimeType* _this, int32_t rank)
    {
        return Type::make_array_type(&_this->type, rank);
    }

    Il2CppReflectionType* RuntimeType::make_byref_type(Il2CppReflectionRuntimeType* _this)
    {
        return Type::make_byref_type(&_this->type);
    }

    Il2CppReflectionType* RuntimeType::MakeGenericType(Il2CppReflectionType* gt, Il2CppArray* types)
    {
        return Type::MakeGenericType(gt, types);
    }

    Il2CppReflectionType* RuntimeType::MakePointerType(Il2CppReflectionType* type)
    {
        return Type::MakePointerType(type);
    }

    Il2CppArray* RuntimeType::GetGenericArgumentsInternal(Il2CppReflectionRuntimeType* _this, bool runtimeArray)
    {
        return vm::Type::GetGenericArgumentsInternal(&_this->type, runtimeArray);
    }

    Il2CppArray* RuntimeType::GetGenericParameterConstraints_impl(Il2CppReflectionRuntimeType* _this)
    {
        return Type::GetGenericParameterConstraints_impl(&_this->type);
    }

    Il2CppArray* RuntimeType::GetInterfaces(Il2CppReflectionRuntimeType* _this)
    {
        return MonoType::GetInterfaces(&_this->type);
    }

    int32_t RuntimeType::GetTypeCodeImplInternal(Il2CppReflectionType* type)
    {
        return Type::GetTypeCodeInternal(type);
    }

    void RuntimeType::GetInterfaceMapData(Il2CppReflectionType* t, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods)
    {
        Type::GetInterfaceMapData(t, iface, targets, methods);
    }

    void RuntimeType::GetPacking(Il2CppReflectionRuntimeType* _this, int32_t* packing, int32_t* size)
    {
        Type::GetPacking(&_this->type, packing, size);
    }

    intptr_t RuntimeType::GetConstructors_native(Il2CppReflectionRuntimeType* thisPtr, int32_t bindingAttr)
    {
        Il2CppReflectionMethod *method;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        VoidPtrArray res_array;
        res_array.reserve(4);

        Il2CppArray* constructors = MonoType::GetConstructors_internal(&thisPtr->type, bindingAttr, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(constructors); i++)
        {
            method = il2cpp_array_get(constructors, Il2CppReflectionMethod*, i);
            res_array.push_back((MethodInfo*)method->method);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetEvents_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr)
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
        Il2CppArray* events = MonoType::GetEventsByName(&thisPtr->type, nameStr, bindingAttr, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(events); i++)
        {
            event = il2cpp_array_get(events, Il2CppReflectionMonoEvent*, i);
            res_array.push_back((EventInfo*)event->eventInfo);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetFields_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr)
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
        Il2CppArray* fields = MonoType::GetFieldsByName(&thisPtr->type, nameStr, bindingAttr, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(fields); i++)
        {
            field = il2cpp_array_get(fields, Il2CppReflectionField*, i);
            res_array.push_back(field->field);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetMethodsByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t namePtr, int32_t bindingAttr, bool ignoreCase)
    {
        VoidPtrArray res_array;
        Il2CppReflectionMethod *method;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        const char *utf8_name = reinterpret_cast<const char*>(namePtr);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* methods = MonoType::GetMethodsByName(&thisPtr->type, nameStr, bindingAttr, ignoreCase, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(methods); i++)
        {
            method = il2cpp_array_get(methods, Il2CppReflectionMethod*, i);
            res_array.push_back((MethodInfo*)method->method);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetNestedTypes_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr)
    {
        Il2CppReflectionType *nested;
        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* nestedTypes = MonoType::GetNestedTypesByName(&thisPtr->type, nameStr, bindingAttr);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(nestedTypes); i++)
        {
            nested = il2cpp_array_get(nestedTypes, Il2CppReflectionType*, i);
            res_array.push_back((Il2CppType*)nested->type);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    intptr_t RuntimeType::GetPropertiesByName_native(Il2CppReflectionRuntimeType* thisPtr, intptr_t name, int32_t bindingAttr, bool icase)
    {
        Il2CppReflectionProperty *prop;
        VoidPtrArray res_array;

        if (thisPtr->type.type->byref)
        {
            return reinterpret_cast<intptr_t>(empty_gptr_array());
        }

        res_array.reserve(8);

        const char *utf8_name = reinterpret_cast<const char*>(name);
        Il2CppString* nameStr = utf8_name == NULL ? NULL : il2cpp::vm::String::New(utf8_name);
        Il2CppArray* properties = MonoType::GetPropertiesByName(&thisPtr->type, nameStr, bindingAttr, icase, &thisPtr->type);

        for (unsigned int i = 0; i < il2cpp::vm::Array::GetLength(properties); i++)
        {
            prop = il2cpp_array_get(properties, Il2CppReflectionProperty*, i);
            res_array.push_back((PropertyInfo*)prop->property);
        }

        return reinterpret_cast<intptr_t>(void_ptr_array_to_gptr_array(res_array));
    }

    void* /* System.Reflection.ConstructorInfo */ RuntimeType::GetCorrespondingInflatedConstructor(void* /* System.MonoType */ self, void* /* System.Reflection.ConstructorInfo */ genericInfo)
    {
        NOT_SUPPORTED_IL2CPP(MonoType::GetCorrespondingInflatedConstructor, "This icall is only used by System.Reflection.Emit.TypeBuilder.");
        return 0;
    }

    mscorlib_System_Reflection_MethodInfo *  RuntimeType::GetCorrespondingInflatedMethod(Il2CppReflectionMonoType *, Il2CppReflectionMonoType *)
    {
        NOT_SUPPORTED_IL2CPP(MonoType::GetCorrespondingInflatedMethod, "This icall is only used by System.Reflection.Emit.TypeBuilder.");
        return 0;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
