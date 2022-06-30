#pragma once

#include "il2cpp-config.h"
#include "utils/Il2CppError.h"

namespace il2cpp
{
namespace utils
{
    template<typename T>
    class Expected
    {
    public:
        Expected(const T& result) : m_Result(result) {}
        Expected(const Il2CppError& error) : m_Error(error) {}

        bool HasError() const
        {
            return m_Error.GetErrorCode() != NoError;
        }

        Il2CppError GetError() const
        {
            return m_Error;
        }

        T& Get()
        {
            IL2CPP_ASSERT(!HasError());
            return m_Result;
        }

        const T& Get() const
        {
            IL2CPP_ASSERT(!HasError());
            return m_Result;
        }

    private:
        T m_Result;
        const Il2CppError m_Error;

        Expected() {}
    };
} // namespace il2cpp
} // namespace utils
