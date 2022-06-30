#include "il2cpp-config.h"
#include <memory>
#include "icalls/mscorlib/System.Threading/Thread.h"
#include "il2cpp-class-internals.h"
#include "gc/GarbageCollector.h"
#include "os/Atomic.h"
#include "os/Thread.h"
#include "os/Mutex.h"
#include "os/Semaphore.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Domain.h"
#include "vm/Object.h"
#include "vm/Runtime.h"
#include "vm/String.h"
#include "vm/Thread.h"
#include "vm/Exception.h"
#include "vm/String.h"
#include "vm/StackTrace.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"

using il2cpp::gc::GarbageCollector;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
    struct StartData
    {
        Il2CppThread* m_Thread;
        Il2CppDomain* m_Domain;
        Il2CppDelegate* m_Delegate;
        Il2CppObject* m_StartArg;
        il2cpp::os::Semaphore* m_Semaphore;
    };

    static void ThreadStart(void* arg)
    {
        StartData* startData = (StartData*)arg;

        startData->m_Semaphore->Wait();

        {
            int temp = 0;
            if (!GarbageCollector::RegisterThread(&temp))
                IL2CPP_ASSERT(0 && "GarbageCollector::RegisterThread failed");

            il2cpp::vm::StackTrace::InitializeStackTracesForCurrentThread();

            il2cpp::vm::Thread::InitializeManagedThread(startData->m_Thread, startData->m_Domain);
            il2cpp::vm::Thread::SetState(startData->m_Thread, vm::kThreadStateRunning);

            try
            {
                Il2CppException* exc = NULL;
                void* args[1] = { startData->m_StartArg };
                vm::Runtime::DelegateInvoke(startData->m_Delegate, args, &exc);

                if (exc)
                    vm::Runtime::UnhandledException(exc);
            }
            catch (il2cpp::vm::Thread::NativeThreadAbortException)
            {
                // Nothing to do. We've successfully aborted the thread.
                il2cpp::vm::Thread::SetState(startData->m_Thread, vm::kThreadStateAborted);
            }

            il2cpp::vm::Thread::ClrState(startData->m_Thread, vm::kThreadStateRunning);
            il2cpp::vm::Thread::SetState(startData->m_Thread, vm::kThreadStateStopped);
            il2cpp::vm::Thread::UninitializeManagedThread(startData->m_Thread);

            il2cpp::vm::StackTrace::CleanupStackTracesForCurrentThread();
        }

        delete startData->m_Semaphore;
        GarbageCollector::FreeFixed(startData);
    }

    bool Thread::JoinInternal(Il2CppThread* thisPtr, int32_t millisecondsTimeout)
    {
        // Throw ThreadStateException if thread has not been started yet.
        if (il2cpp::vm::Thread::GetState(thisPtr) & vm::kThreadStateUnstarted)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetThreadStateException("Thread has not been started."));

        // Mark current thread as blocked.
        Il2CppThread* currentThread = il2cpp::vm::Thread::Current();
        SetState(currentThread->internal_thread, vm::kThreadStateWaitSleepJoin);

        // Join with other thread.
        il2cpp::os::Thread* osThread = (il2cpp::os::Thread*)thisPtr->GetInternalThread()->handle;
        IL2CPP_ASSERT(osThread != NULL);
        il2cpp::os::WaitStatus status = osThread->Join(millisecondsTimeout);

        // Unblock current thread.
        ClrState(currentThread->internal_thread, vm::kThreadStateWaitSleepJoin);

        if (status == kWaitStatusSuccess)
            return true;

        return false;
    }

    bool Thread::Thread_internal(Il2CppThread* thisPtr, Il2CppDelegate* start)
    {
        IL2CPP_ASSERT(thisPtr->GetInternalThread()->longlived->synch_cs != NULL);
        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->longlived->synch_cs);

        if (il2cpp::vm::Thread::GetState(thisPtr) & vm::kThreadStateAborted)
        {
            return reinterpret_cast<intptr_t>(thisPtr->GetInternalThread()->handle);
        }

        // use fixed GC memory since we are storing managed object pointers
        StartData* startData = (StartData*)GarbageCollector::AllocateFixed(sizeof(StartData), NULL);

        startData->m_Thread = thisPtr;
        GarbageCollector::SetWriteBarrier((void**)&startData->m_Thread);
        startData->m_Domain = vm::Domain::GetCurrent();
        startData->m_Delegate = start;
        GarbageCollector::SetWriteBarrier((void**)&startData->m_Delegate);
        startData->m_StartArg = thisPtr->start_obj;
        GarbageCollector::SetWriteBarrier((void**)&startData->m_StartArg);
        startData->m_Semaphore = new il2cpp::os::Semaphore(0);

        il2cpp::os::Thread* thread = new il2cpp::os::Thread();
        thread->SetStackSize(thisPtr->GetInternalThread()->stack_size);
        thread->SetExplicitApartment(static_cast<il2cpp::os::ApartmentState>(thisPtr->GetInternalThread()->apartment_state));
        il2cpp::os::ErrorCode status = thread->Run(&ThreadStart, startData);
        if (status != il2cpp::os::kErrorCodeSuccess)
        {
            delete thread;
            return false;
        }

        uint32_t existingPriority = il2cpp::vm::Thread::GetPriority(thisPtr);

        thisPtr->GetInternalThread()->handle = thread;
        thisPtr->GetInternalThread()->state &= ~vm::kThreadStateUnstarted;
        thisPtr->GetInternalThread()->tid = thread->Id();
        if (!thisPtr->GetInternalThread()->managed_id)
            thisPtr->GetInternalThread()->managed_id = il2cpp::vm::Thread::GetNewManagedId();

        startData->m_Semaphore->Post(1, NULL);

        il2cpp::vm::Thread::SetPriority(thisPtr, existingPriority);

        // this is just checked against 0 in the calling code
        return reinterpret_cast<intptr_t>(thisPtr->GetInternalThread()->handle);
    }

    bool Thread::YieldInternal()
    {
        return vm::Thread::YieldInternal();
    }

    Il2CppArray* Thread::ByteArrayToCurrentDomain(Il2CppArray* arr)
    {
        // IL2CPP only has one domain, so just return the same array.
        return arr;
    }

    Il2CppArray* Thread::ByteArrayToRootDomain(Il2CppArray* arr)
    {
        // IL2CPP only has one domain, so just return the same array.
        return arr;
    }

    int32_t Thread::GetDomainID()
    {
        return il2cpp::vm::Domain::GetCurrent()->domain_id;
    }

    int32_t Thread::GetPriorityNative(Il2CppThread* _this)
    {
        return il2cpp::vm::Thread::GetPriority(_this);
    }

    int32_t Thread::SystemMaxStackStize()
    {
        return il2cpp::os::Thread::GetMaxStackSize();
    }

    Il2CppObject* Thread::GetAbortExceptionState(Il2CppThread* thisPtr)
    {
        NOT_SUPPORTED_IL2CPP(Thread::GetAbortExceptionState, "Thread abortion is currently not implemented on IL2CPP; it is recommended to use safer mechanisms to terminate threads.");
        return 0;
    }

// Unlike Mono we cannot just do a memory read/write of the correct type and reuse that for the
// floating-point types because we are compiling to C++ and have to account for its type conversion
// rules. For example, if we read a double as a uint64_t and return it as such, the compiler will
// perform a default conversion from uint64_t to double -- whereas what we want is to simply interpret
// the memory contents as a double.
    int8_t Thread::VolatileRead_1(volatile void* address)
    {
        int8_t tmp = *reinterpret_cast<volatile int8_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    double Thread::VolatileRead_Double(volatile double* address)
    {
        double tmp = *reinterpret_cast<volatile double*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    float Thread::VolatileRead_Float(volatile float* address)
    {
        float tmp = *reinterpret_cast<volatile float*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int16_t Thread::VolatileRead_2(volatile void* address)
    {
        int16_t tmp = *reinterpret_cast<volatile int16_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int32_t Thread::VolatileRead_4(volatile void* address)
    {
        int32_t tmp = *reinterpret_cast<volatile int32_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int64_t Thread::VolatileRead_8(volatile void* address)
    {
        int64_t tmp = *reinterpret_cast<volatile int64_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    intptr_t Thread::VolatileRead_IntPtr(volatile void* address)
    {
        intptr_t tmp = *reinterpret_cast<volatile intptr_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    Il2CppObject* Thread::VolatileRead_Object(volatile void* address)
    {
        volatile Il2CppObject* tmp = *(volatile Il2CppObject**)(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return (Il2CppObject*)tmp;
    }

    Il2CppString* Thread::GetName_internal(Il2CppInternalThread* thread)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);

        if (thread->name.length == 0)
            return NULL;

        return il2cpp::vm::String::NewUtf16(thread->name.chars, thread->name.length);
    }

    int32_t Thread::GetState(Il2CppInternalThread* thread)
    {
        // There is a chance that the managed thread object can be used from code (like a
        // finalizer) after it has been destroyed. In that case, the objects that
        // the runtime uses to track this thread may have been freed. Try to check for
        // that case here and return early.
        if (thread->longlived == NULL)
            return vm::kThreadStateStopped;

        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);
        return (il2cpp::vm::ThreadState)thread->state;
    }

    void Thread::Abort_internal(Il2CppInternalThread* thread, Il2CppObject* stateInfo)
    {
        il2cpp::vm::Thread::RequestAbort(thread);
    }

    void Thread::ClrState(Il2CppInternalThread* thread, int32_t clr)
    {
        il2cpp::vm::Thread::ClrState(thread, (il2cpp::vm::ThreadState)clr);
    }

    void Thread::ConstructInternalThread(Il2CppThread* _this)
    {
        // The os::Thread object is deallocated in the InternalThread::Thread_free_internal icall, which
        // is called from the managed thread finalizer.
        vm::Thread::SetupInternalManagedThread(_this, new os::Thread());
        _this->GetInternalThread()->state = vm::kThreadStateUnstarted;
    }

    void Thread::GetCurrentThread_icall(volatile Il2CppThread** thread)
    {
        *thread = il2cpp::vm::Thread::Current();
        il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)thread);
    }

    void Thread::GetStackTraces(Il2CppArray** threads, Il2CppArray** stack_frames)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Thread::GetStackTraces);
        IL2CPP_UNREACHABLE;
    }

    void Thread::InterruptInternal(Il2CppThread* thisPtr)
    {
        il2cpp::vm::Thread::RequestInterrupt(thisPtr);
    }

    void Thread::MemoryBarrier()
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
    }

    void Thread::ResetAbortNative(Il2CppThread* thisPtr)
    {
        vm::Thread::ResetAbort(vm::Thread::CurrentInternal());
    }

    void Thread::ResumeInternal(Il2CppThread* thisPtr)
    {
        NOT_SUPPORTED_IL2CPP(Thread::Resume_internal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

    void Thread::SetName_icall(Il2CppInternalThread* thread, Il2CppChar* name, int32_t nameLength)
    {
        il2cpp::os::FastAutoLock lock(thread->longlived->synch_cs);

        // Throw if already set.
        if (thread->name.length != 0)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Thread name can only be set once."));

        // Store name.
        thread->name.length = nameLength;
        thread->name.chars = il2cpp::utils::StringUtils::StringDuplicate(name, thread->name.length);

        // Hand over to OS layer, if thread has been started already.
        if (thread->handle)
        {
            std::string utf8Name = il2cpp::utils::StringUtils::Utf16ToUtf8(thread->name.chars);
            thread->handle->SetName(utf8Name.c_str());
        }
    }

    void Thread::SetPriorityNative(Il2CppThread* thisPtr, int32_t priority)
    {
        vm::Thread::SetPriority(thisPtr, priority);
    }

    void Thread::SetState(Il2CppInternalThread* thisPtr, int32_t state)
    {
        il2cpp::os::FastAutoLock lock(thisPtr->longlived->synch_cs);
        il2cpp::vm::Thread::SetState(thisPtr, (il2cpp::vm::ThreadState)state);
    }

    void Thread::SleepInternal(int32_t millisecondsTimeout)
    {
        Il2CppInternalThread* thread = il2cpp::vm::Thread::Current()->internal_thread;
        SetState(thread, vm::kThreadStateWaitSleepJoin);
        il2cpp::os::Thread::Sleep(millisecondsTimeout, true);
        ClrState(thread, vm::kThreadStateWaitSleepJoin);
    }

    void Thread::SpinWait_nop()
    {
        // :rotating_thinking_face:
    }

    void Thread::SuspendInternal(Il2CppThread* thisPtr)
    {
        NOT_SUPPORTED_IL2CPP(Thread::SuspendInternal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

    void Thread::VolatileWrite_1(volatile void* address, int8_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int8_t*>(address) = value;
    }

    void Thread::VolatileWrite_Double(volatile void* address, double value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile double*>(address) = value;
    }

    void Thread::VolatileWrite_2(volatile void* address, int16_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int16_t*>(address) = value;
    }

    void Thread::VolatileWrite_4(volatile void* address, int32_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int32_t*>(address) = value;
    }

    void Thread::VolatileWrite_8(volatile void* address, int64_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int64_t*>(address) = value;
    }

    void Thread::VolatileWrite_IntPtr(volatile void* address, intptr_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile intptr_t*>(address) = value;
    }

    void Thread::VolatileWrite_Object(volatile void* address, Il2CppObject* value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<Il2CppObject* volatile *>(address) = value;
        il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)&address);
    }

    void Thread::VolatileWrite_Float(volatile void* address, float value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile float*>(address) = value;
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
