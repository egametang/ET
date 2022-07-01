#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "icalls/mscorlib/System/Enum.h"
#include "vm/Class.h"
#include "vm/Object.h"
#include "vm/Exception.h"
#include "vm/Reflection.h"
#include "vm/Enum.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    Il2CppObject * Enum::get_value(Il2CppObject *thisPtr)
    {
        if (!thisPtr)
            return NULL;

        IL2CPP_ASSERT(thisPtr->klass->enumtype);

        Il2CppClass* enumClass = vm::Class::FromIl2CppType(vm::Class::GetEnumBaseType(thisPtr->klass));
        Il2CppObject* res = vm::Object::New(enumClass);
        void* dst = (char*)res + sizeof(Il2CppObject);
        void* src = (char*)thisPtr + sizeof(Il2CppObject);
        int32_t size = vm::Class::GetValueSize(enumClass, NULL);

        memcpy(dst, src, size);

        return res;
    }

    int Enum::compare_value_to(Il2CppObject * thisPtr, Il2CppObject * other)
    {
        void* tdata = (char*)thisPtr + sizeof(Il2CppObject);
        void* odata = (char*)other + sizeof(Il2CppObject);
        const Il2CppType *basetype = vm::Class::GetEnumBaseType(vm::Object::GetClass(thisPtr));
        IL2CPP_ASSERT(basetype);

#define COMPARE_ENUM_VALUES(ENUM_TYPE) do { \
        ENUM_TYPE me = *((ENUM_TYPE*)tdata); \
        ENUM_TYPE other = *((ENUM_TYPE*)odata); \
        if (me == other) \
            return 0; \
        return me > other ? 1 : -1; \
    } while (0)

#define COMPARE_ENUM_VALUES_RANGE(ENUM_TYPE) do { \
        ENUM_TYPE me = *((ENUM_TYPE*)tdata); \
        ENUM_TYPE other = *((ENUM_TYPE*)odata); \
        if (me == other) \
            return 0; \
        return me - other; \
    } while (0)

        switch (basetype->type)
        {
            case IL2CPP_TYPE_U1:
                COMPARE_ENUM_VALUES(uint8_t);
            case IL2CPP_TYPE_I1:
                COMPARE_ENUM_VALUES(int8_t);
            case IL2CPP_TYPE_CHAR:
                COMPARE_ENUM_VALUES_RANGE(Il2CppChar);
            case IL2CPP_TYPE_U2:
                COMPARE_ENUM_VALUES_RANGE(uint16_t);
            case IL2CPP_TYPE_I2:
                COMPARE_ENUM_VALUES(int16_t);
            case IL2CPP_TYPE_U4:
                COMPARE_ENUM_VALUES(uint32_t);
            case IL2CPP_TYPE_I4:
                COMPARE_ENUM_VALUES(int32_t);
            case IL2CPP_TYPE_U8:
                COMPARE_ENUM_VALUES(uint64_t);
            case IL2CPP_TYPE_I8:
                COMPARE_ENUM_VALUES(int64_t);
            default:
                IL2CPP_ASSERT(false && "Implement type 0x%02x in compare_value_to");
        }

#undef COMPARE_ENUM_VALUES_RANGE
#undef COMPARE_ENUM_VALUES
        return 0;
    }

    int32_t Enum::get_hashcode(Il2CppObject * thisPtr)
    {
        void* data = (char*)thisPtr + sizeof(Il2CppObject);
        Il2CppClass *basetype = thisPtr->klass->element_class;
        IL2CPP_ASSERT(basetype);

        if (basetype == il2cpp_defaults.sbyte_class)
            return *((int8_t*)data);
        if (basetype == il2cpp_defaults.byte_class)
            return *((uint8_t*)data);
        if (basetype == il2cpp_defaults.char_class)
            return *((Il2CppChar*)data);
        if (basetype == il2cpp_defaults.uint16_class)
            return *((uint16_t*)data);
        if (basetype == il2cpp_defaults.int16_class)
            return *((uint16_t*)data);
        if (basetype == il2cpp_defaults.uint32_class)
            return *((uint32_t*)data);
        if (basetype == il2cpp_defaults.int32_class)
            return *((int32_t*)data);
        if (basetype == il2cpp_defaults.uint64_class || basetype == il2cpp_defaults.int64_class)
        {
            int64_t value = *((int64_t*)data);
            return (int32_t)(value & 0xffffffff) ^ (int32_t)(value >> 32);
        }

        IL2CPP_ASSERT(0 && "System_Enum_get_hashcode_icall");
        return 0;
    }

    static uint64_t
    read_enum_value(char *mem, Il2CppClass* type)
    {
        if (type == il2cpp_defaults.byte_class)
            return *(int8_t*)mem;
        if (type == il2cpp_defaults.sbyte_class)
            return *(uint8_t*)mem;
        if (type == il2cpp_defaults.uint16_class)
            return *(uint16_t*)mem;
        if (type == il2cpp_defaults.int16_class)
            return *(int16_t*)mem;
        if (type == il2cpp_defaults.uint32_class)
            return *(uint32_t*)mem;
        if (type == il2cpp_defaults.int32_class)
            return *(int32_t*)mem;
        if (type == il2cpp_defaults.uint64_class)
            return *(uint64_t*)mem;
        if (type == il2cpp_defaults.int64_class)
            return *(int64_t*)mem;

        IL2CPP_ASSERT(0);

        return 0;
    }

    static void
    write_enum_value(char *mem, Il2CppClass* type, uint64_t value)
    {
        if (type == il2cpp_defaults.byte_class || type == il2cpp_defaults.sbyte_class)
        {
            uint8_t *p = (uint8_t*)mem;
            *p = (uint8_t)value;
        }
        else if (type == il2cpp_defaults.uint16_class || type == il2cpp_defaults.int16_class)
        {
            uint16_t *p = (uint16_t*)mem;
            *p = (uint16_t)value;
        }
        else if (type == il2cpp_defaults.uint32_class || type == il2cpp_defaults.int32_class)
        {
            uint32_t *p = (uint32_t*)mem;
            *p = (uint32_t)value;
        }
        else if (type == il2cpp_defaults.uint64_class || type == il2cpp_defaults.int64_class)
        {
            uint64_t *p = (uint64_t*)mem;
            *p = value;
        }
        else
        {
            IL2CPP_ASSERT(0);
        }
    }

    Il2CppObject * Enum::ToObject(Il2CppReflectionType * enumType, Il2CppObject * value)
    {
        //MonoDomain *domain;
        Il2CppClass *enumc, *objc;
        Il2CppObject *res;
        Il2CppClass *etype;
        uint64_t val;

        IL2CPP_CHECK_ARG_NULL(enumType);
        IL2CPP_CHECK_ARG_NULL(value);

        //domain = mono_object_domain (enumType);
        enumc = vm::Class::FromIl2CppType(enumType->type);
        objc = vm::Object::GetClass(value);

        //if (!enumc->enumtype)
        //  mono_raise_exception (mono_get_exception_argument ("enumType", "Type provided must be an Enum."));
        //if (!((objc->enumtype) || (objc->byval_arg.type >= MONO_TYPE_I1 && objc->byval_arg.type <= MONO_TYPE_U8)))
        //  mono_raise_exception (mono_get_exception_argument ("value", "The value passed in must be an enum base or an underlying type for an enum, such as an Int32."));

        etype = vm::Class::GetElementClass(enumc);
        if (!etype)
            /* MS throws this for typebuilders */
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("Type must be a type provided by the runtime.", "enumType"));

        res = (Il2CppObject*)il2cpp::vm::Object::New(enumc);
        //val = read_enum_value ((char *)value + sizeof (mscorlib_System_Object), objc->enumtype? mono_class_enum_basetype (objc)->type: objc->byval_arg.type);
        val = read_enum_value((char*)value + sizeof(Il2CppObject), objc->enumtype ? objc->element_class : objc);
        write_enum_value((char*)res + sizeof(Il2CppObject), etype, val);

        return res;
    }

    Il2CppReflectionType * Enum::get_underlying_type(Il2CppReflectionType *type)
    {
        const Il2CppType *etype;

        etype = vm::Class::GetEnumBaseType(vm::Class::FromIl2CppType(type->type));
        if (!etype)
            /* MS throws this for typebuilders */
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("Type must be a type provided by the runtime.", "enumType"));

        return il2cpp::vm::Reflection::GetTypeObject(etype);
    }

    bool Enum::GetEnumValuesAndNames(Il2CppReflectionRuntimeType* enumType, Il2CppArray** values, Il2CppArray** names)
    {
        return vm::Enum::GetEnumValuesAndNames(vm::Class::FromIl2CppType(enumType->type.type), values, names);
    }

    bool Enum::InternalHasFlag(Il2CppObject* thisPtr, Il2CppObject* flags)
    {
        Il2CppClass* enumClass = vm::Class::FromIl2CppType(vm::Class::GetEnumBaseType(thisPtr->klass));
        int32_t size = vm::Class::GetValueSize(enumClass, NULL);
        uint64_t a_val = 0, b_val = 0;

        memcpy(&a_val, vm::Object::Unbox(thisPtr), size);
        memcpy(&b_val, vm::Object::Unbox(flags), size);

        return (a_val & b_val) == b_val;
    }

    int32_t Enum::InternalCompareTo(Il2CppObject* o1, Il2CppObject* o2)
    {
        return compare_value_to(o1, o2);
    }

    Il2CppObject* Enum::InternalBoxEnum(Il2CppReflectionRuntimeType* enumType, int64_t value)
    {
        return vm::Object::Box(vm::Class::FromIl2CppType(enumType->type.type), &value);
    }

    Il2CppReflectionRuntimeType* Enum::InternalGetUnderlyingType(Il2CppReflectionRuntimeType* enumType)
    {
        return reinterpret_cast<Il2CppReflectionRuntimeType*>(get_underlying_type(&enumType->type));
    }

#if IL2CPP_TINY
    bool Enum::TinyEnumEquals(Il2CppObject* left, Il2CppObject* right)
    {
        if (left->klass != right->klass)
            return false;

        return compare_value_to(left, right) == 0;
    }

#endif
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
