#include "il2cpp-config.h"
#include "os/Mutex.h"
#include "os/Thread.h"
#include "os/ThreadLocalValue.h"
#include "os/Time.h"
#include "os/Semaphore.h"
#include "vm/Domain.h"
#include "vm/Exception.h"
#include "vm/Object.h"
#include "vm/Profiler.h"
#include "vm/Runtime.h"
#include "vm/StackTrace.h"
#include "vm/Thread.h"
#include "vm/String.h"
#include "gc/Allocator.h"
#include "gc/GarbageCollector.h"
#include "gc/GCHandle.h"
#include "gc/WriteBarrier.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "vm-utils/Debugger.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-object-internals.h"
#include <algorithm>

#include "Baselib.h"
#include "Cpp/Atomic.h"
#include "Cpp/ReentrantLock.h"

#if IL2CPP_MONO_DEBUGGER

extern "C" {
#include <mono/metadata/profiler-private.h>
}
#endif

namespace il2cpp
{
namespace vm
{
    Il2CppThread* Thread::s_MainThread = NULL;

    typedef std::vector<Il2CppThread*, il2cpp::gc::Allocator<Il2CppThread*> > GCTrackedThreadVector;

    // we need to allocate this ourselves so the CRT does not initialize it and try to allocate GC memory on startup before the GC is initialized
    static GCTrackedThreadVector* s_AttachedThreads;

    static bool s_BlockNewThreads = false;

#define AUTO_LOCK_THREADS() \
    il2cpp::os::FastAutoLock lock(&s_ThreadMutex)
    static baselib::ReentrantLock s_ThreadMutex;

    static std::vector<int32_t> s_ThreadStaticSizes;

    static il2cpp::os::ThreadLocalValue s_CurrentThread;

    static baselib::atomic<int32_t> s_NextManagedThreadId = {0};

    static void
    set_wbarrier_for_attached_threads()
    {
        gc::GarbageCollector::SetWriteBarrier((void**)s_AttachedThreads->data(), sizeof(Il2CppThread*) * s_AttachedThreads->size());
    }

    static void
    thread_cleanup_on_cancel(void* arg)
    {
        Thread::Detach((Il2CppThread*)arg);

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        il2cpp::os::Thread* osThread = ((Il2CppThread*)arg)->GetInternalThread()->handle;
        osThread->SignalExited();
#endif
    }

    void Thread::Initialize()
    {
#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        os::Thread::SetNativeThreadCleanup(&thread_cleanup_on_cancel);
#endif
#if IL2CPP_ENABLE_RELOAD
        s_BlockNewThreads = false;
#endif
        s_AttachedThreads = new GCTrackedThreadVector();
    }

    void Thread::Uninitialize()
    {
        IL2CPP_ASSERT(Current() == Main());

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        os::Thread::SetNativeThreadCleanup(NULL);
#endif

        delete s_AttachedThreads;
        s_AttachedThreads = NULL;

        s_MainThread = NULL;

        s_CurrentThread.SetValue(NULL);
    }

    Il2CppThread* Thread::Attach(Il2CppDomain *domain)
    {
        Il2CppThread* managedThread = Current();

        if (managedThread != NULL)
            return managedThread;

        int temp = 0;
        if (!gc::GarbageCollector::RegisterThread(&temp))
            IL2CPP_ASSERT(0 && "gc::GarbageCollector::RegisterThread failed");

        StackTrace::InitializeStackTracesForCurrentThread();

        // Get/create OS thread representing the current thread. For pre-existing threads such as
        // the main thread, this will create an OS thread instance on demand. For threads that have
        // been started through our OS layer, there will already be an instance.
        os::Thread* osThread = os::Thread::GetOrCreateCurrentThread();

        // Create managed object representing the current thread.
        managedThread = (Il2CppThread*)Object::New(il2cpp_defaults.thread_class);
        SetupInternalManagedThread(managedThread, osThread);

        managedThread->GetInternalThread()->state = kThreadStateRunning;

        InitializeManagedThread(managedThread, domain);

        return managedThread;
    }

    void Thread::SetupInternalManagedThread(Il2CppThread* thread, os::Thread* osThread)
    {
        Il2CppInternalThread* internalManagedThread = (Il2CppInternalThread*)Object::New(il2cpp_defaults.internal_thread_class);
        internalManagedThread->handle = osThread;
        internalManagedThread->tid = osThread->Id();
        internalManagedThread->managed_id = GetNewManagedId();

        // The synch_cs object is deallocated in the InternalThread::Thread_free_internal icall, which
        // is called from the managed thread finalizer.
        internalManagedThread->longlived = (Il2CppLongLivedThreadData*)IL2CPP_MALLOC(sizeof(Il2CppLongLivedThreadData));
        internalManagedThread->longlived->synch_cs = new baselib::ReentrantLock;

        internalManagedThread->apartment_state = il2cpp::os::kApartmentStateUnknown;
        gc::WriteBarrier::GenericStore(&thread->internal_thread, internalManagedThread);
    }

    void Thread::InitializeManagedThread(Il2CppThread* thread, Il2CppDomain* domain)
    {
#if IL2CPP_SUPPORT_THREADS
        IL2CPP_ASSERT(thread->GetInternalThread()->handle != NULL);
        IL2CPP_ASSERT(thread->GetInternalThread()->longlived->synch_cs != NULL);
#endif

#if IL2CPP_MONO_DEBUGGER
        utils::Debugger::AllocateThreadLocalData();
#endif

        s_CurrentThread.SetValue(thread);

        Domain::ContextSet(domain->default_context);

        Register(thread);
        AdjustStaticData();

#if IL2CPP_MONO_DEBUGGER
        MONO_PROFILER_RAISE(thread_started, ((uintptr_t)thread->GetInternalThread()->tid));
#endif

#if IL2CPP_ENABLE_PROFILER
        vm::Profiler::ThreadStart(((unsigned long)thread->GetInternalThread()->tid));
#endif

        // Sync thread name.
        if (thread->GetInternalThread()->name.chars)
        {
            std::string utf8Name = il2cpp::utils::StringUtils::Utf16ToUtf8(thread->GetInternalThread()->name.chars);
            thread->GetInternalThread()->handle->SetName(utf8Name.c_str());
        }

        // Sync thread apartment state.
        thread->GetInternalThread()->apartment_state = thread->GetInternalThread()->handle->GetApartment();

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        // register us for platform specific cleanup attempt in case thread is not exited cleanly
        os::Thread::RegisterCurrentThreadForCleanup(thread);
#endif

        // If an interrupt has been requested before the thread was started, re-request
        // the interrupt now.
        if (thread->GetInternalThread()->interruption_requested)
            RequestInterrupt(thread);
    }

    void Thread::UninitializeManagedThread(Il2CppThread *thread)
    {
#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP
        // unregister from special cleanup since we are doing it now
        os::Thread::UnregisterCurrentThreadForCleanup();
#endif

        if (!gc::GarbageCollector::UnregisterThread())
            IL2CPP_ASSERT(0 && "gc::GarbageCollector::UnregisterThread failed");

#if IL2CPP_ENABLE_PROFILER
        vm::Profiler::ThreadEnd(((unsigned long)thread->GetInternalThread()->tid));
#endif

#if IL2CPP_MONO_DEBUGGER
        // Only raise the event for the debugger if there is a current thread at the OS thread level.
        // The debugger code will try to take a lock, which requires a current thread. If this
        // thread is being detached by a call from thread_cleanup_on_cancel, then there might
        // not be a current thread, as pthreads does not privide TLS entries in thread destructors.
        if (os::Thread::HasCurrentThread())
            MONO_PROFILER_RAISE(thread_stopped, ((uintptr_t)thread->GetInternalThread()->tid));
#endif

        FreeThreadStaticData(thread);

        // Call Unregister after all access to managed objects (Il2CppThread and Il2CppInternalThread)
        // is complete. Unregister will remove the managed thread object from the GC tracked vector of
        // attached threads, and allow it to be finalized and re-used. If runtime code accesses it
        // after a call to Unregister, there will be a race condition between the GC and the runtime
        // code for access to that object.
        Unregister(thread);

#if IL2CPP_MONO_DEBUGGER
        utils::Debugger::FreeThreadLocalData();
#endif

        os::Thread::DetachCurrentThread();
        s_CurrentThread.SetValue(NULL);
    }

    Il2CppThread* Thread::Current()
    {
        void* value = NULL;
        s_CurrentThread.GetValue(&value);
        return (Il2CppThread*)value;
    }

    Il2CppThread** Thread::GetAllAttachedThreads(size_t &size)
    {
        size = s_AttachedThreads->size();
        return &(*s_AttachedThreads)[0];
    }

    static void STDCALL TerminateThread(void* context)
    {
        // We throw a dummy exception to make sure things clean up properly
        // and we don't leave any locks behind (such as global locks in the allocator which
        // would then deadlock other threads). This could work off ThreadAbortException
        // but we don't want to deal with a managed exception here. So we use a C++ exception.
        throw Thread::NativeThreadAbortException();
    }

    static bool IsDebuggerThread(os::Thread* thread)
    {
#if IL2CPP_MONO_DEBUGGER
        return utils::Debugger::IsDebuggerThread(thread);
#else
        return false;
#endif
    }

    // This function requests that all threads exit
    // If a thread is in a non-alertable wait it may not have exited when this method exits
    void Thread::AbortAllThreads()
    {
#if IL2CPP_SUPPORT_THREADS
        Il2CppThread* gcFinalizerThread = NULL;
        Il2CppThread* currentThread = Current();
        IL2CPP_ASSERT(currentThread != NULL && "No current thread!");

        s_ThreadMutex.Acquire();
        s_BlockNewThreads = true;
        GCTrackedThreadVector attachedThreadsCopy = *s_AttachedThreads;

        // In theory, we don't need a write barrier here for Boehm, because we keep a
        // reference to the object on the stack during it's lifetime. But for validation
        // tests, we turn off GC, and thus we need it to pass.
        gc::GarbageCollector::SetWriteBarrier((void**)attachedThreadsCopy.data(), sizeof(Il2CppThread*) * attachedThreadsCopy.size());
        s_ThreadMutex.Release();

        std::vector<os::Thread*> activeThreads;

        // Kill all threads but the finalizer and current one. We temporarily flush out
        // the entire list and then just put the two threads back.
        while (attachedThreadsCopy.size())
        {
            Il2CppThread* thread = attachedThreadsCopy.back();
            os::Thread* osThread = thread->GetInternalThread()->handle;

            if (gc::GarbageCollector::IsFinalizerThread(thread))
            {
                IL2CPP_ASSERT(gcFinalizerThread == NULL && "There seems to be more than one finalizer thread!");
                gcFinalizerThread = thread;
            }
            else if (thread != currentThread && !IsDebuggerThread(osThread))
            {
                ////TODO: use Thread.Abort() instead
                osThread->QueueUserAPC(TerminateThread, NULL);
                activeThreads.push_back(osThread);
            }

            attachedThreadsCopy.pop_back();
        }
        // In theory, we don't need a write barrier here for Boehm, because we keep a
        // reference to the object on the stack during it's lifetime. But for validation
        // tests, we turn off GC, and thus we need it to pass.
        gc::GarbageCollector::SetWriteBarrier((void**)attachedThreadsCopy.data(), sizeof(Il2CppThread*) * attachedThreadsCopy.size());

        ////FIXME: While we don't have stable thread abortion in place yet, work around problems in
        ////    the current implementation by repeatedly requesting threads to terminate. This works around
        ////    race condition to some extent.
        while (activeThreads.size())
        {
            os::Thread* osThread = activeThreads.back();

            // Wait for the thread.
            if (osThread->Join(10) == kWaitStatusSuccess)
                activeThreads.pop_back();
            else
            {
                ////TODO: use Thread.Abort() instead
                osThread->QueueUserAPC(TerminateThread, NULL);
            }
        }


        AUTO_LOCK_THREADS();
        s_AttachedThreads->clear();

        // Put finalizer and current thread back in list.
        IL2CPP_ASSERT(gcFinalizerThread != NULL && "GC finalizer thread was not found in list of attached threads!");
        if (gcFinalizerThread)
            s_AttachedThreads->push_back(gcFinalizerThread);
        if (currentThread)
            s_AttachedThreads->push_back(currentThread);
        set_wbarrier_for_attached_threads();
#endif
    }

    void Thread::Detach(Il2CppThread *thread)
    {
        IL2CPP_ASSERT(thread != NULL && "Cannot detach a NULL thread");

        UninitializeManagedThread(thread);
        il2cpp::vm::StackTrace::CleanupStackTracesForCurrentThread();
    }

    Il2CppThread* Thread::Main()
    {
        return s_MainThread;
    }

    void Thread::SetMain(Il2CppThread* thread)
    {
        IL2CPP_ASSERT(s_MainThread == NULL);
        s_MainThread = thread;
    }

    void Thread::SetState(Il2CppThread *thread, ThreadState value)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);
        thread->GetInternalThread()->state |= value;
    }

    void Thread::ClrState(Il2CppInternalThread* thread, ThreadState clr)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);
        thread->state &= ~clr;
    }

    void Thread::SetState(Il2CppInternalThread *thread, ThreadState value)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);
        thread->state |= value;
    }

    ThreadState Thread::GetState(Il2CppInternalThread *thread)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);
        return (ThreadState)thread->state;
    }

    bool Thread::TestState(Il2CppInternalThread* thread, ThreadState value)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);
        return (thread->state & value) != 0;
    }

    Il2CppInternalThread* Thread::CurrentInternal()
    {
        return Current()->GetInternalThread();
    }

    ThreadState Thread::GetState(Il2CppThread *thread)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);
        return (ThreadState)thread->GetInternalThread()->state;
    }

    void Thread::ClrState(Il2CppThread* thread, ThreadState state)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);
        thread->GetInternalThread()->state &= ~state;
    }

    const int32_t kMaxThreadStaticSlots = 2048;

    void Thread::AdjustStaticData()
    {
        AUTO_LOCK_THREADS();
        size_t index = 0;
        Il2CppThread* thread = Current();
        if (!thread->GetInternalThread()->static_data)
            thread->GetInternalThread()->static_data = (void**)IL2CPP_CALLOC(kMaxThreadStaticSlots, sizeof(void*));
        for (std::vector<int32_t>::const_iterator iter = s_ThreadStaticSizes.begin(); iter != s_ThreadStaticSizes.end(); ++iter)
        {
            if (!thread->GetInternalThread()->static_data[index])
                thread->GetInternalThread()->static_data[index] = gc::GarbageCollector::AllocateFixed(*iter, NULL);
            index++;
        }
    }

    int32_t Thread::AllocThreadStaticData(int32_t size)
    {
        AUTO_LOCK_THREADS();
        int32_t index = (int32_t)s_ThreadStaticSizes.size();
        IL2CPP_ASSERT(index < kMaxThreadStaticSlots);
        s_ThreadStaticSizes.push_back(size);
        for (GCTrackedThreadVector::const_iterator iter = s_AttachedThreads->begin(); iter != s_AttachedThreads->end(); ++iter)
        {
            Il2CppThread* thread = *iter;
            if (!thread->GetInternalThread()->static_data)
                thread->GetInternalThread()->static_data = (void**)IL2CPP_CALLOC(kMaxThreadStaticSlots, sizeof(void*));
            thread->GetInternalThread()->static_data[index] = gc::GarbageCollector::AllocateFixed(size, NULL);
        }

        return index;
    }

    void Thread::FreeThreadStaticData(Il2CppThread *thread)
    {
        AUTO_LOCK_THREADS();
        size_t index = 0;
        for (std::vector<int32_t>::const_iterator iter = s_ThreadStaticSizes.begin(); iter != s_ThreadStaticSizes.end(); ++iter)
        {
            if (thread->GetInternalThread()->static_data[index])
                gc::GarbageCollector::FreeFixed(thread->GetInternalThread()->static_data[index]);
            index++;
        }
        IL2CPP_FREE(thread->GetInternalThread()->static_data);
        thread->GetInternalThread()->static_data = NULL;
    }

    void* Thread::GetThreadStaticData(int32_t offset)
    {
        // No lock. We allocate static_data once with a fixed size so we can read it
        // safely without a lock here.
        IL2CPP_ASSERT(offset >= 0 && static_cast<uint32_t>(offset) < s_ThreadStaticSizes.size());
        return Current()->GetInternalThread()->static_data[offset];
    }

    void* Thread::GetThreadStaticDataForThread(int32_t offset, Il2CppThread* thread)
    {
        // No lock. We allocate static_data once with a fixed size so we can read it
        // safely without a lock here.
        IL2CPP_ASSERT(offset >= 0 && static_cast<uint32_t>(offset) < s_ThreadStaticSizes.size());
        return thread->GetInternalThread()->static_data[offset];
    }

    void* Thread::GetThreadStaticDataForThread(int32_t offset, Il2CppInternalThread* thread)
    {
        // No lock. We allocate static_data once with a fixed size so we can read it
        // safely without a lock here.
        IL2CPP_ASSERT(offset >= 0 && static_cast<uint32_t>(offset) < s_ThreadStaticSizes.size());
        return thread->static_data[offset];
    }

    void Thread::Register(Il2CppThread *thread)
    {
        AUTO_LOCK_THREADS();
        if (s_BlockNewThreads)
            TerminateThread(NULL);
        else
        {
            s_AttachedThreads->push_back(thread);
            set_wbarrier_for_attached_threads();
        }
    }

    void Thread::Unregister(Il2CppThread *thread)
    {
        AUTO_LOCK_THREADS();
        GCTrackedThreadVector::iterator it = std::find(s_AttachedThreads->begin(), s_AttachedThreads->end(), thread);

#if IL2CPP_MONO_DEBUGGER
        if (it == s_AttachedThreads->end() && thread->internal_thread && il2cpp::utils::Debugger::IsDebuggerThread(thread->internal_thread->handle))
            return;
#endif

        IL2CPP_ASSERT(it != s_AttachedThreads->end() && "Vm thread not found in list of attached threads.");
        s_AttachedThreads->erase(it);
        set_wbarrier_for_attached_threads();
    }

    bool Thread::IsVmThread(Il2CppThread *thread)
    {
        return !gc::GarbageCollector::IsFinalizerThread(thread);
    }

    std::string Thread::GetName(Il2CppInternalThread* thread)
    {
        if (thread->name.chars == NULL)
            return std::string();

        return utils::StringUtils::Utf16ToUtf8(thread->name.chars);
    }

    void Thread::SetName(Il2CppThread* thread, Il2CppString* name)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);

        // Throw if already set.
        if (thread->GetInternalThread()->name.length != 0)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Thread name can only be set once."));

        // Store name.
        thread->GetInternalThread()->name.length = utils::StringUtils::GetLength(name);
        thread->GetInternalThread()->name.chars = il2cpp::utils::StringUtils::StringDuplicate(utils::StringUtils::GetChars(name), thread->GetInternalThread()->name.length);

        // Hand over to OS layer, if thread has been started already.
        if (thread->GetInternalThread()->handle)
        {
            std::string utf8Name = il2cpp::utils::StringUtils::Utf16ToUtf8(thread->GetInternalThread()->name.chars);
            thread->GetInternalThread()->handle->SetName(utf8Name.c_str());
        }
    }

    void Thread::SetName(Il2CppInternalThread* thread, Il2CppString* name)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);

        // Throw if already set.
        if (thread->name.length != 0)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Thread name can only be set once."));

        // Store name.
        thread->name.length = utils::StringUtils::GetLength(name);
        thread->name.chars = il2cpp::utils::StringUtils::StringDuplicate(utils::StringUtils::GetChars(name), thread->name.length);

        // Hand over to OS layer, if thread has been started already.
        if (thread->handle)
        {
            std::string utf8Name = il2cpp::utils::StringUtils::Utf16ToUtf8(thread->name.chars);
            thread->handle->SetName(utf8Name.c_str());
        }
    }

    static void STDCALL CheckCurrentThreadForInterruptCallback(void* context)
    {
        Thread::CheckCurrentThreadForInterruptAndThrowIfNecessary();
    }

    void Thread::RequestInterrupt(Il2CppThread* thread)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);

        thread->GetInternalThread()->interruption_requested = true;

        // If thread has already been started, queue an interrupt now.
        il2cpp::os::Thread* osThread = thread->GetInternalThread()->handle;
        if (osThread)
            osThread->QueueUserAPC(CheckCurrentThreadForInterruptCallback, NULL);
    }

    void Thread::CheckCurrentThreadForInterruptAndThrowIfNecessary()
    {
        Il2CppThread* currentThread = il2cpp::vm::Thread::Current();
        if (!currentThread)
            return;

        il2cpp::os::FastAutoLock lock(currentThread->GetInternalThread()->longlived->synch_cs);

        // Don't throw if thread is not currently in waiting state or if there's
        // no pending interrupt.
        if (!currentThread->GetInternalThread()->interruption_requested
            || !(il2cpp::vm::Thread::GetState(currentThread) & il2cpp::vm::kThreadStateWaitSleepJoin))
            return;

        // Mark the current thread as being unblocked.
        currentThread->GetInternalThread()->interruption_requested = false;
        il2cpp::vm::Thread::ClrState(currentThread, il2cpp::vm::kThreadStateWaitSleepJoin);

        // Throw interrupt exception.
        il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetThreadInterruptedException());
    }

    static void STDCALL CheckCurrentThreadForAbortCallback(void* context)
    {
        Thread::CheckCurrentThreadForAbortAndThrowIfNecessary();
    }

    bool Thread::RequestAbort(Il2CppThread* thread)
    {
        il2cpp::os::FastAutoLock lock(thread->GetInternalThread()->longlived->synch_cs);

        ThreadState state = il2cpp::vm::Thread::GetState(thread);
        if (state & kThreadStateAbortRequested || state & kThreadStateStopped || state & kThreadStateStopRequested)
            return false;

        il2cpp::os::Thread* osThread = thread->GetInternalThread()->handle;
        if (osThread)
        {
            // If thread has already been started, queue an abort now.
            Thread::SetState(thread, kThreadStateAbortRequested);
            osThread->QueueUserAPC(CheckCurrentThreadForAbortCallback, NULL);
        }
        else
        {
            // If thread has not started, put it in the aborted state.
            Thread::SetState(thread, kThreadStateAborted);
        }

        return true;
    }

    bool Thread::RequestAbort(Il2CppInternalThread* thread)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);

        ThreadState state = il2cpp::vm::Thread::GetState(thread);
        if (state & kThreadStateAbortRequested || state & kThreadStateStopped || state & kThreadStateStopRequested)
            return false;

        il2cpp::os::Thread* osThread = thread->handle;
        if (osThread)
        {
            // If thread has already been started, queue an abort now.
            Thread::SetState(thread, kThreadStateAbortRequested);
            osThread->QueueUserAPC(CheckCurrentThreadForAbortCallback, NULL);
        }
        else
        {
            // If thread has not started, put it in the aborted state.
            Thread::SetState(thread, kThreadStateAborted);
        }

        return true;
    }

    void Thread::SetPriority(Il2CppThread* thread, int32_t priority)
    {
        Il2CppInternalThread* internalThread = thread->GetInternalThread();
        il2cpp::os::FastAutoLock lock(internalThread->longlived->synch_cs);
        internalThread->handle->SetPriority((il2cpp::os::ThreadPriority)priority);
    }

    int32_t Thread::GetPriority(Il2CppThread* thread)
    {
        Il2CppInternalThread* internalThread = thread->GetInternalThread();
        il2cpp::os::FastAutoLock lock(internalThread->longlived->synch_cs);
        return internalThread->handle->GetPriority();
    }

    struct StartDataInternal
    {
        Il2CppThread* m_Thread;
        Il2CppDomain* m_Domain;
        void* m_Delegate;
        void* m_StartArg;
        il2cpp::os::Semaphore* m_Semaphore;
    };

    static void ThreadStart(void* arg)
    {
        StartDataInternal* startData = (StartDataInternal*)arg;

        startData->m_Semaphore->Wait();

        {
            int temp = 0;
            if (!gc::GarbageCollector::RegisterThread(&temp))
                IL2CPP_ASSERT(0 && "gc::GarbageCollector::RegisterThread failed");

            il2cpp::vm::StackTrace::InitializeStackTracesForCurrentThread();

            bool attachSuccessful = false;
            try
            {
                il2cpp::vm::Thread::InitializeManagedThread(startData->m_Thread, startData->m_Domain);
                il2cpp::vm::Thread::SetState(startData->m_Thread, kThreadStateRunning);

                attachSuccessful = true;

                try
                {
                    ((void(*)(void*))startData->m_Delegate)(startData->m_StartArg);
                }
                catch (Il2CppExceptionWrapper& ex)
                {
                    // Only deal with the unhandled exception if the runtime is not
                    // shutting down. Otherwise, the code to process the unhandled
                    // exception might fail in unexpected ways, because it needs
                    // the full runtime available. We've seen this cause crashes
                    // that are difficult to reproduce locally.
                    if (!il2cpp::vm::Runtime::IsShuttingDown())
                        Runtime::UnhandledException(ex.ex);
                }
            }
            catch (il2cpp::vm::Thread::NativeThreadAbortException)
            {
                // Nothing to do. We've successfully aborted the thread.
                il2cpp::vm::Thread::SetState(startData->m_Thread, kThreadStateAborted);
            }

            il2cpp::vm::Thread::ClrState(startData->m_Thread, kThreadStateRunning);
            il2cpp::vm::Thread::SetState(startData->m_Thread, kThreadStateStopped);
            if (attachSuccessful)
                il2cpp::vm::Thread::UninitializeManagedThread(startData->m_Thread);

            il2cpp::vm::StackTrace::CleanupStackTracesForCurrentThread();
        }

        delete startData->m_Semaphore;
        gc::GarbageCollector::FreeFixed(startData);
    }

    Il2CppInternalThread* Thread::CreateInternal(void(*func)(void*), void* arg, bool threadpool_thread, uint32_t stack_size)
    {
        // The os::Thread object is deallocated in the InternalThread::Thread_free_internal icall, which
        // is called from the managed thread finalizer.
        os::Thread* osThread = new os::Thread();
        Il2CppThread* managedThread = (Il2CppThread*)Object::New(il2cpp_defaults.thread_class);

        SetupInternalManagedThread(managedThread, osThread);
        Il2CppInternalThread* internalManagedThread = managedThread->GetInternalThread();

        internalManagedThread->state = kThreadStateUnstarted;
        internalManagedThread->threadpool_thread = threadpool_thread;

        // use fixed GC memory since we are storing managed object pointers
        StartDataInternal* startData = (StartDataInternal*)gc::GarbageCollector::AllocateFixed(sizeof(StartDataInternal), NULL);
        gc::WriteBarrier::GenericStore(&startData->m_Thread, managedThread);
        gc::WriteBarrier::GenericStore(&startData->m_Domain, Domain::GetCurrent());
        startData->m_Delegate = (void*)func;
        startData->m_StartArg = arg;
        startData->m_Semaphore = new il2cpp::os::Semaphore(0);

        osThread->SetStackSize(stack_size);
        osThread->SetExplicitApartment(static_cast<il2cpp::os::ApartmentState>(managedThread->GetInternalThread()->apartment_state));
        il2cpp::os::ErrorCode status = osThread->Run(&ThreadStart, startData);
        if (status != il2cpp::os::kErrorCodeSuccess)
        {
            delete osThread;
            return NULL;
        }

        internalManagedThread->state &= ~kThreadStateUnstarted;

        startData->m_Semaphore->Post(1, NULL);

        return internalManagedThread;
    }

    void Thread::Stop(Il2CppInternalThread* thread)
    {
        IL2CPP_ASSERT(thread != CurrentInternal());
        if (!RequestAbort(thread))
            return;

        os::Thread* osThread = thread->handle;

        ////FIXME: While we don't have stable thread abortion in place yet, work around problems in
        ////    the current implementation by repeatedly requesting threads to terminate. This works around
        ////    race condition to some extent.
        while (true)
        {
            // If it's a background thread, request it to kill itself.
            if (GetState(thread) & kThreadStateBackground)
            {
                ////TODO: use Thread.Abort() instead
                osThread->QueueUserAPC(TerminateThread, NULL);
            }

            // Wait for the thread.
            if (osThread->Join(10) == kWaitStatusSuccess)
                break;
        }
    }

    void Thread::Sleep(uint32_t ms)
    {
        CurrentInternal()->handle->Sleep(ms);
    }

    bool Thread::YieldInternal()
    {
        return os::Thread::YieldInternal();
    }

    void Thread::SetDefaultAffinityMask(int64_t affinityMask)
    {
#if defined(IL2CPP_ENABLE_PLATFORM_THREAD_AFFINTY)
        os::Thread::SetDefaultAffinityMask(affinityMask);
#endif
    }

    void Thread::CheckCurrentThreadForAbortAndThrowIfNecessary()
    {
        Il2CppThread* currentThread = il2cpp::vm::Thread::Current();
        if (!currentThread)
            return;

        il2cpp::os::FastAutoLock lock(currentThread->GetInternalThread()->longlived->synch_cs);

        ThreadState state = il2cpp::vm::Thread::GetState(currentThread);
        if (!(state & kThreadStateAbortRequested))
            return;

        // Throw interrupt exception.
        Il2CppException* abortException = il2cpp::vm::Exception::GetThreadAbortException();
        IL2CPP_OBJECT_SETREF(currentThread->GetInternalThread(), abort_exc, abortException);
        il2cpp::vm::Exception::Raise(abortException);
    }

    void Thread::ResetAbort(Il2CppThread* thread)
    {
        il2cpp::vm::Thread::ClrState(thread, kThreadStateAbortRequested);
        if (thread->GetInternalThread()->abort_exc == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetThreadStateException("Unable to reset abort because no abort was requested."));
    }

    void Thread::ResetAbort(Il2CppInternalThread* thread)
    {
        il2cpp::vm::Thread::ClrState(thread, kThreadStateAbortRequested);
        if (thread->abort_exc == NULL)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetThreadStateException("Unable to reset abort because no abort was requested."));
    }

    void Thread::FullMemoryBarrier()
    {
        os::Atomic::FullMemoryBarrier();
    }

    int32_t Thread::GetNewManagedId()
    {
        return ++s_NextManagedThreadId;
    }

    uint64_t Thread::GetId(Il2CppThread* thread)
    {
        return thread->GetInternalThread()->tid;
    }

    uint64_t Thread::GetId(Il2CppInternalThread* thread)
    {
        return thread->tid;
    }
} /* namespace vm */
} /* namespace il2cpp */
