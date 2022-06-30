#pragma once
#include "il2cpp-config.h"

#include <string>

namespace il2cpp
{
namespace utils
{
    enum Il2CppErrorCode
    {
        NoError,
        NotSupported,
        ComError,
        UnauthorizedAccess,
    };

    class Il2CppError
    {
    public:
        Il2CppError();
        Il2CppError(Il2CppErrorCode errorCode, const char* message);
        Il2CppError(Il2CppErrorCode errorCode, il2cpp_hresult_t hr);
        Il2CppErrorCode GetErrorCode() const;
        std::string GetErrorMessage() const;
        il2cpp_hresult_t GetHr() const;
        bool GetDefaultToCOMException() const;

    private:
        const Il2CppErrorCode m_ErrorCode;
        const std::string m_Message;
        const il2cpp_hresult_t m_Hr;
    };
} // namespace utils
} // namespace error
