#pragma once

#include <stdint.h>

namespace il2cpp
{
namespace utils
{
    template<typename T, int Size>
    class ExceptionSupportStack
    {
    public:
        ExceptionSupportStack() : m_count(0)
        {
        }

        void push(T value)
        {
            // This function is rather unsafe. We don't track the size of storage,
            // and assume the caller will not push more values than it has allocated.
            // This function should only be used from generated code, where
            // we control the calls to this function.
            IL2CPP_ASSERT(m_count < Size);
            m_Storage[m_count] = value;
            m_count++;
        }

        void pop()
        {
            IL2CPP_ASSERT(!empty());
            m_count--;
        }

        T top() const
        {
            IL2CPP_ASSERT(!empty());
            return m_Storage[m_count - 1];
        }

        bool empty() const
        {
            return m_count == 0;
        }

    private:
        T m_Storage[Size];
        int m_count;
    };
}
}
