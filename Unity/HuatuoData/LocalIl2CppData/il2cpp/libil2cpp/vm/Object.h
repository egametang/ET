#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppObject;
struct Il2CppClass;
struct MethodInfo;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Object
    {
    public:
        static Il2CppObject* Box(Il2CppClass *klass, void* data);
        static Il2CppClass* GetClass(Il2CppObject* obj);
        static int32_t GetHash(Il2CppObject* obj);
        static uint32_t GetSize(Il2CppObject* obj);
        static const MethodInfo* GetVirtualMethod(Il2CppObject *obj, const MethodInfo *method);
        static Il2CppObject * IsInst(Il2CppObject *obj, Il2CppClass *klass);
        static Il2CppObject* New(Il2CppClass *klass);
        static void* Unbox(Il2CppObject* obj);
        static void UnboxNullable(Il2CppObject* obj, Il2CppClass* nullableArgumentClass, void* storage);
        static void UnboxNullableWithWriteBarrier(Il2CppObject* obj, Il2CppClass* nullableArgumentClass, void* storage);

        static Il2CppObject * Clone(Il2CppObject *obj);
        static Il2CppObject* NewPinned(Il2CppClass *klass);
        static void NullableInit(uint8_t* buf, Il2CppObject* value, Il2CppClass* klass);
        static bool NullableHasValue(Il2CppClass* klass, void* data);
    private:
        static Il2CppObject * NewAllocSpecific(Il2CppClass *klass);
        static Il2CppObject* NewPtrFree(Il2CppClass *klass);
        static Il2CppObject* Allocate(size_t size, Il2CppClass *typeInfo);
        static Il2CppObject* AllocatePtrFree(size_t size, Il2CppClass *typeInfo);
        static Il2CppObject* AllocateSpec(size_t size, Il2CppClass *typeInfo);

        // Yo! Don't call this function! See the comments in the implementation if you do.
        static uint32_t UnboxNullableGCUnsafe(Il2CppObject* obj, Il2CppClass* nullableArgumentClass, void* storage);

        friend class Array;
        friend class RCW;
        friend class String;
    };
} /* namespace vm */
} /* namespace il2cpp */
