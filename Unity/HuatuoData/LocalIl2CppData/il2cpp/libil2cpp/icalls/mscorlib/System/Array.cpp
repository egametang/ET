#include "il2cpp-config.h"
#include <memory>
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "gc/GarbageCollector.h"
#include "icalls/mscorlib/System/Array.h"
#include "utils/Exception.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Type.h"

#include <vector>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    static std::string FormatCreateInstanceException(const Il2CppType* type)
    {
        std::string typeName = vm::Type::GetName(type, IL2CPP_TYPE_NAME_FORMAT_IL);

        std::string message;
        message += "Unable to create an array of type '";
        message += typeName;
        message += "'. IL2CPP needs to know about the array type at compile time, so please define a private static field like this:\n\nprivate static ";
        message += typeName;
        message += "[] _unused;\n\nin any MonoBehaviour class, and this exception should go away.";

        return message;
    }

    Il2CppArray* Array::CreateInstanceImpl(Il2CppReflectionType* elementType, Il2CppArray* lengths, Il2CppArray* bounds)
    {
        int32_t* i32lengths = NULL;
        int32_t* i32bounds = NULL;
        il2cpp_array_size_t* arraySizeLengths = NULL;
        il2cpp_array_size_t* arraySizeBounds = NULL;

        if (lengths != NULL)
            i32lengths = (int32_t*)il2cpp_array_addr(lengths, int32_t, 0);

        if (bounds != NULL)
            i32bounds = (int32_t*)il2cpp_array_addr(bounds, int32_t, 0);

        int32_t boundsCount = bounds != NULL ? il2cpp::vm::Array::GetLength(bounds) : 0;

        Il2CppClass* arrayType = il2cpp::vm::Class::GetBoundedArrayClass(
            il2cpp::vm::Class::FromIl2CppType(elementType->type),
            il2cpp::vm::Array::GetLength(lengths),
            boundsCount > 2 || (boundsCount == 1 && i32bounds[0] != 0)
        );

        if (arrayType == NULL)
            vm::Exception::Raise(vm::Exception::GetInvalidOperationException(FormatCreateInstanceException(elementType->type).c_str()));

        //Convert the lengths and bounds of the array into il2cpp_array_size_t
        if (lengths)
        {
            arraySizeLengths = (il2cpp_array_size_t*)alloca(lengths->max_length * sizeof(il2cpp_array_size_t));
            for (il2cpp_array_size_t i = 0; i < lengths->max_length; i++)
            {
                arraySizeLengths[i] = i32lengths[i];
            }
        }

        if (bounds)
        {
            arraySizeBounds = (il2cpp_array_size_t*)alloca(bounds->max_length * sizeof(il2cpp_array_size_t));
            for (il2cpp_array_size_t i = 0; i < bounds->max_length; i++)
            {
                arraySizeBounds[i] = i32bounds[i];
            }
        }

        return (Il2CppArray*)il2cpp::vm::Array::NewFull(arrayType, arraySizeLengths, arraySizeBounds);
    }

    bool Array::FastCopy(Il2CppArray* source, int32_t source_idx, Il2CppArray* dest, int32_t dest_idx, int32_t length)
    {
        int element_size;
        Il2CppClass *src_class;
        Il2CppClass *dest_class;
        int i;

        if (source->klass->rank != dest->klass->rank)
            return false;

        if (source->bounds || dest->bounds)
            return false;

        // Our max array length is il2cpp_array_size_t, which is currently int32_t,
        // so Array::GetLength will never return more than 2^31 - 1
        // Therefore, casting sum to uint32_t is safe even if it overflows - it if does,
        // the comparison will succeed and this function will return false
        if ((static_cast<uint32_t>(dest_idx + length) > il2cpp::vm::Array::GetLength(dest)) ||
            (static_cast<uint32_t>(source_idx + length) > il2cpp::vm::Array::GetLength(source)))
            return false;

        src_class = source->klass->element_class;
        dest_class = dest->klass->element_class;

        // object[] -> valuetype[]
        if (src_class == il2cpp_defaults.object_class && dest_class->byval_arg.valuetype)
        {
            for (i = source_idx; i < source_idx + length; ++i)
            {
                Il2CppObject *elem = il2cpp_array_get(source, Il2CppObject*, i);
                if (elem && !vm::Object::IsInst(elem, dest_class))
                    return false;
            }

            element_size = il2cpp_array_element_size(dest->klass);
            void *baseAddr = il2cpp_array_addr_with_size(dest, element_size, dest_idx);

            size_t byte_len = (size_t)length * element_size;
            memset(baseAddr, 0, byte_len);

            for (i = 0; i < length; ++i)
            {
                Il2CppObject *elem = il2cpp_array_get(source, Il2CppObject*, source_idx + (size_t)i);

                if (!elem)
                    vm::Exception::Raise(vm::Exception::GetInvalidCastException("At least one element in the source array could not be cast down to the destination array type."));

                memcpy(il2cpp_array_addr_with_size(dest, element_size, dest_idx + (size_t)i), vm::Object::Unbox(elem), element_size);
            }
            gc::GarbageCollector::SetWriteBarrier((void**)baseAddr, byte_len);

            return true;
        }

        if (src_class != dest_class)
        {
            if (vm::Class::IsValuetype(dest_class) || vm::Class::IsEnum(dest_class) || vm::Class::IsValuetype(src_class) || vm::Class::IsEnum(src_class))
                return false;

            // object[] -> reftype[]
            if (vm::Class::IsSubclassOf(dest_class, src_class, false))
            {
                for (i = source_idx; i < source_idx + length; ++i)
                {
                    Il2CppObject *elem = il2cpp_array_get(source, Il2CppObject*, i);
                    if (elem && !vm::Object::IsInst(elem, dest_class))
                        vm::Exception::Raise(vm::Exception::GetInvalidCastException("At least one element in the source array could not be cast down to the destination array type."));
                }
            }
            else if (!vm::Class::IsSubclassOf(src_class, dest_class, false))
                return false;

            // derivedtype[] -> basetype[]
            IL2CPP_ASSERT(vm::Type::IsReference(&src_class->byval_arg));
            IL2CPP_ASSERT(vm::Type::IsReference(&dest_class->byval_arg));
        }

        element_size = il2cpp_array_element_size(dest->klass);

        IL2CPP_ASSERT(element_size == il2cpp_array_element_size(source->klass));

        size_t byte_len = (size_t)length * element_size;

        memmove(
            il2cpp_array_addr_with_size(dest, element_size, dest_idx),
            il2cpp_array_addr_with_size(source, element_size, source_idx),
            byte_len);

        gc::GarbageCollector::SetWriteBarrier((void**)il2cpp_array_addr_with_size(dest, element_size, dest_idx), byte_len);

        return true;
    }

    int32_t Array::GetLength(Il2CppArray* thisPtr, int32_t dimension)
    {
        int32_t rank = thisPtr->klass->rank;
        il2cpp_array_size_t length;

        if ((dimension < 0) || (dimension >= rank))
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetIndexOutOfRangeException());

        if (thisPtr->bounds == NULL)
            length = thisPtr->max_length;
        else
            length = thisPtr->bounds[dimension].length;

#ifdef IL2CPP_BIG_ARRAYS
        if (length > G_MAXINT32)
            mono_raise_exception(mono_get_exception_overflow());
#endif
        return ARRAY_LENGTH_AS_INT32(length);
    }

    int32_t Array::GetLowerBound(Il2CppArray* thisPtr, int32_t dimension)
    {
        int32_t rank = thisPtr->klass->rank;

        if ((dimension < 0) || (dimension >= rank))
            vm::Exception::Raise(vm::Exception::GetIndexOutOfRangeException());

        if (thisPtr->bounds == NULL)
            return false;

        return thisPtr->bounds[dimension].lower_bound;
    }

    int32_t Array::GetRank(Il2CppArray* arr)
    {
        return arr->klass->rank;
    }

    Il2CppObject* Array::GetValue(Il2CppArray* thisPtr, Il2CppArray* indices)
    {
        Il2CppClass *ac, *ic;
        Il2CppArray *ao, *io;
        int32_t i, *ind;
        il2cpp_array_size_t pos;

        IL2CPP_CHECK_ARG_NULL(indices);

        io = (Il2CppArray*)indices;
        ic = (Il2CppClass*)io->klass;

        ao = (Il2CppArray*)thisPtr;
        ac = (Il2CppClass*)ao->klass;

        IL2CPP_ASSERT(ic->rank == 1);
        if (io->bounds != NULL || io->max_length !=  ac->rank)
            vm::Exception::Raise(vm::Exception::GetArgumentException(NULL, NULL));

        ind = (int32_t*)il2cpp::vm::Array::GetFirstElementAddress(io);

        if (ao->bounds == NULL)
        {
            if (*ind < 0 || *ind >= ARRAY_LENGTH_AS_INT32(ao->max_length))
                vm::Exception::Raise(vm::Exception::GetIndexOutOfRangeException());

            return GetValueImpl(thisPtr, *ind);
        }

        for (i = 0; i < ac->rank; i++)
            if ((ind[i] < ao->bounds[i].lower_bound) ||
                (ind[i] >= ARRAY_LENGTH_AS_INT32(ao->bounds[i].length) + ao->bounds[i].lower_bound))
                vm::Exception::Raise(vm::Exception::GetIndexOutOfRangeException());

        pos = ind[0] - ao->bounds[0].lower_bound;
        for (i = 1; i < ac->rank; i++)
            pos = pos * ao->bounds[i].length + ind[i] - ao->bounds[i].lower_bound;

        return GetValueImpl(thisPtr, ARRAY_LENGTH_AS_INT32(pos));
    }

    Il2CppObject* Array::GetValueImpl(Il2CppArray* thisPtr, int32_t pos)
    {
        Il2CppClass* typeInfo = thisPtr->klass;
        void **ea;

        ea = (void**)il2cpp_array_addr_with_size(thisPtr, pos, typeInfo->element_size);

        if (typeInfo->element_class->byval_arg.valuetype)
            return il2cpp::vm::Object::Box(typeInfo->element_class, ea);

        return (Il2CppObject*)*ea;
    }

    void Array::ClearInternal(Il2CppArray* arr, int32_t index, int32_t count)
    {
        int sz = il2cpp_array_element_size(arr->klass);
        memset(il2cpp_array_addr_with_size(arr, sz, index), 0, count * sz);
    }

    void Array::SetValue(Il2CppArray* thisPtr, Il2CppObject* value, Il2CppArray* indices)
    {
        Il2CppClass *ac, *ic;
        int32_t i, *ind;
        il2cpp_array_size_t pos;

        IL2CPP_CHECK_ARG_NULL(indices);

        ic = indices->klass;
        ac = thisPtr->klass;

        IL2CPP_ASSERT(ic->rank == 1);
        if (indices->bounds != NULL || indices->max_length != ac->rank)
            vm::Exception::Raise(vm::Exception::GetArgumentException(NULL, NULL));

        ind = (int32_t*)il2cpp::vm::Array::GetFirstElementAddress(indices);

        if (thisPtr->bounds == NULL)
        {
            if (*ind < 0 || *ind >= ARRAY_LENGTH_AS_INT32(thisPtr->max_length))
                vm::Exception::Raise(vm::Exception::GetIndexOutOfRangeException());

            SetValueImpl(thisPtr, value, *ind);
            return;
        }

        for (i = 0; i < ac->rank; i++)
            if ((ind[i] < thisPtr->bounds[i].lower_bound) ||
                (ind[i] >= (il2cpp_array_lower_bound_t)thisPtr->bounds[i].length + thisPtr->bounds[i].lower_bound))
                vm::Exception::Raise(vm::Exception::GetIndexOutOfRangeException());

        pos = ind[0] - thisPtr->bounds[0].lower_bound;
        for (i = 1; i < ac->rank; i++)
            pos = pos * thisPtr->bounds[i].length + ind[i] -
                thisPtr->bounds[i].lower_bound;

        SetValueImpl(thisPtr, value, ARRAY_LENGTH_AS_INT32(pos));
    }

    static void ThrowNoWidening()
    {
        vm::Exception::Raise(vm::Exception::GetArgumentException("value", "not a widening conversion"));
    }

    static void ThrowInvalidCast(const Il2CppClass* a, const Il2CppClass* b)
    {
        vm::Exception::Raise(vm::Exception::GetInvalidCastException(utils::Exception::FormatInvalidCastException(b, a).c_str()));
    }

    union WidenedValueUnion
    {
        int64_t i64;
        uint64_t u64;
        double r64;
    };

    WidenedValueUnion ExtractWidenedValue(Il2CppTypeEnum type, void* value)
    {
        WidenedValueUnion extractedValue = { 0 };
        switch (type)
        {
            case IL2CPP_TYPE_U1:
                extractedValue.u64 = *(uint8_t*)value;
                break;
            case IL2CPP_TYPE_CHAR:
                extractedValue.u64 = *(Il2CppChar*)value;
                break;
            case IL2CPP_TYPE_U2:
                extractedValue.u64 = *(uint16_t*)value;
                break;
            case IL2CPP_TYPE_U4:
                extractedValue.u64 = *(uint32_t*)value;
                break;
            case IL2CPP_TYPE_U8:
                extractedValue.u64 = *(uint64_t*)value;
                break;
            case IL2CPP_TYPE_I1:
                extractedValue.i64 = *(int8_t*)value;
                break;
            case IL2CPP_TYPE_I2:
                extractedValue.i64 = *(int16_t*)value;
                break;
            case IL2CPP_TYPE_I4:
                extractedValue.i64 = *(int32_t*)value;
                break;
            case IL2CPP_TYPE_I8:
                extractedValue.i64 = *(int64_t*)value;
                break;
            case IL2CPP_TYPE_R4:
                extractedValue.r64 = *(float*)value;
                break;
            case IL2CPP_TYPE_R8:
                extractedValue.r64 = *(double*)value;
                break;
            default:
                IL2CPP_ASSERT(0);
                break;
        }

        return extractedValue;
    }

    static void CheckWideningConversion(size_t elementSize, size_t valueSize, size_t extra = 0)
    {
        if (elementSize < valueSize + (extra))
            ThrowNoWidening();
    }

    template<typename T>
    static void AssignUnsigned(WidenedValueUnion value, void* elementAddress, Il2CppTypeEnum valueType, size_t elementSize, size_t valueSize)
    {
        switch (valueType)
        {
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_U4:
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_CHAR:
                CheckWideningConversion(elementSize, valueSize);
                *(T*)elementAddress = (T)value.u64;
                break;
            /* You can't assign a signed value to an unsigned array. */
            case IL2CPP_TYPE_I1:
            case IL2CPP_TYPE_I2:
            case IL2CPP_TYPE_I4:
            case IL2CPP_TYPE_I8:
            /* You can't assign a floating point number to an integer array. */
            case IL2CPP_TYPE_R4:
            case IL2CPP_TYPE_R8:
                ThrowNoWidening();
                break;
            default:
                IL2CPP_ASSERT(0);
                break;
        }
    }

    template<typename T>
    static void AssignSigned(WidenedValueUnion value, void* elementAddress, Il2CppTypeEnum valueType, size_t elementSize, size_t valueSize)
    {
        switch (valueType)
        {
            /* You can assign an unsigned value to a signed array if the array's
                element size is larger than the value size. */
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_U4:
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_CHAR:
                CheckWideningConversion(elementSize, valueSize, 1);
                *(T*)elementAddress = (T)value.u64;
                break;
            case IL2CPP_TYPE_I1:
            case IL2CPP_TYPE_I2:
            case IL2CPP_TYPE_I4:
            case IL2CPP_TYPE_I8:
                CheckWideningConversion(elementSize, valueSize);
                *(T*)elementAddress = (T)value.i64;
                break;
            /* You can't assign a floating point number to an integer array. */
            case IL2CPP_TYPE_R4:
            case IL2CPP_TYPE_R8:
                ThrowNoWidening();
                break;
            default:
                IL2CPP_ASSERT(0);
                break;
        }
    }

    template<typename T>
    static void AssignReal(WidenedValueUnion value, void* elementAddress, Il2CppTypeEnum valueType, size_t elementSize, size_t valueSize)
    {
        switch (valueType)
        {
            /* All integers fit into the floating point value range. No need to check size. */
            case IL2CPP_TYPE_U1:
            case IL2CPP_TYPE_U2:
            case IL2CPP_TYPE_U4:
            case IL2CPP_TYPE_U8:
            case IL2CPP_TYPE_CHAR:
                *(T*)elementAddress = (T)value.u64;
                break;
            case IL2CPP_TYPE_I1:
            case IL2CPP_TYPE_I2:
            case IL2CPP_TYPE_I4:
            case IL2CPP_TYPE_I8:
                *(T*)elementAddress = (T)value.i64;
                break;
            case IL2CPP_TYPE_R4:
            case IL2CPP_TYPE_R8:
                CheckWideningConversion(elementSize, valueSize);
                *(T*)elementAddress = (T)value.r64;
                break;
            default:
                IL2CPP_ASSERT(0);
                break;
        }
    }

    void Array::SetValueImpl(Il2CppArray* thisPtr, Il2CppObject* value, int32_t index)
    {
        Il2CppClass* typeInfo = thisPtr->klass;
        Il2CppClass* elementClass = vm::Class::GetElementClass(typeInfo);

        int elementSize = vm::Class::GetArrayElementSize(elementClass);
        void* elementAddress = il2cpp_array_addr_with_size(thisPtr, elementSize, index);

        if (vm::Class::IsNullable(elementClass))
        {
            vm::Object::NullableInit((uint8_t*)elementAddress, value, elementClass);
            return;
        }

        if (value == NULL)
        {
            memset(elementAddress, 0, elementSize);
            return;
        }

        if (!vm::Class::IsValuetype(elementClass))
        {
            if (!vm::Object::IsInst(value, elementClass))
                vm::Exception::Raise(vm::Exception::GetInvalidCastException(utils::Exception::FormatInvalidCastException(thisPtr->klass->element_class, value->klass).c_str()));
            il2cpp_array_setref(thisPtr, index, value);
            return;
        }

        if (vm::Object::IsInst(value, elementClass))
        {
            memcpy(elementAddress, vm::Object::Unbox(value), elementSize);
            gc::GarbageCollector::SetWriteBarrier((void**)elementAddress, elementSize);
            return;
        }

        Il2CppClass* valueClass = vm::Object::GetClass(value);

        if (!vm::Class::IsValuetype(valueClass))
            ThrowInvalidCast(elementClass, valueClass);

        int valueSize = vm::Class::GetInstanceSize(valueClass) - sizeof(Il2CppObject);

        Il2CppTypeEnum elementType = vm::Class::IsEnum(elementClass) ? vm::Class::GetEnumBaseType(elementClass)->type : elementClass->byval_arg.type;
        Il2CppTypeEnum valueType = vm::Class::IsEnum(valueClass) ? vm::Class::GetEnumBaseType(valueClass)->type : valueClass->byval_arg.type;

        if (elementType == IL2CPP_TYPE_BOOLEAN)
        {
            switch (valueType)
            {
                case IL2CPP_TYPE_BOOLEAN:
                    break;
                case IL2CPP_TYPE_CHAR:
                case IL2CPP_TYPE_U1:
                case IL2CPP_TYPE_U2:
                case IL2CPP_TYPE_U4:
                case IL2CPP_TYPE_U8:
                case IL2CPP_TYPE_I1:
                case IL2CPP_TYPE_I2:
                case IL2CPP_TYPE_I4:
                case IL2CPP_TYPE_I8:
                case IL2CPP_TYPE_R4:
                case IL2CPP_TYPE_R8:
                    ThrowNoWidening();
                default:
                    ThrowInvalidCast(elementClass, valueClass);
            }
        }

        WidenedValueUnion widenedValue = ExtractWidenedValue(valueType, vm::Object::Unbox(value));

        switch (elementType)
        {
            case IL2CPP_TYPE_U1:
                AssignUnsigned<uint8_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_CHAR:
                AssignUnsigned<Il2CppChar>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_U2:
                AssignUnsigned<uint16_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_U4:
                AssignUnsigned<uint32_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_U8:
                AssignUnsigned<uint64_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_I1:
                AssignSigned<int8_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_I2:
                AssignSigned<int16_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_I4:
                AssignSigned<int32_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_I8:
                AssignSigned<int64_t>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_R4:
                AssignReal<float>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            case IL2CPP_TYPE_R8:
                AssignReal<double>(widenedValue, elementAddress, valueType, elementSize, valueSize);
                break;
            default:
                ThrowInvalidCast(elementClass, valueClass);
                break;
        }
    }
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
