#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_COM
#include "os/MarshalStringAlloc.h"

namespace il2cpp
{
namespace os
{
    il2cpp_hresult_t MarshalStringAlloc::AllocateBStringLength(const Il2CppChar* text, int32_t length, Il2CppChar** bstr)
    {
        NO_UNUSED_WARNING(text);
        NO_UNUSED_WARNING(length);
        IL2CPP_ASSERT(bstr);
        *bstr = NULL;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t MarshalStringAlloc::GetBStringLength(const Il2CppChar* bstr, int32_t* length)
    {
        NO_UNUSED_WARNING(bstr);
        IL2CPP_ASSERT(length);
        *length = 0;
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }

    il2cpp_hresult_t MarshalStringAlloc::FreeBString(Il2CppChar* bstr)
    {
        NO_UNUSED_WARNING(bstr);
        return IL2CPP_COR_E_PLATFORMNOTSUPPORTED;
    }
}
}
#endif
