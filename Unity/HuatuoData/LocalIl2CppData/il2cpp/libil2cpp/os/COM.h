#pragma once

#include "il2cpp-config.h"
#include "il2cpp-windowsruntime-types.h"

namespace il2cpp
{
namespace os
{
    class COM
    {
    public:
        static il2cpp_hresult_t CreateInstance(const Il2CppGuid& clsid, Il2CppIUnknown** object);
        static il2cpp_hresult_t CreateFreeThreadedMarshaler(Il2CppIUnknown* outer, Il2CppIUnknown** marshal);

        // variant

        static void VariantInit(Il2CppVariant* variant);
        static il2cpp_hresult_t VariantClear(Il2CppVariant* variant);

        // safe array

        static Il2CppSafeArray* SafeArrayCreate(uint16_t type, uint32_t dimension_count, Il2CppSafeArrayBound* bounds);
        static il2cpp_hresult_t SafeArrayDestroy(Il2CppSafeArray* safeArray);
        static il2cpp_hresult_t SafeArrayAccessData(Il2CppSafeArray* safeArray, void** data);
        static il2cpp_hresult_t SafeArrayUnaccessData(Il2CppSafeArray* safeArray);
        static il2cpp_hresult_t SafeArrayGetVartype(Il2CppSafeArray* safeArray, uint16_t* type);
        static uint32_t SafeArrayGetDim(Il2CppSafeArray* safeArray);
        static il2cpp_hresult_t SafeArrayGetLBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound);
        static il2cpp_hresult_t SafeArrayGetUBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound);
    };
} /* namespace os */
} /* namespace il2cpp*/
#pragma once
