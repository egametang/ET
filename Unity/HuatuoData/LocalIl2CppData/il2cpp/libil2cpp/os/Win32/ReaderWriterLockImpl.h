#pragma once

#if IL2CPP_THREADS_WIN32

#include "WindowsHeaders.h"

namespace il2cpp
{
namespace os
{
    class ReaderWriterLockImpl
    {
    public:

        ReaderWriterLockImpl()
        {
            InitializeSRWLock(&m_Lock);
        }

        void LockExclusive()
        {
            AcquireSRWLockExclusive(&m_Lock);
        }

        void LockShared()
        {
            AcquireSRWLockShared(&m_Lock);
        }

        void ReleaseExclusive()
        {
            ReleaseSRWLockExclusive(&m_Lock);
        }

        void ReleaseShared()
        {
            ReleaseSRWLockShared(&m_Lock);
        }

        PSRWLOCK GetOSHandle()
        {
            return &m_Lock;
        }

    private:
        SRWLOCK  m_Lock;
    };
}
}

#endif
