#include "il2cpp-config.h"
#include "icalls/mscorlib/System/Type.h"
#include "icalls/mscorlib/System/MonoType.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-api.h"
#include "il2cpp-tabledefs.h"
#include "metadata/GenericMetadata.h"
#include "metadata/Il2CppTypeVector.h"
#include "vm/Array.h"
#include "vm/Assembly.h"
#include "vm/Class.h"
#include "vm/ClassInlines.h"
#include "vm/GenericClass.h"
#include "vm/MetadataCache.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "vm/Thread.h"
#include "vm/Exception.h"
#include "vm/Reflection.h"
#include "utils/StringUtils.h"

#include <vector>
#include <string>

using il2cpp::metadata::GenericMetadata;
using il2cpp::metadata::Il2CppTypeVector;
using il2cpp::utils::StringUtils;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool Type::EqualsInternal(Il2CppReflectionType * left, Il2CppReflectionType * right)
    {
        return (left->type == right->type);
    }

    bool Type::get_IsGenericType(Il2CppReflectionType* type)
    {
        Il2CppClass *klass;

        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(MonoType::get_IsGenericTypeDefinition, "Check for custom Type implementations");
        //if (!IS_MONOTYPE (type))
        //  return FALSE;

        if (type->type->byref)
            return false;

        klass = vm::Class::FromIl2CppType(type->type);
        return klass->generic_class != NULL || vm::Class::IsGeneric(klass);
    }

    bool Type::get_IsGenericTypeDefinition(Il2CppReflectionType * type)
    {
        Il2CppClass *klass;

        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(MonoType::get_IsGenericTypeDefinition, "Check for custom Type implementations");
        //if (!IS_MONOTYPE (type))
        //  return FALSE;

        if (type->type->byref)
            return false;

        klass = vm::Class::FromIl2CppType(type->type);

        return vm::Class::IsGeneric(klass);
    }

    int32_t Type::GetGenericParameterPosition(Il2CppReflectionType* type)
    {
        if (MonoType::get_IsGenericParameter(type))
            return vm::Type::GetGenericParameterInfo(type->type).num;
        return -1;
    }

    Il2CppReflectionType * Type::GetGenericTypeDefinition_impl(Il2CppReflectionType* type)
    {
        Il2CppClass *klass;

        if (type->type->byref)
            return NULL;

        klass = vm::Class::FromIl2CppType(type->type);
        if (vm::Class::IsGeneric(klass))
            return type;

        if (klass->generic_class)
        {
            Il2CppClass *generic_class = vm::GenericClass::GetTypeDefinition(klass->generic_class);
            return vm::Reflection::GetTypeObject(&generic_class->byval_arg);
        }

        return NULL;
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

    int Type::GetTypeCodeInternal(Il2CppReflectionType* type)
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

    Il2CppReflectionType * Type::internal_from_handle(intptr_t ptr)
    {
        return vm::Type::GetTypeFromHandle(ptr);
    }

#define CHECK_IF_NULL(v)    \
    if ( (v) == NULL && throwOnError ) \
        vm::Exception::Raise (vm::Exception::GetTypeLoadException (info)); \
    if ( (v) == NULL ) \
        return NULL;

    Il2CppReflectionType * Type::internal_from_name(Il2CppString* name, bool throwOnError, bool ignoreCase)
    {
        std::string str = StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(name));

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

    bool Type::IsArrayImpl(Il2CppReflectionType *t)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL_NO_ASSERT(Type::IsArrayImpl, "Faulty implementation?");

        Il2CppClass* typeInfo = vm::Class::FromSystemType(t);
        return typeInfo->rank > 0;
    }

    bool Type::IsInstanceOfType(Il2CppReflectionType *type, Il2CppObject * obj)
    {
        Il2CppClass *klass = vm::Class::FromIl2CppType(type->type);
        return il2cpp::vm::Object::IsInst(obj, klass) != NULL;
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

        const Il2CppClass *klass = vm::Class::FromIl2CppType(type->type);

        if ((strcmp(klass->namespaze, "System") == 0 && strcmp(klass->name, "TypedReference") == 0))
        {
            std::string message;
            message + "Could not create array type '" + klass->namespaze + "." + klass->name + "[]'.";
            il2cpp_raise_exception(vm::Exception::GetTypeLoadException(message.c_str()));
        }
    }

    Il2CppReflectionType* Type::make_array_type(Il2CppReflectionType* type, int32_t rank)
    {
        validate_make_array_type_inputs(type, rank);

        Il2CppClass* arrayClass;

        Il2CppClass* klass = il2cpp_class_from_il2cpp_type(type->type);
        if (rank == 0) //single dimentional array
            arrayClass = il2cpp_array_class_get(klass, 1);
        else
            arrayClass = il2cpp_bounded_array_class_get(klass, rank, true);

        return arrayClass != NULL ? vm::Reflection::GetTypeObject(&arrayClass->byval_arg) : NULL;
    }

    static std::string FormatExceptionMessageForNonConstructableGenericType(const Il2CppType* type, const Il2CppTypeVector& genericArguments)
    {
        std::string message;
        message += "Failed to construct generic type '";
        message += il2cpp::vm::Type::GetName(type, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        message += "' with generic arguments [";
        for (Il2CppTypeVector::const_iterator iter = genericArguments.begin(); iter != genericArguments.end(); ++iter)
        {
            if (iter != genericArguments.begin())
                message += ", ";
            message += il2cpp::vm::Type::GetName(*iter, IL2CPP_TYPE_NAME_FORMAT_FULL_NAME);
        }
        message += "] at runtime.";

        return message;
    }

    Il2CppReflectionType * Type::MakeGenericType(Il2CppReflectionType* type, Il2CppArray* genericArgumentTypes)
    {
        const Il2CppType* genericTypeDefinitionType = type->type;
        Il2CppClass* genericTypeDefinitionClass = vm::Class::FromIl2CppType(genericTypeDefinitionType);
        IL2CPP_ASSERT(vm::Class::IsGeneric(genericTypeDefinitionClass));

        uint32_t arrayLength = vm::Array::GetLength(genericArgumentTypes);
        Il2CppTypeVector genericArguments;
        genericArguments.reserve(arrayLength);

        for (uint32_t i = 0; i < arrayLength; i++)
        {
            Il2CppReflectionType* genericArgumentType = il2cpp_array_get(genericArgumentTypes, Il2CppReflectionType*, i);
            genericArguments.push_back(genericArgumentType->type);
        }

        const Il2CppGenericInst* inst = vm::MetadataCache::GetGenericInst(genericArguments);
        Il2CppGenericClass* genericClass = GenericMetadata::GetGenericClass(genericTypeDefinitionClass, inst);
        Il2CppClass* genericInstanceTypeClass = vm::GenericClass::GetClass(genericClass);

        if (!genericInstanceTypeClass)
        {
            vm::Exception::Raise(vm::Exception::GetNotSupportedException(FormatExceptionMessageForNonConstructableGenericType(genericTypeDefinitionType, genericArguments).c_str()));
            return NULL;
        }

        return vm::Reflection::GetTypeObject(&genericInstanceTypeClass->byval_arg);
    }

    bool Type::type_is_assignable_from(Il2CppReflectionType * type, Il2CppReflectionType * c)
    {
        return vm::Class::IsAssignableFrom(type, c);
    }

    bool Type::type_is_subtype_of(Il2CppReflectionType *type, Il2CppReflectionType *c, bool check_interfaces)
    {
        Il2CppClass *klass;
        Il2CppClass *klassc;

        IL2CPP_ASSERT(type != NULL);

        if (!c) /* FIXME: dont know what do do here */
            return false;

        klass = vm::Class::FromSystemType(type);
        klassc = vm::Class::FromSystemType(c);

        /*if (type->type->byref)
            return klassc == mono_defaults.object_class;*/

        return vm::Class::IsSubclassOf(klass, klassc, check_interfaces);
    }

    Il2CppReflectionType* Type::make_byref_type(Il2CppReflectionType *type)
    {
        Il2CppClass *klass;

        klass = vm::Class::FromIl2CppType(type->type);

        return il2cpp::vm::Reflection::GetTypeObject(&klass->this_arg);
    }

    Il2CppReflectionType * Type::MakePointerType(Il2CppReflectionType* type)
    {
        Il2CppClass* pointerType = vm::Class::GetPtrClass(type->type);

        return vm::Reflection::GetTypeObject(&pointerType->byval_arg);
    }

    void Type::GetInterfaceMapData(Il2CppReflectionType* type, Il2CppReflectionType* iface, Il2CppArray** targets, Il2CppArray** methods)
    {
        Il2CppClass* klass = il2cpp_class_from_il2cpp_type(type->type);
        Il2CppClass* iklass = il2cpp_class_from_il2cpp_type(iface->type);

        int32_t numberOfMethods = (int32_t)vm::Class::GetNumMethods(iklass);
        *targets = il2cpp_array_new(il2cpp_defaults.method_info_class, numberOfMethods);
        *methods = il2cpp_array_new(il2cpp_defaults.method_info_class, numberOfMethods);

        if (numberOfMethods == 0)
            return;

        void* unused = NULL;
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

        for (int i = 0; i < numberOfMethods; ++i)
        {
            const MethodInfo *method = il2cpp_class_get_methods(iklass, &unused);
            Il2CppReflectionMethod* member = il2cpp_method_get_object(method, iklass);
            il2cpp_array_setref(*methods, i, member);
            member = il2cpp_method_get_object(invokeDataStart[i].method, klass);
            il2cpp_array_setref(*targets, i, member);
        }
    }

    Il2CppGenericParameterAttributes Type::GetGenericParameterAttributes(Il2CppReflectionType* type)
    {
        Il2CppGenericParameterInfo genericParameter = vm::Type::GetGenericParameterInfo(type->type);
        if (genericParameter.containerHandle == NULL)
            return 0;

        return genericParameter.flags;
    }

    Il2CppArray* Type::GetGenericParameterConstraints_impl(Il2CppReflectionType* type)
    {
        Il2CppMetadataGenericParameterHandle handle = vm::Type::GetGenericParameterHandle(type->type);
        Il2CppGenericParameterInfo genericParameter = vm::Type::GetGenericParameterInfo(type->type);
        if (genericParameter.containerHandle == NULL)
            return NULL;

        int16_t constraintsCount = vm::MetadataCache::GetGenericConstraintCount(handle);
        Il2CppArray* res = il2cpp_array_new(il2cpp_defaults.monotype_class, constraintsCount);
        for (int i = 0; i < constraintsCount; i++)
        {
            const Il2CppType* constarintType = vm::MetadataCache::GetGenericParameterConstraintFromIndex(handle, i);
            il2cpp_array_setref(res, i, il2cpp_type_get_object(constarintType));
        }

        return res;
    }

    void Type::GetPacking(Il2CppReflectionType* type, int32_t* packing, int32_t* size)
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
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
