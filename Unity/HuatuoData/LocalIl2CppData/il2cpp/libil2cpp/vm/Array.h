#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppArray;
struct Il2CppObject;
struct Il2CppString;
struct Il2CppClass;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Array
    {
    public:
        static Il2CppArray* Clone(Il2CppArray* arr);
        static int32_t GetElementSize(const Il2CppClass *klass);
        static uint32_t GetLength(Il2CppArray* array);
        static uint32_t GetByteLength(Il2CppArray* array);
        static Il2CppArray* New(Il2CppClass *elementTypeInfo, il2cpp_array_size_t length);
        static Il2CppArray* NewSpecific(Il2CppClass *arrayTypeInfo, il2cpp_array_size_t length);
        static Il2CppArray* NewFull(Il2CppClass *array_class, il2cpp_array_size_t *lengths, il2cpp_array_size_t *lower_bounds);
    public:
        // internal
        static Il2CppArray* NewCached(Il2CppClass *elementTypeInfo, il2cpp_array_size_t length)
        {
            return New(elementTypeInfo, length);
        }

        static char* GetFirstElementAddress(Il2CppArray *array);
    };
} /* namespace vm */
} /* namespace il2cpp */

LIBIL2CPP_CODEGEN_API char* il2cpp_array_addr_with_size(Il2CppArray *array, int32_t size, uintptr_t idx);

extern "C"
{
    IL2CPP_EXPORT int il2cpp_array_element_size(const Il2CppClass *ac);
}

#define load_array_elema(arr, idx, size) ((((uint8_t*)(arr)) + kIl2CppSizeOfArray) + ((size) * (idx)))

#define il2cpp_array_setwithsize(array, elementSize, index, value)  \
    do {    \
        void*__p = (void*) il2cpp_array_addr_with_size ((array), elementSize, (index)); \
        memcpy(__p, &(value), elementSize); \
    } while (0)
#define il2cpp_array_setrefwithsize(array, elementSize, index, value)  \
    do {    \
        void*__p = (void*) il2cpp_array_addr_with_size ((array), elementSize, (index)); \
        memcpy(__p, value, elementSize); \
        } while (0)
#define il2cpp_array_addr(array, type, index) ((type*)(void*) il2cpp_array_addr_with_size (array, sizeof (type), index))
#define il2cpp_array_get(array, type, index) ( *(type*)il2cpp_array_addr ((array), type, (index)) )
#define il2cpp_array_set(array, type, index, value)    \
    do {    \
        type *__p = (type *) il2cpp_array_addr ((array), type, (index));    \
        *__p = (value); \
    } while (0)
#define il2cpp_array_setref(array, index, value)  \
    do {    \
        void* *__p = (void* *) il2cpp_array_addr ((array), void*, (index)); \
         *__p = (value);    \
        il2cpp_gc_wbarrier_set_field((Il2CppObject *)(array), __p, (value));\
    } while (0)
