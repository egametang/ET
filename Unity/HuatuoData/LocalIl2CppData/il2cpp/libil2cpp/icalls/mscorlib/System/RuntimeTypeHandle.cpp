#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "mono-structs.h"

#include "RuntimeTypeHandle.h"
#include "Type.h"

#include "utils/StringUtils.h"
#include "vm/Class.h"
#include "vm/Image.h"
#include "vm/Object.h"
#include "vm/Reflection.h"
#include "vm/MetadataCache.h"
#include "vm/GenericClass.h"
#include "il2cpp-api.h"

#define CHECK_IF_NULL(v)    \
    if ( (v) == NULL && throwOnError ) \
        vm::Exception::Raise (vm::Exception::GetTypeLoadException (info)); \
    if ( (v) == NULL ) \
        return NULL;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool RuntimeTypeHandle::HasInstantiation(Il2CppReflectionRuntimeType* typeObject)
    {
        const Il2CppType* type = typeObject->type.type;

        if (type->byref)
            return false;

        Il2CppClass* klass = vm::Class::FromIl2CppType(type);

        return vm::Class::IsGeneric(klass) || vm::Class::IsInflated(klass);
    }

    bool RuntimeTypeHandle::HasReferences(Il2CppReflectionRuntimeType* type)
    {
        return vm::Class::FromIl2CppType(type->type.type)->has_references;
    }

    static bool is_generic_parameter(Il2CppType *type)
    {
        return !type->byref && (type->type == IL2CPP_TYPE_VAR || type->type == IL2CPP_TYPE_MVAR);
    }

    bool RuntimeTypeHandle::is_subclass_of(Il2CppType* childType, Il2CppType* baseType)
    {
        bool result = false;
        Il2CppClass *childClass;
        Il2CppClass *baseClass;

        childClass = vm::Class::FromIl2CppType(childType);
        baseClass = vm::Class::FromIl2CppType(baseType);

        if (childType->byref)
            return !baseType->byref && baseClass == il2cpp_defaults.object_class;

        if (baseType->byref)
            return false;

        /* .NET IsSubclassOf is not reflexive */
        if (childType == baseType)
            return false;

        if (is_generic_parameter(childType))
        {
            /* slow path: walk the type hierarchy looking at base types
             * until we see baseType.  If the current type is not a gparam,
             * break out of the loop and use is_subclass_of.
             */
            Il2CppClass *c = vm::Class::GenericParamGetBaseType(childClass);

            result = false;
            while (c != NULL)
            {
                if (c == baseClass)
                {
                    result = true;
                    break;
                }
                if (!is_generic_parameter(&c->byval_arg))
                {
                    result = vm::Class::IsSubclassOf(c, baseClass, false);
                    break;
                }
                else
                    c = vm::Class::GenericParamGetBaseType(c);
            }
        }
        else
        {
            result = vm::Class::IsSubclassOf(childClass, baseClass, false);
        }

        return result;
    }

    bool RuntimeTypeHandle::IsByRefLike(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass = vm::Class::FromIl2CppType(type->type.type);
        return klass->is_byref_like;
    }

    bool RuntimeTypeHandle::IsComObject(Il2CppReflectionRuntimeType* type)
    {
        return false; // il2cpp does not support COM objects, so this is always false
    }

    bool RuntimeTypeHandle::IsGenericTypeDefinition(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass;

        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(MonoType::get_IsGenericTypeDefinition, "Check for custom Type implementations");
        //if (!IS_MONOTYPE (type))
        //  return FALSE;

        if (type->type.type->byref)
            return false;

        klass = vm::Class::FromIl2CppType(type->type.type);

        return vm::Class::IsGeneric(klass);
    }

    bool RuntimeTypeHandle::IsGenericVariable(Il2CppReflectionRuntimeType* type)
    {
        return !type->type.type->byref && (type->type.type->type == IL2CPP_TYPE_VAR || type->type.type->type == IL2CPP_TYPE_MVAR);
    }

    bool RuntimeTypeHandle::IsInstanceOfType(Il2CppReflectionRuntimeType* type, Il2CppObject* obj)
    {
        Il2CppClass* klass = vm::Class::FromIl2CppType(type->type.type);
        return il2cpp::vm::Object::IsInst(obj, klass) != NULL;
    }

    bool RuntimeTypeHandle::type_is_assignable_from(Il2CppReflectionType* a, Il2CppReflectionType* b)
    {
        return vm::Class::IsAssignableFrom(a, b);
    }

    int32_t RuntimeTypeHandle::GetArrayRank(Il2CppReflectionRuntimeType* type)
    {
        if (type->type.type->type != IL2CPP_TYPE_ARRAY && type->type.type->type != IL2CPP_TYPE_SZARRAY)
            IL2CPP_ASSERT("Type must be an array type");

        Il2CppClass* klass = vm::Class::FromIl2CppType(type->type.type);
        return klass->rank;
    }

    int32_t RuntimeTypeHandle::GetMetadataToken(Il2CppReflectionRuntimeType* type)
    {
        return vm::Class::FromSystemType(&type->type)->token;
    }

    intptr_t RuntimeTypeHandle::GetGenericParameterInfo(Il2CppReflectionRuntimeType* type)
    {
        intptr_t retVal = 0;

        const Il2CppType *thisType = type->type.type;
        if ((thisType->type == IL2CPP_TYPE_VAR) || (thisType->type == IL2CPP_TYPE_MVAR))
        {
            Il2CppMetadataGenericParameterHandle param = il2cpp::vm::MetadataCache::GetGenericParameterFromType(thisType);
            if (param)
            {
                retVal = reinterpret_cast<intptr_t>(vm::Class::GetOrCreateMonoGenericParameterInfo(param));
            }
        }

        return retVal;
    }

    uint8_t RuntimeTypeHandle::GetCorElementType(Il2CppReflectionRuntimeType* ref_type)
    {
        const Il2CppType *type = ref_type->type.type;

        if (type->byref)
            return IL2CPP_TYPE_BYREF;
        else
            return (uint8_t)type->type;
    }

    Il2CppReflectionAssembly* RuntimeTypeHandle::GetAssembly(Il2CppReflectionRuntimeType* type)
    {
        return vm::Reflection::GetAssemblyObject(vm::Image::GetAssembly(vm::Class::GetImage(vm::Class::FromIl2CppType(type->type.type))));
    }

    Il2CppReflectionModule* RuntimeTypeHandle::GetModule(Il2CppReflectionRuntimeType* type)
    {
        return vm::Reflection::GetModuleObject(vm::Class::GetImage(vm::Class::FromIl2CppType(type->type.type)));
    }

    int32_t RuntimeTypeHandle::GetAttributes(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass = vm::Class::FromSystemType(&type->type);

        return klass->flags;
    }

    Il2CppReflectionRuntimeType* RuntimeTypeHandle::GetBaseType(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass = vm::Class::FromIl2CppType((&type->type)->type);

        Il2CppReflectionType* retVal = klass->parent ? il2cpp::vm::Reflection::GetTypeObject(&klass->parent->byval_arg) : NULL;
        return reinterpret_cast<Il2CppReflectionRuntimeType*>(retVal);
    }

    Il2CppReflectionRuntimeType* RuntimeTypeHandle::GetElementType(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass;
        Il2CppReflectionType* retVal = NULL;

        klass = vm::Class::FromIl2CppType(type->type.type);

        // GetElementType should only return a type for:
        // Array Pointer PassedByRef
        if (type->type.type->byref)
            retVal = il2cpp::vm::Reflection::GetTypeObject(&klass->byval_arg);
        else if (klass->element_class && IL2CPP_CLASS_IS_ARRAY(klass))
            retVal = il2cpp::vm::Reflection::GetTypeObject(&klass->element_class->byval_arg);
        else if (klass->element_class && type->type.type->type == IL2CPP_TYPE_PTR)
            retVal = il2cpp::vm::Reflection::GetTypeObject(&klass->element_class->byval_arg);

        if (retVal)
            return reinterpret_cast<Il2CppReflectionRuntimeType*>(retVal);
        return NULL;
    }

    Il2CppReflectionType* RuntimeTypeHandle::internal_from_name(Il2CppString* name, int32_t* stackMark, Il2CppObject* callerAssembly, bool throwOnError, bool ignoreCase, bool reflectionOnly)
    {
        std::string str = utils::StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(name));

        il2cpp::vm::TypeNameParseInfo info;
        il2cpp::vm::TypeNameParser parser(str, info, false);

        if (!parser.Parse())
        {
            if (throwOnError)
                vm::Exception::Raise(vm::Exception::GetArgumentException("typeName", "Invalid type name"));
            else
                return NULL;
        }

        vm::TypeSearchFlags searchFlags = vm::kTypeSearchFlagNone;

        if (throwOnError)
            searchFlags = static_cast<vm::TypeSearchFlags>(searchFlags | vm::kTypeSearchFlagThrowOnError);

        if (ignoreCase)
            searchFlags = static_cast<vm::TypeSearchFlags>(searchFlags | vm::kTypeSearchFlagIgnoreCase);

        const Il2CppType *type = vm::Class::il2cpp_type_from_type_info(info, searchFlags);

        CHECK_IF_NULL(type);

        return il2cpp::vm::Reflection::GetTypeObject(type);
    }

    Il2CppReflectionType* RuntimeTypeHandle::GetGenericTypeDefinition_impl(Il2CppReflectionRuntimeType* type)
    {
        Il2CppClass* klass;

        if (type->type.type->byref)
            return NULL;

        klass = vm::Class::FromIl2CppType(type->type.type);
        if (vm::Class::IsGeneric(klass))
            return &type->type;

        if (klass->generic_class)
        {
            Il2CppClass* generic_class = vm::GenericClass::GetTypeDefinition(klass->generic_class);
            return vm::Reflection::GetTypeObject(&generic_class->byval_arg);
        }

        return NULL;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
