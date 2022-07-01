#pragma once

#include "os/ErrorCodes.h"

#if IL2CPP_SUPPORT_THREADS

#if IL2CPP_THREADS_WIN32
#include "os/Win32/ThreadLocalValueImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/ThreadLocalValueImpl.h"
#else
#include "os/ThreadLocalValueImpl.h"
#endif

#endif

namespace il2cpp
{
namespace os
{
    class ThreadLocalValueImpl;

    class ThreadLocalValue
    {
#if IL2CPP_SUPPORT_THREADS
    public:
        inline ThreadLocalValue()
            : m_ThreadLocalValue(new ThreadLocalValueImpl())
        {
        }

        inline ~ThreadLocalValue()
        {
            delete m_ThreadLocalValue;
        }

        inline ErrorCode SetValue(void* value)
        {
            return m_ThreadLocalValue->SetValue(value);
        }

        inline ErrorCode GetValue(void** value)
        {
            return m_ThreadLocalValue->GetValue(value);
        }

    private:
        ThreadLocalValueImpl * m_ThreadLocalValue;
#else
    public:
        inline ThreadLocalValue()
        {
        }

        inline ~ThreadLocalValue()
        {
        }

        inline ErrorCode SetValue(void* value)
        {
            m_ThreadLocalValue = value;
            return kErrorCodeSuccess;
        }

        inline ErrorCode GetValue(void** value)
        {
            *value = m_ThreadLocalValue;
            return kErrorCodeSuccess;
        }

    private:
        void* m_ThreadLocalValue;
#endif
    };
}
}
