#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <cerrno>
#include <ctime>

#include "PosixWaitObject.h"
#include "PosixHelpers.h"
#include "ThreadImpl.h"
#include "os/Time.h"


// Notes:
// - Situation
//   - None of the pthread APIs are interruptible (they all explicitly forbid returning EINTR).
//   - We cannot do any non-local transfers of control from signal handlers safely (C++ exceptions
//     or longjmp). Thus we cannot use signals to inject interruptions into a thread.
//   - Very few of the system APIs we have available support timeouts (at least not on all platforms).
// - Ergo: we need to roll our own synchronization primitives based on pthread condition variables
//     (they support timeouts and have the functionality needed to model the other primitives).
// - BUT: the condition variables still involve mutexes which we cannot lock in way that allows
//     interruptions. This means that there will be time windows where threads will wait and just
//     block and not allow interruption.


namespace il2cpp
{
namespace os
{
namespace posix
{
    static pthread_mutex_t s_WaitObjectDeletionLock = PTHREAD_MUTEX_INITIALIZER;

    PosixWaitObject::PosixWaitObject(Type type)
        : m_Type(type)
        , m_Count(0)
        , m_WaitingThreadCount(0)
    {
        pthread_mutex_init(&m_Mutex, NULL);
#if !IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL
        pthread_condattr_t attr;
        int result = pthread_condattr_init(&attr);
        pthread_condattr_setclock(&attr, CLOCK_MONOTONIC);
        pthread_cond_init(&m_Condition, &attr);
        pthread_condattr_destroy(&attr);
#else
        // On OSX and iOS we can use pthread_cond_timedwait_relative_np.
        pthread_cond_init(&m_Condition, NULL);
#endif
    }

    PosixWaitObject::~PosixWaitObject()
    {
        // Make sure it's okay to delete wait objects.
        AutoLockWaitObjectDeletion lockDeletion;

        pthread_mutex_destroy(&m_Mutex);
        pthread_cond_destroy(&m_Condition);
    }

    void PosixWaitObject::LockWaitObjectDeletion()
    {
        pthread_mutex_lock(&s_WaitObjectDeletionLock);
    }

    void PosixWaitObject::UnlockWaitObjectDeletion()
    {
        pthread_mutex_unlock(&s_WaitObjectDeletionLock);
    }

    WaitStatus PosixWaitObject::Wait(bool interruptible)
    {
        return Wait(kNoTimeout, interruptible);
    }

    WaitStatus PosixWaitObject::Wait(uint32_t timeoutMS, bool interruptible)
    {
        // IMPORTANT: This function must be exception-safe! APCs may throw.

        ThreadImpl* currentThread = ThreadImpl::GetCurrentThread();

        // Do up-front check about pending APC except this is a zero-timeout
        // wait (i.e. a wait that is never supposed to block and thus go into
        // an interruptible state).
        if (interruptible && timeoutMS != 0)
            currentThread->CheckForUserAPCAndHandle();

        // Lock object. We release this mutex during waiting.
        posix::PosixAutoLock lock(&m_Mutex);

        // See if the object is in a state where we can acquire it right away.
        if (m_Count == 0)
        {
            // No, hasn't. If we're not supposed to wait, we're done.
            if (timeoutMS == 0)
                return kWaitStatusTimeout;

            try
            {
                // We should wait. Let the world know this thread is now waiting
                // on this object.
                if (interruptible)
                    currentThread->SetWaitObject(this);

                // Check APC queue again to avoid race condition.
                if (interruptible)
                    currentThread->CheckForUserAPCAndHandle();

                // Go into wait until we either have a release or timeout or otherwise fail.
                uint32_t remainingWaitTime = timeoutMS;
                WaitStatus waitStatus = kWaitStatusSuccess;
                while (m_Count == 0)
                {
                    if (timeoutMS == posix::kNoTimeout)
                    {
                        // Infinite wait. Can only be interrupted by APC.
                        ++m_WaitingThreadCount; // No synchronization necessary; we hold the mutex.
                        int status = pthread_cond_wait(&m_Condition, &m_Mutex);
                        --m_WaitingThreadCount;

                        if (status != 0)
                        {
                            waitStatus = kWaitStatusFailure;
                            break;
                        }
                    }
                    else
                    {
                        // Timed wait. Can be interrupted by APC or timeout.
                        const int64_t waitStartTime = Time::GetTicks100NanosecondsMonotonic();
                        timespec timeout = posix::MillisecondsToTimespec(remainingWaitTime);

#if !IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL
                        timespec waitStartTimeSpec = posix::Ticks100NanosecondsToTimespec(waitStartTime);
                        timeout.tv_sec += waitStartTimeSpec.tv_sec;
                        timeout.tv_nsec += waitStartTimeSpec.tv_nsec;
                        if (timeout.tv_nsec >= 1000000000)
                        {
                            timeout.tv_nsec -= 1000000000;
                            ++timeout.tv_sec;
                        }
#endif

                        ++m_WaitingThreadCount;
#if !IL2CPP_USE_POSIX_COND_TIMEDWAIT_REL
                        // For plain POSIX, do an absolute timed wait.
                        int status = pthread_cond_timedwait(&m_Condition, &m_Mutex, &timeout);
#else
                        // For OSX and iOS, do a relative timed wait.
                        int status = pthread_cond_timedwait_relative_np(&m_Condition, &m_Mutex, &timeout);
#endif
                        --m_WaitingThreadCount; ////TODO: make this atomic for when we fail to reacquire the mutex

                        if (status == ETIMEDOUT)
                        {
                            waitStatus = kWaitStatusTimeout;
                            break;
                        }
                        else if (status != 0)
                        {
                            waitStatus = kWaitStatusFailure;
                            break;
                        }

                        // Update time we have have left to wait.
                        const uint32_t waitTimeThisRound = (Time::GetTicks100NanosecondsMonotonic() - waitStartTime) / 10000;
                        if (waitTimeThisRound > remainingWaitTime)
                            remainingWaitTime = 0;
                        else
                            remainingWaitTime -= waitTimeThisRound;
                    }

                    // We've received a signal but it may be because of an APC and not because
                    // the semaphore got signaled. If so, handle the APC and go back to waiting.
                    if (interruptible)
                        currentThread->CheckForUserAPCAndHandle();
                }

                // We're done waiting so untie us from the current thread.
                // NOTE: A thread may have grabbed us and then got paused. If we return now and then our owner
                //       tries to delete us, we would pull the rug from under the other thread. This is prevented by
                //       having a central lock on wait object deletion which any thread trying to deal with wait
                //       objects from other threads has to acquire.
                if (interruptible)
                {
                    currentThread->SetWaitObject(NULL);

                    // Avoid race condition by checking APC queue again after unsetting wait object.
                    currentThread->CheckForUserAPCAndHandle();
                }

                // If we failed, bail out now.
                if (waitStatus != kWaitStatusSuccess)
                    return waitStatus;
            }
            catch (...)
            {
                if (interruptible)
                    currentThread->SetWaitObject(NULL);
                throw;
            }
        }

        // At this point, we should be in signaled state and have the lock on
        // the object.

        // Object has been released. Acquire it for this thread.
        IL2CPP_ASSERT(m_Count > 0);
        switch (m_Type)
        {
            case kManualResetEvent:
                // Nothing to do.
                break;

            case kMutex:
            case kAutoResetEvent:
                m_Count = 0;
                break;

            case kSemaphore:
                if (m_Count > 0) // Defensive.
                {
                    --m_Count;
                    if (m_Count > 0)
                    {
                        // There's more releases on the semaphore. Signal the next thread in line.
                        if (HaveWaitingThreads())
                            pthread_cond_signal(&m_Condition);
                    }
                }
                break;
        }

        return kWaitStatusSuccess;
    }

    void PosixWaitObject::InterruptWait()
    {
        pthread_cond_broadcast(&m_Condition);
    }

    void* PosixWaitObject::GetOSHandle()
    {
        IL2CPP_ASSERT(0 && "This function is not implemented and should not be called");
        return NULL;
    }
}
}
}

#endif // IL2CPP_TARGET_POSIX
