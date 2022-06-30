#include "os/c-api/il2cpp-config-platforms.h"
#include "os/Thread.h"

#if IL2CPP_SUPPORT_THREADS

#include "os/Mutex.h"
#include "os/ThreadLocalValue.h"
#if IL2CPP_THREADS_STD
#include "os/Std/ThreadImpl.h"
#elif IL2CPP_TARGET_WINDOWS
#include "os/Win32/ThreadImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/ThreadImpl.h"
#else
#include "os/ThreadImpl.h"
#endif

#include "utils/dynamic_array.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

#include <limits>

namespace il2cpp
{
namespace os
{
/// TLS variable referring to current thread.
    static ThreadLocalValue s_CurrentThread;

    // TLS variable referring to whether this thread is currently executing Thread::Shutdown
    // It is thread local for thread safety
    static ThreadLocalValue s_IsCleaningUpThreads;

    static baselib::ReentrantLock s_AliveThreadsMutex;
    static il2cpp::utils::dynamic_array<Thread*> s_AliveThreads;

    int64_t Thread::s_DefaultAffinityMask = kThreadAffinityAll;

    static bool GetIsCleaningUpThreads()
    {
        void* value = NULL;
        s_IsCleaningUpThreads.GetValue(&value);
        return reinterpret_cast<intptr_t>(value) != 0;
    }

    static void SetIsCleaningUpThreads(bool value)
    {
        s_IsCleaningUpThreads.SetValue(reinterpret_cast<void*>(static_cast<intptr_t>(value)));
    }

    Thread::Thread()
        : m_Thread(new ThreadImpl())
        , m_State(kThreadCreated)
        , m_ThreadExitedEvent(true) // Manual reset event
        , m_CleanupFunc(NULL)
        , m_CleanupFuncArg(NULL)
    {
        FastAutoLock lock(&s_AliveThreadsMutex);
        s_AliveThreads.push_back(this);
    }

    Thread::Thread(ThreadImpl* thread)
        : m_Thread(thread)
        , m_State(kThreadRunning)
        , m_ThreadExitedEvent(true) // Manual reset event
        , m_CleanupFunc(NULL)
        , m_CleanupFuncArg(NULL)
    {
        FastAutoLock lock(&s_AliveThreadsMutex);
        s_AliveThreads.push_back(this);
    }

    Thread::~Thread()
    {
        delete m_Thread;

        if (!GetIsCleaningUpThreads())
        {
            FastAutoLock lock(&s_AliveThreadsMutex);
            size_t count = s_AliveThreads.size();
            for (size_t i = 0; i < count; i++)
            {
                if (s_AliveThreads[i] == this)
                {
                    s_AliveThreads.erase_swap_back(&s_AliveThreads[i]);
                    break;
                }
            }
        }
    }

    void Thread::Init()
    {
        Thread* thread = GetOrCreateCurrentThread();
        if (thread->GetApartment() == kApartmentStateUnknown)
            thread->SetApartment(kApartmentStateInMTA);
    }

    void Thread::Shutdown()
    {
        Thread* currentThread = GetCurrentThread();
        currentThread->SetApartment(kApartmentStateUnknown);

        SetIsCleaningUpThreads(true);

        FastAutoLock lock(&s_AliveThreadsMutex);
        size_t count = s_AliveThreads.size();
        for (size_t i = 0; i < count; i++)
        {
            // If this is not the current thread, wait a bit for it to exit. This will avoid an
            // infinite wait on shutdown, but it should give the thread enough time to complete its
            // use of the os::Thread object before we delete it. Note that we don't call Join here,
            // as we want to explicitly do a non-interruptable wait because we are pretty late in
            // the shutdown process. The VM thread code should have already caused any running
            // threads to get a thread abort exception, meaning that any running OS threads will
            // be exiting soon, with no need to check for APCs.
            if (s_AliveThreads[i] != currentThread)
            {
                s_AliveThreads[i]->m_ThreadExitedEvent.Wait(10, false);
                delete s_AliveThreads[i];
            }
        }

        // Wait to delete the current thread last, as waiting on an event may need to access the current thread
        delete currentThread;

        s_AliveThreads.clear();

        SetIsCleaningUpThreads(false);
#if IL2CPP_ENABLE_RELOAD
        s_CurrentThread.SetValue(NULL);
#endif
    }

    Thread::ThreadId Thread::Id()
    {
        return m_Thread->Id();
    }

    void Thread::SetName(const char* name)
    {
        m_Thread->SetName(name);
    }

    void Thread::SetPriority(ThreadPriority priority)
    {
        m_Thread->SetPriority(priority);
    }

    ThreadPriority Thread::GetPriority()
    {
        return m_Thread->GetPriority();
    }

    void Thread::SetStackSize(size_t stackSize)
    {
        m_Thread->SetStackSize(stackSize);
    }

    int Thread::GetMaxStackSize()
    {
        return ThreadImpl::GetMaxStackSize();
    }

    struct StartData
    {
        Thread* thread;
        Thread::StartFunc startFunction;
        void* startFunctionArgument;
    };

/// Wrapper for the user's thread start function. Sets s_CurrentThread.
    void Thread::RunWrapper(void* arg)
    {
        StartData* data = reinterpret_cast<StartData*>(arg);

        // Store thread reference.
        Thread* thread = data->thread;

        const ApartmentState apartment = thread->GetExplicitApartment();
        if (apartment != kApartmentStateUnknown)
        {
            thread->SetExplicitApartment(kApartmentStateUnknown);
            thread->SetApartment(apartment);
        }

        s_CurrentThread.SetValue(thread);

        // Get rid of StartData.
        StartFunc startFunction = data->startFunction;
        void* startFunctionArgument = data->startFunctionArgument;
        delete data;

        // Make sure thread exit event is not signaled.
        thread->m_ThreadExitedEvent.Reset();

        // Run user thread start function.
        thread->m_State = kThreadRunning;
        startFunction(startFunctionArgument);
        thread->m_State = kThreadExited;

        thread->SetApartment(kApartmentStateUnknown);

        CleanupFunc cleanupFunc = thread->m_CleanupFunc;
        void* cleanupFuncArg = thread->m_CleanupFuncArg;

        // Signal that we've finished execution.
        thread->m_ThreadExitedEvent.Set();

        if (cleanupFunc)
            cleanupFunc(cleanupFuncArg);
    }

    ErrorCode Thread::Run(StartFunc func, void* arg)
    {
        IL2CPP_ASSERT(m_State == kThreadCreated || m_State == kThreadExited);

        StartData* startData = new StartData;
        startData->startFunction = func;
        startData->startFunctionArgument = arg;
        startData->thread = this;

        return m_Thread->Run(RunWrapper, startData, s_DefaultAffinityMask);
    }

    WaitStatus Thread::Join()
    {
        IL2CPP_ASSERT(this != GetCurrentThread() && "Trying to join the current thread will deadlock");
        return Join(std::numeric_limits<uint32_t>::max());
    }

    WaitStatus Thread::Join(uint32_t ms)
    {
        // Wait for thread exit event.
        if (m_ThreadExitedEvent.Wait(ms, true) != kWaitStatusSuccess)
            return kWaitStatusFailure;

        return kWaitStatusSuccess;
    }

    void Thread::QueueUserAPC(APCFunc func, void* context)
    {
        m_Thread->QueueUserAPC(func, context);
    }

    ApartmentState Thread::GetApartment()
    {
#if IL2CPP_THREAD_IMPL_HAS_COM_APARTMENTS
        return m_Thread->GetApartment();
#else
        return kApartmentStateUnknown;
#endif
    }

    ApartmentState Thread::GetExplicitApartment()
    {
#if IL2CPP_THREAD_IMPL_HAS_COM_APARTMENTS
        return m_Thread->GetExplicitApartment();
#else
        return kApartmentStateUnknown;
#endif
    }

    ApartmentState Thread::SetApartment(ApartmentState state)
    {
#if IL2CPP_THREAD_IMPL_HAS_COM_APARTMENTS
        return m_Thread->SetApartment(state);
#else
        NO_UNUSED_WARNING(state);
        return GetApartment();
#endif
    }

    void Thread::SetExplicitApartment(ApartmentState state)
    {
#if IL2CPP_THREAD_IMPL_HAS_COM_APARTMENTS
        m_Thread->SetExplicitApartment(state);
#else
        NO_UNUSED_WARNING(state);
#endif
    }

    void Thread::Sleep(uint32_t milliseconds, bool interruptible)
    {
        ThreadImpl::Sleep(milliseconds, interruptible);
    }

    size_t Thread::CurrentThreadId()
    {
        return ThreadImpl::CurrentThreadId();
    }

    Thread* Thread::GetCurrentThread()
    {
        void* value;
        s_CurrentThread.GetValue(&value);
        IL2CPP_ASSERT(value != NULL);
        return reinterpret_cast<Thread*>(value);
    }

    bool Thread::HasCurrentThread()
    {
        void* value;
        s_CurrentThread.GetValue(&value);
        return value != NULL;
    }

    Thread* Thread::GetOrCreateCurrentThread()
    {
        Thread* thread = NULL;
        s_CurrentThread.GetValue(reinterpret_cast<void**>(&thread));
        if (thread)
            return thread;

        // The os::Thread object is deallocated in the InternalThread::Thread_free_internal icall, which
        // is called from the managed thread finalizer.
        thread = new Thread(ThreadImpl::CreateForCurrentThread());
        s_CurrentThread.SetValue(thread);

        return thread;
    }

    void Thread::DetachCurrentThread()
    {
        // PTHREAD cleanup isn't deterministic: it could be that our thread local variables get cleaned up before thread clean up routine runs
#if IL2CPP_DEBUG && !IL2CPP_THREADS_PTHREAD
        void* value;
        s_CurrentThread.GetValue(&value);
        IL2CPP_ASSERT(value != NULL);
#endif

        s_CurrentThread.SetValue(NULL);
    }

    bool Thread::YieldInternal()
    {
        return ThreadImpl::YieldInternal();
    }

    void Thread::SetDefaultAffinityMask(int64_t affinityMask)
    {
        s_DefaultAffinityMask = affinityMask;
    }

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP

    void Thread::SetNativeThreadCleanup(ThreadCleanupFunc cleanupFunction)
    {
        ThreadImpl::SetNativeThreadCleanup(cleanupFunction);
    }

    void Thread::RegisterCurrentThreadForCleanup(void* arg)
    {
        ThreadImpl::RegisterCurrentThreadForCleanup(arg);
    }

    void Thread::UnregisterCurrentThreadForCleanup()
    {
        ThreadImpl::UnregisterCurrentThreadForCleanup();
    }

    void Thread::SignalExited()
    {
        m_ThreadExitedEvent.Set();
    }

#endif
}
}

#else

#include <limits.h>

namespace il2cpp
{
namespace os
{
    int64_t Thread::s_DefaultAffinityMask = -1;

    Thread::Thread()
    {
    }

    Thread::~Thread()
    {
    }

    void Thread::Init()
    {
    }

    void Thread::Shutdown()
    {
    }

    Thread::ThreadId Thread::Id()
    {
        return 0;
    }

    void Thread::SetName(const char* name)
    {
    }

    void Thread::SetPriority(ThreadPriority priority)
    {
    }

    ThreadPriority Thread::GetPriority()
    {
        return kThreadPriorityLowest;
    }

    void Thread::SetStackSize(size_t stackSize)
    {
    }

    int Thread::GetMaxStackSize()
    {
        return INT_MAX;
    }

    void Thread::RunWrapper(void* arg)
    {
    }

    ErrorCode Thread::Run(StartFunc func, void* arg)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return kErrorCodeSuccess;
    }

    WaitStatus Thread::Join()
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return kWaitStatusSuccess;
    }

    WaitStatus Thread::Join(uint32_t ms)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return kWaitStatusSuccess;
    }

    void Thread::QueueUserAPC(APCFunc func, void* context)
    {
    }

    ApartmentState Thread::GetApartment()
    {
        return kApartmentStateUnknown;
    }

    ApartmentState Thread::GetExplicitApartment()
    {
        return kApartmentStateUnknown;
    }

    ApartmentState Thread::SetApartment(ApartmentState state)
    {
        return kApartmentStateUnknown;
    }

    void Thread::SetExplicitApartment(ApartmentState state)
    {
    }

    void Thread::Sleep(uint32_t milliseconds, bool interruptible)
    {
    }

    size_t Thread::CurrentThreadId()
    {
        return 0;
    }

    Thread* Thread::GetCurrentThread()
    {
        return NULL;
    }

    Thread* Thread::GetOrCreateCurrentThread()
    {
        return NULL;
    }

    void Thread::DetachCurrentThread()
    {
    }

    bool Thread::YieldInternal()
    {
        return false;
    }

    void Thread::SetDefaultAffinityMask(int64_t affinityMask)
    {
        s_DefaultAffinityMask = affinityMask;
    }

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP

    void Thread::SetNativeThreadCleanup(ThreadCleanupFunc cleanupFunction)
    {
    }

    void Thread::RegisterCurrentThreadForCleanup(void* arg)
    {
    }

    void Thread::UnregisterCurrentThreadForCleanup()
    {
    }

    void Thread::SignalExited()
    {
    }

#endif
}
}

#endif
