#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/COM.h"
#include "os/c-api/COM-c-api.h"

extern "C"
{
    il2cpp_hresult_t UnityPalCOMCreateInstance(const UnityPalIl2CppGuid& clsid, UnityPalIl2CppIUnknown** object)
    {
        return il2cpp::os::COM::CreateInstance(clsid, object);
    }

    il2cpp_hresult_t UnityPalCOMCreateFreeThreadedMarshaler(UnityPalIl2CppIUnknown* outer, UnityPalIl2CppIUnknown** marshal)
    {
        return il2cpp::os::COM::CreateFreeThreadedMarshaler(outer, marshal);
    }

    void UnityPalCOMVariantInit(UnityPalIl2CppVariant* variant)
    {
        return il2cpp::os::COM::VariantInit(variant);
    }

    il2cpp_hresult_t UnityPalCOMVariantClear(UnityPalIl2CppVariant* variant)
    {
        return il2cpp::os::COM::VariantClear(variant);
    }

    UnityPalIl2CppSafeArray* UnityPalCOMSafeArrayCreate(uint16_t type, uint32_t dimension_count, UnityPalIl2CppSafeArrayBound* bounds)
    {
        return il2cpp::os::COM::SafeArrayCreate(type, dimension_count, bounds);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayDestroy(UnityPalIl2CppSafeArray* safeArray)
    {
        return il2cpp::os::COM::SafeArrayDestroy(safeArray);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayAccessData(UnityPalIl2CppSafeArray* safeArray, void** data)
    {
        return il2cpp::os::COM::SafeArrayAccessData(safeArray, data);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayUnaccessData(UnityPalIl2CppSafeArray* safeArray)
    {
        return il2cpp::os::COM::SafeArrayUnaccessData(safeArray);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayGetVartype(UnityPalIl2CppSafeArray* safeArray, uint16_t* type)
    {
        return il2cpp::os::COM::SafeArrayGetVartype(safeArray, type);
    }

    uint32_t UnityPalCOMSafeArrayGetDim(UnityPalIl2CppSafeArray* safeArray)
    {
        return il2cpp::os::COM::SafeArrayGetDim(safeArray);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayGetLBound(UnityPalIl2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        return il2cpp::os::COM::SafeArrayGetLBound(safeArray, dimention, bound);
    }

    il2cpp_hresult_t UnityPalCOMSafeArrayGetUBound(UnityPalIl2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        return il2cpp::os::COM::SafeArrayGetUBound(safeArray, dimention, bound);
    }
}

#endif
