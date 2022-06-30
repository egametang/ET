#pragma once

namespace il2cpp
{
namespace os
{
    class MarshalStringAlloc
    {
    public:
        static il2cpp_hresult_t AllocateBStringLength(const Il2CppChar* text, int32_t length, Il2CppChar** bstr);
        static il2cpp_hresult_t GetBStringLength(const Il2CppChar* bstr, int32_t* length);
        static il2cpp_hresult_t FreeBString(Il2CppChar* bstr);
    };
} /* namespace os */
} /* namespace il2cpp*/
