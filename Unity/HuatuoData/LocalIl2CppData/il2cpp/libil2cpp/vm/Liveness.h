#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppClass;
struct Il2CppObject;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Liveness
    {
    public:
        typedef void (*register_object_callback)(Il2CppObject** arr, int size, void* userdata);
        typedef void*(*ReallocateArrayCallback)(void* ptr, size_t size, void* state);

        static void* AllocateStruct(Il2CppClass* filter, int max_object_count, register_object_callback callback, void* userdata, ReallocateArrayCallback reallocateArray);
        static void FreeStruct(void* state);
        static void Finalize(void* state);
        static void FromRoot(Il2CppObject* root, void* state);
        static void FromStatics(void* state);
    };
} /* namespace vm */
} /* namespace il2cpp */
