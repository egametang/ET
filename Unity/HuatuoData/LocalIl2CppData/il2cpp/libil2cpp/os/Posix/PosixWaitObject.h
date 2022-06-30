#pragma once

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <pthread.h>
#include <stdint.h>
#include <limits.h>
#include "utils/NonCopyable.h"
#include "os/WaitStatus.h"


#if (IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL)
int     pthread_cond_timedwait_relative_np(pthread_cond_t *cond, pthread_mutex_t *mutex, const struct timespec *spec);
#endif


namespace il2cpp
{
namespace os
{
    class ThreadImpl;

namespace posix
{
    const uint32_t kNoTimeout = UINT_MAX;


////TODO: generalize this so that it can be used with c++11 condition variables


/// Base class for all synchronization primitives when running on POSIX.
///
/// To support interruption and timeouts for all synchronization primitives (events, mutexes, and
/// semaphores) we implement these primitives ourselves instead of using their standard POSIX/Mach
/// system counterparts. See PosixWaitObject.cpp for an explanation why.
    class PosixWaitObject : public il2cpp::utils::NonCopyable
    {
    public:

        ~PosixWaitObject();

        WaitStatus Wait(bool interruptible = false);
        WaitStatus Wait(uint32_t ms, bool interruptible = false);

        /// Cause an ongoing blocking wait on this object to exit and check for pending APCs.
        /// If the object is not currently being waited on, will cause the next wait to exit
        /// right away and check for APCs. After APCs have been handled, the object will go
        /// back to waiting except if the wait timeout has expired.
        void InterruptWait();

        void* GetOSHandle();

        static void LockWaitObjectDeletion();
        static void UnlockWaitObjectDeletion();

    protected:

        enum Type
        {
            kMutex, /// All mutexes are recursive.
            kManualResetEvent,
            kAutoResetEvent,
            kSemaphore
        };

        PosixWaitObject(Type type);

        Type m_Type;

        /// Always have to acquire this mutex to touch m_Count.
        pthread_mutex_t m_Mutex;

        /// Signal other threads of changes to m_Count.
        pthread_cond_t m_Condition;

        /// "Release" count for the primitive. Means different things depending on the type of primitive
        /// but for all primitives, we wait until this is zero. Semaphores are the only primitive for which
        /// this can go past 1.
        uint32_t m_Count;

        /// Number of threads waiting on this object. This is used to prevent unnecessary signals
        /// on m_Condition.
        uint32_t m_WaitingThreadCount;

        bool HaveWaitingThreads() const { return (m_WaitingThreadCount != 0); }
    };

    struct AutoLockWaitObjectDeletion
    {
        AutoLockWaitObjectDeletion() { PosixWaitObject::LockWaitObjectDeletion(); }
        ~AutoLockWaitObjectDeletion() { PosixWaitObject::UnlockWaitObjectDeletion(); }
    };
}
}
}

#endif // IL2CPP_TARGET_POSIX
