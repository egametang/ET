#pragma once

#include "il2cpp-config.h"

#if IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include <pthread.h>

namespace il2cpp
{
namespace os
{
    class ReaderWriterLockImpl
    {
    public:

        ReaderWriterLockImpl()
        {
            int result = pthread_rwlock_init(&m_Lock, NULL);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        ~ReaderWriterLockImpl()
        {
            int result = pthread_rwlock_destroy(&m_Lock);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        void LockExclusive()
        {
            int result = pthread_rwlock_wrlock(&m_Lock);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        void LockShared()
        {
            int result = pthread_rwlock_rdlock(&m_Lock);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        void ReleaseExclusive()
        {
            int result = pthread_rwlock_unlock(&m_Lock);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        void ReleaseShared()
        {
            int result = pthread_rwlock_unlock(&m_Lock);
            NO_UNUSED_WARNING(result);
            IL2CPP_ASSERT(result == 0);
        }

        pthread_rwlock_t* GetOSHandle()
        {
            return &m_Lock;
        }

    private:
        pthread_rwlock_t  m_Lock;
    };
}
}

#endif
