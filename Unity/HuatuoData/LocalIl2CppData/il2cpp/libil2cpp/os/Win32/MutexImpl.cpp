#include "il2cpp-config.h"

#if IL2CPP_THREADS_WIN32

#include "MutexImpl.h"
#include "WindowsHelpers.h"

// Can't use critical sections as they don't allow for interruption by APCs.

namespace il2cpp
{
namespace os
{
    MutexImpl::MutexImpl()
    {
#if IL2CPP_THREADS_ALL_ACCESS
        m_MutexHandle = ::CreateMutexEx(NULL, NULL, 0, MUTEX_ALL_ACCESS);
#else
        m_MutexHandle = ::CreateMutex(NULL, FALSE, NULL);
#endif
        IL2CPP_ASSERT(m_MutexHandle);
    }

    MutexImpl::~MutexImpl()
    {
        IL2CPP_ASSERT(m_MutexHandle);
        ::CloseHandle(m_MutexHandle);
    }

    void MutexImpl::Lock(bool interruptible)
    {
        TryLock(INFINITE, interruptible);
    }

    bool MutexImpl::TryLock(uint32_t milliseconds, bool interruptible)
    {
        return (il2cpp::os::win::WaitForSingleObjectAndAccountForAPCs(m_MutexHandle, milliseconds, interruptible) == kWaitStatusSuccess);
    }

    void MutexImpl::Unlock()
    {
        ReleaseMutex(m_MutexHandle);
    }

    void* MutexImpl::GetOSHandle()
    {
        return (void*)m_MutexHandle;
    }
}
}

#endif
