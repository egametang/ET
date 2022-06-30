#pragma once

#if !IL2CPP_THREADS_STD && IL2CPP_THREADS_WIN32

#include "os/ErrorCodes.h"
#include "os/Thread.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

#include "WindowsHeaders.h"

#define IL2CPP_DEFAULT_STACK_SIZE ( 1 * 1024 * 1024)            // default .NET stacksize is 1mb

namespace il2cpp
{
namespace os
{
    class ThreadImpl : public il2cpp::utils::NonCopyable
    {
    public:
        ThreadImpl();
        ~ThreadImpl();

        size_t Id();
        ErrorCode Run(Thread::StartFunc func, void* arg, int64_t affinityMask);
        void SetName(const char* name);
        void SetPriority(ThreadPriority priority);
        ThreadPriority GetPriority();

        void SetStackSize(size_t newsize)
        {
            // only makes sense if it's called BEFORE the thread has been created
            IL2CPP_ASSERT(m_ThreadHandle == NULL);
            // if newsize is zero we use the per-platform default value for size of stack
            if (newsize == 0)
            {
                newsize = IL2CPP_DEFAULT_STACK_SIZE;
            }
            m_StackSize = newsize;
        }

        static int GetMaxStackSize();

        void QueueUserAPC(Thread::APCFunc func, void* context);

        ApartmentState GetApartment();
        ApartmentState GetExplicitApartment();
        ApartmentState SetApartment(ApartmentState state);
        void SetExplicitApartment(ApartmentState state);

        static void Sleep(uint32_t ms, bool interruptible);
        static size_t CurrentThreadId();
        static ThreadImpl* CreateForCurrentThread();

        static bool YieldInternal();

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        static void SetNativeThreadCleanup(Thread::ThreadCleanupFunc cleanupFunction);
        static void RegisterCurrentThreadForCleanup(void* arg);
        static void UnregisterCurrentThreadForCleanup();
        static void OnCurrentThreadExiting();
#endif

    private:
        HANDLE m_ThreadHandle;
        volatile DWORD m_ThreadId;
        SIZE_T m_StackSize;
        ApartmentState m_ApartmentState;
        ThreadPriority m_Priority;
    };
}
}

#endif
