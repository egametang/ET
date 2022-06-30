#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_COM
#include "os/MarshalStringAlloc.h"
#include "WindowsHeaders.h"

namespace il2cpp
{
namespace os
{
    il2cpp_hresult_t MarshalStringAlloc::AllocateBStringLength(const Il2CppChar* text, int32_t length, Il2CppChar** bstr)
    {
        IL2CPP_ASSERT(bstr);
        *bstr = ::SysAllocStringLen(text, length);
        return *bstr ? IL2CPP_S_OK : IL2CPP_E_OUTOFMEMORY;
    }

    il2cpp_hresult_t MarshalStringAlloc::GetBStringLength(const Il2CppChar* bstr, int32_t* length)
    {
        IL2CPP_ASSERT(length);
        *length = ::SysStringLen((BSTR)bstr);
        return IL2CPP_S_OK;
    }

    il2cpp_hresult_t MarshalStringAlloc::FreeBString(Il2CppChar* bstr)
    {
        ::SysFreeString((BSTR)bstr);
        return IL2CPP_S_OK;
    }
}
}
#endif
