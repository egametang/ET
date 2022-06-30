#include "Il2CppError.h"

namespace il2cpp
{
namespace utils
{
    Il2CppError::Il2CppError() : m_ErrorCode(NoError), m_Hr(0)
    {
    }

    Il2CppError::Il2CppError(Il2CppErrorCode errorCode, const char* message) :
        m_ErrorCode(errorCode), m_Message(message), m_Hr(0)
    {
    }

    Il2CppError::Il2CppError(Il2CppErrorCode errorCode, il2cpp_hresult_t hr) :
        m_ErrorCode(errorCode), m_Hr(hr)
    {
    }

    Il2CppErrorCode Il2CppError::GetErrorCode() const
    {
        return m_ErrorCode;
    }

    std::string Il2CppError::GetErrorMessage() const
    {
        return m_Message;
    }

    il2cpp_hresult_t Il2CppError::GetHr() const
    {
        return m_Hr;
    }
} // namespace utils
} // namespace il2cpp
