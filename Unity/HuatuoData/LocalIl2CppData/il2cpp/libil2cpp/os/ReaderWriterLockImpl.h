#pragma once

#include "os/Mutex.h"

namespace il2cpp
{
namespace os
{
    class ReaderWriterLockImpl
    {
    public:
        void LockExclusive()
        {
            m_Mutex.Lock();
        }

        void LockShared()
        {
            m_Mutex.Lock();
        }

        void ReleaseExclusive()
        {
            m_Mutex.Unlock();
        }

        void ReleaseShared()
        {
            m_Mutex.Unlock();
        }

        FastMutex* GetOSHandle()
        {
            return &m_Mutex;
        }

    private:
        FastMutex m_Mutex;
    };
}
}
