#pragma once

#if !IL2CPP_THREADS_STD && IL2CPP_THREADS_PTHREAD && !RUNTIME_TINY

#include <pthread.h>
#include <vector>
#include <atomic>

#include "PosixWaitObject.h"
#include "os/ErrorCodes.h"
#include "os/Mutex.h"
#include "os/Event.h"
#include "os/Thread.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

#if defined(IL2CPP_ENABLE_PLATFORM_THREAD_AFFINTY)
struct cpu_set_t;
int pthread_attr_setaffinity_np(pthread_attr_t *attr, size_t cpusetsize, const cpu_set_t *cpuset);
#endif

#if defined(IL2CPP_ENABLE_PLATFORM_THREAD_RENAME)
int pthread_setname_np(pthread_t handle, const char *name);
#endif

#if !defined(IL2CPP_DEFAULT_STACK_SIZE)
#define IL2CPP_DEFAULT_STACK_SIZE ( 1 * 1024 * 1024)            // default .NET stacksize is 1mb
#endif

namespace il2cpp
{
namespace os
{
/// POSIX threads implementation. Supports APCs and interruptible waits.
    class ThreadImpl : public il2cpp::utils::NonCopyable
    {
    public:

        ThreadImpl();
        ~ThreadImpl();

        uint64_t Id();
        ErrorCode Run(Thread::StartFunc func, void* arg, int64_t affinityMask);
        void QueueUserAPC(Thread::APCFunc func, void* context);
        void SetName(const char* name);
        void SetPriority(ThreadPriority priority);
        ThreadPriority GetPriority();
        void SetStackSize(size_t newsize);
        static int GetMaxStackSize();

        /// Handle any pending APCs.
        /// NOTE: Can only be called on current thread.
        void CheckForUserAPCAndHandle();

        static void Sleep(uint32_t milliseconds, bool interruptible);
        static uint64_t CurrentThreadId();
        static ThreadImpl* GetCurrentThread();
        static ThreadImpl* CreateForCurrentThread();

        static bool YieldInternal();

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        static void SetNativeThreadCleanup(Thread::ThreadCleanupFunc cleanupFunction);
        static void RegisterCurrentThreadForCleanup(void* arg);
        static void UnregisterCurrentThreadForCleanup();
#endif

    private:

        friend class posix::PosixWaitObject; // SetWaitObject(), CheckForAPCAndHandle()

        std::atomic<pthread_t> m_Handle;

        /// The synchronization primitive that this thread is currently blocked on.
        ///
        /// NOTE: This field effectively turns these wait object into shared resources -- which makes deletion
        ///       a tricky affair. To avoid one thread trying to interrupt a wait while the other thread already
        ///       is in progress of deleting the wait object, we use a global mutex in PosixWaitObject.cpp that
        ///       must be locked by any thread trying to trigger an interrupt.
        posix::PosixWaitObject* m_CurrentWaitObject;

        /// Start data.
        Thread::StartFunc m_StartFunc;
        void* m_StartArg;

        /// List of APC requests for this thread.
        struct APCRequest
        {
            Thread::APCFunc callback;
            void* context;

            APCRequest(Thread::APCFunc callback, void* context) :
                callback(callback), context(context)
            {
            }
        };

        pthread_mutex_t m_PendingAPCsMutex;
        std::vector<APCRequest> m_PendingAPCs;

        size_t m_StackSize; // size of stack (can not be adjusted after thread creation)

        /// Set the synchronization object the thread is about to wait on.
        /// NOTE: This can only be called on the current thread.
        void SetWaitObject(posix::PosixWaitObject* waitObject);

        static void* ThreadStartWrapper(void* arg);
    };
}
}

#endif
