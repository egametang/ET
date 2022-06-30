#pragma once

#include "gc/GarbageCollector.h"

struct Il2CppIUnknown;
struct Il2CppObject;
struct Il2CppException;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API CCW
    {
    public:
        // CreateCCW returns upcasted Il2CppIManagedObjectHolder if the CCW is cachable!
        static Il2CppIUnknown* CreateCCW(Il2CppObject* obj);

        static inline Il2CppIUnknown* GetOrCreate(Il2CppObject* obj, const Il2CppGuid& iid)
        {
            return gc::GarbageCollector::GetOrCreateCCW(obj, iid);
        }

        static Il2CppObject* Unpack(Il2CppIUnknown* unknown);

        static il2cpp_hresult_t HandleInvalidIPropertyConversion(const char* fromType, const char* toType);
        static il2cpp_hresult_t HandleInvalidIPropertyConversion(Il2CppObject* value, const char* fromType, const char* toType);

        static il2cpp_hresult_t HandleInvalidIPropertyArrayConversion(const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index);
        static il2cpp_hresult_t HandleInvalidIPropertyArrayConversion(Il2CppObject* value, const char* fromArrayType, const char* fromElementType, const char* toElementType, il2cpp_array_size_t index);
    };
} /* namespace vm */
} /* namespace il2cpp */
