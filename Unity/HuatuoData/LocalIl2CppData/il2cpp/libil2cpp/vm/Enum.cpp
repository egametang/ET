#include "vm/Enum.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "gc/WriteBarrier.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Reflection.h"
#include "vm/GenericClass.h"
#include "vm/Field.h"
#include "vm/String.h"
#include "vm/Type.h"
#include "utils/MemoryRead.h"

namespace il2cpp
{
namespace vm
{
    static uint64_t GetEnumFieldValue(Il2CppClass* enumType, FieldInfo* field)
    {
        const Il2CppType* type = NULL;
        const char* ptr = Class::GetFieldDefaultValue(field, &type);

        switch (Class::GetEnumBaseType(enumType)->type)
        {
            case IL2CPP_TYPE_I1: // Sign extend
                return static_cast<int64_t>(static_cast<int8_t>(*ptr));

            case IL2CPP_TYPE_U1:
                return (uint8_t)*ptr;

            case IL2CPP_TYPE_CHAR:
                return utils::ReadChar(ptr);

            case IL2CPP_TYPE_I2: // Sign extend
                return static_cast<int64_t>(static_cast<int16_t>(utils::Read16(ptr)));

            case IL2CPP_TYPE_U2:
                return utils::Read16(ptr);

            case IL2CPP_TYPE_I4: // Sign extend
                return static_cast<int64_t>(static_cast<int32_t>(utils::Read32(ptr)));

            case IL2CPP_TYPE_U4:
                return utils::Read32(ptr);

            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_I8:
                return utils::Read64(ptr);

            default:
                IL2CPP_ASSERT(0);
                return 0;
        }
    }

    bool Enum::GetEnumValuesAndNames(Il2CppClass* enumType, Il2CppArray** values, Il2CppArray** names)
    {
        size_t nvalues = Class::GetNumFields(enumType) ? Class::GetNumFields(enumType) - 1 : 0;
        bool sorted = true;

        gc::WriteBarrier::GenericStore(values, vm::Array::New(il2cpp_defaults.uint64_class, (il2cpp_array_size_t)nvalues));
        uint64_t field_value, previous_value = 0;
        gc::WriteBarrier::GenericStore(names, vm::Array::New(il2cpp_defaults.string_class, (il2cpp_array_size_t)nvalues));

        if (enumType->generic_class)
            enumType = GenericClass::GetTypeDefinition(enumType->generic_class);

        FieldInfo* field;
        void* iter = NULL;

        int j = 0;
        while ((field = Class::GetFields(enumType, &iter)))
        {
            if (strcmp("value__", field->name) == 0)
                continue;

            if (Field::IsDeleted(field))
                continue;

            il2cpp_array_setref(*names, j, il2cpp::vm::String::New(Field::GetName(field)));

            field_value = GetEnumFieldValue(enumType, field);
            il2cpp_array_set(*values, uint64_t, j, field_value);

            if (previous_value > field_value)
                sorted = false;

            previous_value = field_value;

            j++;
        }

        return sorted;
    }
} /* namespace vm */
} /* namespace il2cpp */
