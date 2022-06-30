#include "il2cpp-config.h"
#include "os/Mutex.h"

#if IL2CPP_SUPPORT_THREADS

#include "os/Atomic.h"
#if IL2CPP_THREADS_WIN32
#include "os/Win32/MutexImpl.h"
#elif IL2CPP_TARGET_PSP2
#include "os/PSP2/MutexImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/MutexImpl.h"
#else
#include "os/MutexImpl.h"
#endif

namespace il2cpp
{
namespace os
{
    Mutex::Mutex(bool initiallyOwned)
        : m_Mutex(new MutexImpl())
    {
        if (initiallyOwned)
            Lock();
    }

    Mutex::~Mutex()
    {
        delete m_Mutex;
    }

    void Mutex::Lock(bool interruptible)
    {
        m_Mutex->Lock(interruptible);
    }

    bool Mutex::TryLock(uint32_t milliseconds, bool interruptible)
    {
        return m_Mutex->TryLock(milliseconds, interruptible);
    }

    void Mutex::Unlock()
    {
        m_Mutex->Unlock();
    }

    void* Mutex::GetOSHandle()
    {
        return m_Mutex->GetOSHandle();
    }

    FastMutex::FastMutex()
        : m_Impl(new FastMutexImpl())
    {
    }

    FastMutex::~FastMutex()
    {
        delete m_Impl;
    }

    void FastMutex::Lock()
    {
        m_Impl->Lock();
    }

    void FastMutex::Unlock()
    {
        m_Impl->Unlock();
    }

    FastMutexImpl* FastMutex::GetImpl()
    {
        return m_Impl;
    }
}
}

#else

namespace il2cpp
{
namespace os
{
    Mutex::Mutex(bool initiallyOwned)
    {
    }

    Mutex::~Mutex()
    {
    }

    void Mutex::Lock(bool interruptible)
    {
    }

    bool Mutex::TryLock(uint32_t milliseconds, bool interruptible)
    {
        return true;
    }

    void Mutex::Unlock()
    {
    }

    void* Mutex::GetOSHandle()
    {
        return NULL;
    }

    FastMutex::FastMutex()
    {
    }

    FastMutex::~FastMutex()
    {
    }

    void FastMutex::Lock()
    {
    }

    void FastMutex::Unlock()
    {
    }

    FastMutexImpl* FastMutex::GetImpl()
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return NULL;
    }
}
}

#endif
