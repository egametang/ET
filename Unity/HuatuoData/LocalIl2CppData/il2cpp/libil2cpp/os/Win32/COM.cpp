#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "os/COM.h"
#include "WindowsHeaders.h"

#if !IL2CPP_USE_GENERIC_COM

namespace il2cpp
{
namespace os
{
    il2cpp_hresult_t COM::CreateInstance(const Il2CppGuid& clsid, Il2CppIUnknown** object)
    {
        return ::CoCreateInstance(reinterpret_cast<REFCLSID>(clsid), NULL, CLSCTX_ALL, __uuidof(IUnknown), reinterpret_cast<LPVOID*>(object));
    }

    il2cpp_hresult_t COM::CreateFreeThreadedMarshaler(Il2CppIUnknown* outer, Il2CppIUnknown** marshal)
    {
        return ::CoCreateFreeThreadedMarshaler(reinterpret_cast<LPUNKNOWN>(outer), reinterpret_cast<LPUNKNOWN*>(marshal));
    }
}
}

#endif

#if !IL2CPP_USE_GENERIC_COM_SAFEARRAYS

namespace il2cpp
{
namespace os
{
// variant

    void COM::VariantInit(Il2CppVariant* variant)
    {
        ::VariantInit(reinterpret_cast<VARIANTARG*>(variant));
    }

    il2cpp_hresult_t COM::VariantClear(Il2CppVariant* variant)
    {
        return ::VariantClear(reinterpret_cast<VARIANTARG*>(variant));
    }

// safe array

    Il2CppSafeArray* COM::SafeArrayCreate(uint16_t type, uint32_t dimension_count, Il2CppSafeArrayBound* bounds)
    {
        return reinterpret_cast<Il2CppSafeArray*>(::SafeArrayCreate(type, dimension_count, reinterpret_cast<SAFEARRAYBOUND*>(bounds)));
    }

    il2cpp_hresult_t COM::SafeArrayDestroy(Il2CppSafeArray* safeArray)
    {
        return ::SafeArrayDestroy(reinterpret_cast<SAFEARRAY*>(safeArray));
    }

    il2cpp_hresult_t COM::SafeArrayAccessData(Il2CppSafeArray* safeArray, void** data)
    {
        return ::SafeArrayAccessData(reinterpret_cast<SAFEARRAY*>(safeArray), data);
    }

    il2cpp_hresult_t COM::SafeArrayUnaccessData(Il2CppSafeArray* safeArray)
    {
        return ::SafeArrayUnaccessData(reinterpret_cast<SAFEARRAY*>(safeArray));
    }

    il2cpp_hresult_t COM::SafeArrayGetVartype(Il2CppSafeArray* safeArray, uint16_t* type)
    {
        return ::SafeArrayGetVartype(reinterpret_cast<SAFEARRAY*>(safeArray), type);
    }

    uint32_t COM::SafeArrayGetDim(Il2CppSafeArray* safeArray)
    {
        return ::SafeArrayGetDim(reinterpret_cast<SAFEARRAY*>(safeArray));
    }

    il2cpp_hresult_t COM::SafeArrayGetLBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        return ::SafeArrayGetLBound(reinterpret_cast<SAFEARRAY*>(safeArray), dimention, reinterpret_cast<LONG*>(bound));
    }

    il2cpp_hresult_t COM::SafeArrayGetUBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        return ::SafeArrayGetUBound(reinterpret_cast<SAFEARRAY*>(safeArray), dimention, reinterpret_cast<LONG*>(bound));
    }
}
}
#endif
