#pragma once

#if defined(__cplusplus)

#include "os/COM.h"

typedef Il2CppGuid UnityPalIl2CppGuid;
typedef Il2CppIUnknown UnityPalIl2CppIUnknown;
typedef Il2CppVariant UnityPalIl2CppVariant;
typedef Il2CppSafeArray UnityPalIl2CppSafeArray;
typedef Il2CppSafeArrayBound UnityPalIl2CppSafeArrayBound;

#else

typedef struct UnityPalIl2CppGuid UnityPalIl2CppGuid;
typedef struct UnityPalIl2CppIUnknown UnityPalIl2CppIUnknown;
typedef struct UnityPalIl2CppVariant UnityPalIl2CppVariant;
typedef struct UnityPalIl2CppSafeArray UnityPalIl2CppSafeArray;
typedef struct UnityPalIl2CppSafeArrayBound UnityPalIl2CppSafeArrayBound;

#endif

#if defined(__cplusplus)
extern "C"
{
#endif

il2cpp_hresult_t UnityPalCOMCreateInstance(const UnityPalIl2CppGuid& clsid, UnityPalIl2CppIUnknown** object);
il2cpp_hresult_t UnityPalCOMCreateFreeThreadedMarshaler(UnityPalIl2CppIUnknown* outer, UnityPalIl2CppIUnknown** marshal);

void UnityPalCOMVariantInit(UnityPalIl2CppVariant* variant);
il2cpp_hresult_t UnityPalCOMVariantClear(UnityPalIl2CppVariant* variant);

UnityPalIl2CppSafeArray* UnityPalCOMSafeArrayCreate(uint16_t type, uint32_t dimension_count, UnityPalIl2CppSafeArrayBound* bounds);
il2cpp_hresult_t UnityPalCOMSafeArrayDestroy(UnityPalIl2CppSafeArray* safeArray);
il2cpp_hresult_t UnityPalCOMSafeArrayAccessData(UnityPalIl2CppSafeArray* safeArray, void** data);
il2cpp_hresult_t UnityPalCOMSafeArrayUnaccessData(UnityPalIl2CppSafeArray* safeArray);
il2cpp_hresult_t UnityPalCOMSafeArrayGetVartype(UnityPalIl2CppSafeArray* safeArray, uint16_t* type);
uint32_t UnityPalCOMSafeArrayGetDim(UnityPalIl2CppSafeArray* safeArray);
il2cpp_hresult_t UnityPalCOMSafeArrayGetLBound(UnityPalIl2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound);
il2cpp_hresult_t UnityPalCOMSafeArrayGetUBound(UnityPalIl2CppSafeArray* safeArray, uint32_t dimention, int32_t* bound);

#if defined(__cplusplus)
}
#endif
