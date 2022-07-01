#include "il2cpp-config.h"
#include "gc/gc_wrapper.h"
#include "gc/GarbageCollector.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Profiler.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include <memory>

namespace il2cpp
{
namespace vm
{
    Il2CppArray* Array::Clone(Il2CppArray* arr)
    {
        Il2CppClass *typeInfo = arr->klass;
        const uint32_t elem_size = il2cpp::vm::Array::GetElementSize(typeInfo);

        if (arr->bounds == NULL)
        {
            il2cpp_array_size_t len = il2cpp::vm::Array::GetLength(arr);
            Il2CppArray *clone = (Il2CppArray*)il2cpp::vm::Array::NewFull(typeInfo, &len, NULL);
            memcpy(il2cpp::vm::Array::GetFirstElementAddress(clone), il2cpp::vm::Array::GetFirstElementAddress(arr), elem_size * len);

            gc::GarbageCollector::SetWriteBarrier((void**)il2cpp::vm::Array::GetFirstElementAddress(clone), elem_size * len);

            return clone;
        }

        il2cpp_array_size_t size = elem_size;
        std::vector<il2cpp_array_size_t> lengths(typeInfo->rank);
        std::vector<il2cpp_array_size_t> lowerBounds(typeInfo->rank);

        for (int i = 0; i < typeInfo->rank; ++i)
        {
            lengths[i] = arr->bounds[i].length;
            size *= arr->bounds[i].length;
            lowerBounds[i] = arr->bounds[i].lower_bound;
        }

        Il2CppArray* clone = il2cpp::vm::Array::NewFull(typeInfo, &lengths[0], &lowerBounds[0]);
        memcpy(il2cpp::vm::Array::GetFirstElementAddress(clone), il2cpp::vm::Array::GetFirstElementAddress(arr), size);

        gc::GarbageCollector::SetWriteBarrier((void**)il2cpp::vm::Array::GetFirstElementAddress(clone), size);

        return clone;
    }

    int32_t Array::GetElementSize(const Il2CppClass *klass)
    {
        IL2CPP_ASSERT(klass->rank);
        return klass->element_size;
    }

    uint32_t Array::GetLength(Il2CppArray* array)
    {
        return ARRAY_LENGTH_AS_INT32(array->max_length);
    }

    uint32_t Array::GetByteLength(Il2CppArray* array)
    {
        Il2CppClass *klass;
        il2cpp_array_size_t length;
        int i;

        klass = array->klass;

        if (array->bounds == NULL)
            length = array->max_length;
        else
        {
            length = 1;
            for (i = 0; i < klass->rank; ++i)
                length *= array->bounds[i].length;
        }

        return ARRAY_LENGTH_AS_INT32(length * GetElementSize(klass));
    }

    Il2CppArray* Array::New(Il2CppClass *elementTypeInfo, il2cpp_array_size_t length)
    {
        return NewSpecific(Class::GetArrayClass(elementTypeInfo, 1), length);
    }

    static void RaiseOverflowException()
    {
        vm::Exception::Raise(vm::Exception::GetOverflowException("Arithmetic operation resulted in an overflow."));
    }

    Il2CppArray* Array::NewSpecific(Il2CppClass *klass, il2cpp_array_size_t n)
    {
        Il2CppObject *o;
        Il2CppArray *ao;
        uint32_t elem_size;
        il2cpp_array_size_t byte_len;

        Class::Init(klass);
        IL2CPP_ASSERT(klass->rank);
        IL2CPP_ASSERT(klass->initialized);
        IL2CPP_ASSERT(klass->element_class->initialized);
        IL2CPP_ASSERT(klass->byval_arg.type == IL2CPP_TYPE_SZARRAY);

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Array::NewSpecific, "Not checking for overflow");
        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Array::NewSpecific, "Handle allocations with a GC descriptor");

        if (n > IL2CPP_ARRAY_MAX_INDEX)
        {
            RaiseOverflowException();
            return NULL;
        }

        elem_size = il2cpp_array_element_size(klass);
        //if (CHECK_MUL_OVERFLOW_UN (n, elem_size)) {
        //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
        //  return NULL;
        //}
        byte_len = n * elem_size;
        //if (CHECK_ADD_OVERFLOW_UN (byte_len, sizeof (MonoArray))) {
        //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
        //  return NULL;
        //}
        byte_len += kIl2CppSizeOfArray;
        if (!klass->has_references)
        {
            o = Object::AllocatePtrFree(byte_len, klass);
#if NEED_TO_ZERO_PTRFREE
            ((Il2CppArray*)o)->bounds = NULL;
            memset((char*)o + sizeof(Il2CppObject), 0, byte_len - sizeof(Il2CppObject));
#endif
        }
#if !RUNTIME_TINY
        else if (klass->element_class->valuetype &&
                 ((GC_descr)klass->element_class->gc_desc & GC_DS_TAGS) == GC_DS_BITMAP)
        {
            o = (Il2CppObject*)GC_gcj_vector_malloc(byte_len, klass);
        }
#endif
#if IL2CPP_HAS_GC_DESCRIPTORS
        else if (klass->gc_desc != GC_NO_DESCRIPTOR)
        {
            o = Object::AllocateSpec(byte_len, klass);
        }
#endif
        else
        {
            o = Object::Allocate(byte_len, klass);
        }

        ao = (Il2CppArray*)o;
        ao->max_length = n;

#if IL2CPP_ENABLE_PROFILER
        if (Profiler::ProfileAllocations())
            Profiler::Allocation(o, klass);
#endif

        return ao;
    }

    Il2CppArray* Array::NewFull(Il2CppClass *array_class, il2cpp_array_size_t *lengths, il2cpp_array_size_t *lower_bounds)
    {
        il2cpp_array_size_t byte_len, len, bounds_size;
        Il2CppObject *o;
        Il2CppArray *array;
        int i;

        Class::Init(array_class);
        IL2CPP_ASSERT(array_class->rank);
        IL2CPP_ASSERT(array_class->initialized);
        IL2CPP_ASSERT(array_class->element_class->initialized);

        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Array::NewFull, "IGNORING non-zero based arrays!");
        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Array::NewFull, "Handle allocations with a GC descriptor");

        byte_len = il2cpp_array_element_size(array_class);
        len = 1;

        /* A single dimensional array with a 0 lower bound is the same as an szarray */
        if (array_class->rank == 1 && ((array_class->byval_arg.type == IL2CPP_TYPE_SZARRAY) || (lower_bounds && lower_bounds[0] == 0)))
        {
            /* A single dimensional array with a 0 lower bound should be an szarray */
            /* but the caller asked for an IL2CPP_TYPE_ARRAY, which insn't correct */
            IL2CPP_ASSERT(array_class->byval_arg.type == IL2CPP_TYPE_SZARRAY);

            len = lengths[0];
            if (len > IL2CPP_ARRAY_MAX_INDEX) //MONO_ARRAY_MAX_INDEX
                RaiseOverflowException();
            bounds_size = 0;
        }
        else
        {
            IL2CPP_ASSERT(array_class->byval_arg.type == IL2CPP_TYPE_ARRAY);

            bounds_size = sizeof(Il2CppArrayBounds) * array_class->rank;

            for (i = 0; i < array_class->rank; ++i)
            {
                if (lengths[i] > IL2CPP_ARRAY_MAX_INDEX)  //MONO_ARRAY_MAX_INDEX
                    RaiseOverflowException();
                //if (CHECK_MUL_OVERFLOW_UN (len, lengths [i]))
                //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
                len *= lengths[i];
            }
        }

        //if (CHECK_MUL_OVERFLOW_UN (byte_len, len))
        //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
        byte_len *= len;
        //if (CHECK_ADD_OVERFLOW_UN (byte_len, sizeof (MonoArray)))
        //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
        byte_len += kIl2CppSizeOfArray;
        if (bounds_size)
        {
            /* align */
            //if (CHECK_ADD_OVERFLOW_UN (byte_len, 3))
            //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
            byte_len = (byte_len + (IL2CPP_SIZEOF_VOID_P - 1)) & ~(IL2CPP_SIZEOF_VOID_P - 1);
            //if (CHECK_ADD_OVERFLOW_UN (byte_len, bounds_size))
            //  mono_gc_out_of_memory (MONO_ARRAY_MAX_SIZE);
            byte_len += bounds_size;
        }
        /*
         * Following three lines almost taken from mono_object_new ():
         * they need to be kept in sync.
         */
        if (!array_class->has_references)
        {
            o = Object::AllocatePtrFree(byte_len, array_class);
#if NEED_TO_ZERO_PTRFREE
            memset((char*)o + sizeof(Il2CppObject), 0, byte_len - sizeof(Il2CppObject));
#endif
        }
#if IL2CPP_HAS_GC_DESCRIPTORS
        else if (array_class->gc_desc != GC_NO_DESCRIPTOR)
        {
            o = Object::AllocateSpec(byte_len, array_class);
        }
#endif
        else
        {
            o = Object::Allocate(byte_len, array_class);
        }

        array = (Il2CppArray*)o;
        array->max_length = len;

        if (bounds_size)
        {
            Il2CppArrayBounds *bounds = (Il2CppArrayBounds*)((char*)array + byte_len - bounds_size);
            array->bounds = bounds;
            for (i = 0; i < array_class->rank; ++i)
            {
                bounds[i].length = lengths[i];
                if (lower_bounds)
                    bounds[i].lower_bound = ARRAY_LENGTH_AS_INT32(lower_bounds[i]);
            }
        }

#if IL2CPP_ENABLE_PROFILER
        if (Profiler::ProfileAllocations())
            Profiler::Allocation(o, array_class);
#endif

        return array;
    }

    char* Array::GetFirstElementAddress(Il2CppArray *array)
    {
        return reinterpret_cast<char*>(array) + kIl2CppSizeOfArray;
    }
} /* namespace vm */
} /* namespace il2cpp */

LIBIL2CPP_CODEGEN_API char*
il2cpp_array_addr_with_size(Il2CppArray *array, int32_t size, uintptr_t idx)
{
    return ((char*)array) + kIl2CppSizeOfArray + size * idx;
}

LIBIL2CPP_CODEGEN_API int32_t
il2cpp_array_element_size(Il2CppClass *ac)
{
    return il2cpp::vm::Array::GetElementSize(ac);
}
