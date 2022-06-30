#include "il2cpp-config.h"
#include "os/ReaderWriterLock.h"

#if IL2CPP_SUPPORT_THREADS

#if IL2CPP_THREADS_WIN32
#include "os/Win32/ReaderWriterLockImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/ReaderWriterLockImpl.h"
#else
#include "os/ReaderWriterLockImpl.h"
#endif

namespace il2cpp
{
namespace os
{
    ReaderWriterLock::ReaderWriterLock()
        : m_Impl(new ReaderWriterLockImpl())
    {
    }

    ReaderWriterLock::~ReaderWriterLock()
    {
        delete m_Impl;
    }

    void ReaderWriterLock::LockExclusive()
    {
        m_Impl->LockExclusive();
    }

    void ReaderWriterLock::LockShared()
    {
        m_Impl->LockShared();
    }

    void ReaderWriterLock::ReleaseExclusive()
    {
        m_Impl->ReleaseExclusive();
    }

    void ReaderWriterLock::ReleaseShared()
    {
        m_Impl->ReleaseShared();
    }

    ReaderWriterLockImpl* ReaderWriterLock::GetImpl()
    {
        return m_Impl;
    }
}
}

#else

#include <stddef.h>

namespace il2cpp
{
namespace os
{
    ReaderWriterLock::ReaderWriterLock()
    {
    }

    ReaderWriterLock::~ReaderWriterLock()
    {
    }

    void ReaderWriterLock::LockExclusive()
    {
    }

    void ReaderWriterLock::LockShared()
    {
    }

    void ReaderWriterLock::ReleaseExclusive()
    {
    }

    void ReaderWriterLock::ReleaseShared()
    {
    }

    ReaderWriterLockImpl* ReaderWriterLock::GetImpl()
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return NULL;
    }
}
}

#endif
