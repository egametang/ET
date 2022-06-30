#pragma once
#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace os
{
    class ReaderWriterLockImpl;

    class ReaderWriterLock
    {
    public:
        ReaderWriterLock();
        ~ReaderWriterLock();

        void LockExclusive();
        void LockShared();
        void ReleaseExclusive();
        void ReleaseShared();

        ReaderWriterLockImpl* GetImpl();

    private:
        ReaderWriterLockImpl* m_Impl;
    };

    struct ReaderWriterAutoLock : public il2cpp::utils::NonCopyable
    {
        ReaderWriterAutoLock(ReaderWriterLock* lock, bool exclusive = false)
            : m_Lock(lock), m_Exclusive(exclusive)
        {
            if (m_Exclusive)
                m_Lock->LockExclusive();
            else
                m_Lock->LockShared();
        }

        ~ReaderWriterAutoLock()
        {
            if (m_Exclusive)
                m_Lock->ReleaseExclusive();
            else
                m_Lock->ReleaseShared();
        }

    private:
        ReaderWriterLock* m_Lock;
        bool m_Exclusive;
    };
}
}
