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

    int32_t Enum::get_hashcode(Il2CppObject* thisPtr)
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

    int32_t Enum::InternalCompareTo(Il2CppObject* o1, Il2CppObject* o2)
    {
        void* tdata = (char*)o1 + sizeof(Il2CppObject);
        void* odata = (char*)o2 + sizeof(Il2CppObject);
        const Il2CppType* basetype = vm::Class::GetEnumBaseType(vm::Object::GetClass(o1));
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

    Il2CppObject* Enum::get_value(Il2CppObject* thisPtr)
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

    Il2CppObject* Enum::InternalBoxEnum(Il2CppReflectionRuntimeType* enumType, int64_t value)
    {
        return vm::Object::Box(vm::Class::FromIl2CppType(enumType->type.type), &value);
    }

    Il2CppReflectionRuntimeType* Enum::InternalGetUnderlyingType(Il2CppReflectionRuntimeType* enumType)
    {
        const Il2CppType* etype;

        etype = vm::Class::GetEnumBaseType(vm::Class::FromIl2CppType(enumType->type.type));
        if (!etype)
            /* MS throws this for typebuilders */
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("Type must be a type provided by the runtime.", "enumType"));

        return reinterpret_cast<Il2CppReflectionRuntimeType*>(il2cpp::vm::Reflection::GetTypeObject(etype));
    }

#if IL2CPP_TINY
    bool Enum::TinyEnumEquals(Il2CppObject* left, Il2CppObject* right)
    {
        if (left->klass != right->klass)
            return false;

        return InternalCompareTo(left, right) == 0;
    }

#endif
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
