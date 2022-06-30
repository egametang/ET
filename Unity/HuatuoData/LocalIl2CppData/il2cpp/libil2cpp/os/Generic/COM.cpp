#include "il2cpp-config.h"
#include "os/COM.h"

#if IL2CPP_USE_GENERIC_COM

namespace il2cpp
{
namespace os
{
    il2cpp_hresult_t COM::CreateInstance(const Il2CppGuid& clsid, Il2CppIUnknown** object)
    {
        NO_UNUSED_WARNING(clsid);
        IL2CPP_ASSERT(object);
        *object = NULL;
        return IL2CPP_REGDB_E_CLASSNOTREG;
    }

    il2cpp_hresult_t COM::CreateFreeThreadedMarshaler(Il2CppIUnknown* outer, Il2CppIUnknown** marshal)
    {
        NO_UNUSED_WARNING(outer);
        IL2CPP_ASSERT(marshal);
        *marshal = NULL;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }
}
}

#endif


#if IL2CPP_USE_GENERIC_COM_SAFEARRAYS

namespace il2cpp
{
namespace os
{
// variant

    void COM::VariantInit(Il2CppVariant* variant)
    {
        NO_UNUSED_WARNING(variant);
    }

    il2cpp_hresult_t COM::VariantClear(Il2CppVariant* variant)
    {
        NO_UNUSED_WARNING(variant);
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

// safe array

    Il2CppSafeArray* COM::SafeArrayCreate(uint16_t type, uint32_t dimension_count, Il2CppSafeArrayBound* bounds)
    {
        NO_UNUSED_WARNING(type);
        NO_UNUSED_WARNING(dimension_count);
        NO_UNUSED_WARNING(bounds);
        return NULL;
    }

    il2cpp_hresult_t COM::SafeArrayDestroy(Il2CppSafeArray* safeArray)
    {
        NO_UNUSED_WARNING(safeArray);
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t COM::SafeArrayAccessData(Il2CppSafeArray* safeArray, void** data)
    {
        NO_UNUSED_WARNING(safeArray);
        IL2CPP_ASSERT(data);
        *data = NULL;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t COM::SafeArrayUnaccessData(Il2CppSafeArray* safeArray)
    {
        NO_UNUSED_WARNING(safeArray);
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t COM::SafeArrayGetVartype(Il2CppSafeArray* safeArray, uint16_t* type)
    {
        NO_UNUSED_WARNING(safeArray);
        IL2CPP_ASSERT(type);
        *type = 0;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    uint32_t COM::SafeArrayGetDim(Il2CppSafeArray* safeArray)
    {
        NO_UNUSED_WARNING(safeArray);
        return 0;
    }

    il2cpp_hresult_t COM::SafeArrayGetLBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        NO_UNUSED_WARNING(safeArray);
        NO_UNUSED_WARNING(dimention);
        IL2CPP_ASSERT(bound);
        *bound = 0;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t COM::SafeArrayGetUBound(Il2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound)
    {
        NO_UNUSED_WARNING(safeArray);
        NO_UNUSED_WARNING(dimention);
        IL2CPP_ASSERT(bound);
        *bound = 0;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }
}
}
#endif
