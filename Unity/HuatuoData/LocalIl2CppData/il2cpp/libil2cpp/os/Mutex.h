#pragma once

#include "os/ErrorCodes.h"
#include "os/Handle.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

namespace il2cpp
{
namespace os
{
    class MutexImpl;
    class FastMutexImpl;

    class Mutex : public il2cpp::utils::NonCopyable
    {
    public:
        Mutex(bool initiallyOwned = false);
        ~Mutex();

        void Lock(bool interruptible = false);
        bool TryLock(uint32_t milliseconds = 0, bool interruptible = false);
        void Unlock();
        void* GetOSHandle();

    private:
        MutexImpl* m_Mutex;
    };

    struct AutoLock : public il2cpp::utils::NonCopyable
    {
        AutoLock(Mutex* mutex) : m_Mutex(mutex) { m_Mutex->Lock(); }
        ~AutoLock() { m_Mutex->Unlock(); }
    private:
        Mutex* m_Mutex;
    };

    class MutexHandle : public Handle
    {
    public:
        MutexHandle(Mutex* mutex) : m_Mutex(mutex) {}
        virtual ~MutexHandle() { delete m_Mutex; }
        virtual bool Wait() { m_Mutex->Lock(true); return true; }
        virtual bool Wait(uint32_t ms) { return m_Mutex->TryLock(ms, true); }
        virtual WaitStatus Wait(bool interruptible) { m_Mutex->Lock(interruptible); return kWaitStatusSuccess; }
        virtual WaitStatus Wait(uint32_t ms, bool interruptible) { return m_Mutex->TryLock(ms, interruptible) ? kWaitStatusSuccess : kWaitStatusFailure; }
        virtual void Signal() { m_Mutex->Unlock(); }
        virtual void* GetOSHandle() { return m_Mutex->GetOSHandle(); }
        Mutex* Get() { return m_Mutex; }

    private:
        Mutex* m_Mutex;
    };


/// Lightweight mutex that has no support for interruption or timed waits. Meant for
/// internal use only.
    class FastMutex
    {
    public:
        FastMutex();
        ~FastMutex();

        void Lock();
        void Unlock();

        FastMutexImpl* GetImpl();

    private:
        FastMutexImpl* m_Impl;
    };

    struct FastAutoLockOld : public il2cpp::utils::NonCopyable
    {
        FastAutoLockOld(FastMutex* mutex)
            : m_Mutex(mutex)
        {
            m_Mutex->Lock();
        }

        ~FastAutoLockOld()
        {
            m_Mutex->Unlock();
        }

    private:
        FastMutex* m_Mutex;
    };

    struct FastAutoLock : public il2cpp::utils::NonCopyable
    {
        FastAutoLock(baselib::ReentrantLock* mutex)
            : m_Mutex(mutex)
        {
            m_Mutex->Acquire();
        }

        ~FastAutoLock()
        {
            m_Mutex->Release();
        }

    private:
        baselib::ReentrantLock* m_Mutex;
    };

    struct FastAutoUnlock : public il2cpp::utils::NonCopyable
    {
        FastAutoUnlock(baselib::ReentrantLock* mutex)
            : m_Mutex(mutex)
        {
            m_Mutex->Release();
        }

        ~FastAutoUnlock()
        {
            m_Mutex->Acquire();
        }

    private:
        baselib::ReentrantLock* m_Mutex;
    };
}
}
