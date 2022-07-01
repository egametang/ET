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
#define NUM_CACHED_CULTURES 4
#define CULTURES_START_IDX 0
#define UICULTURES_START_IDX NUM_CACHED_CULTURES

    static Il2CppObject* lookup_cached_culture(Il2CppThread *thisPtr, int32_t start_idx);
    static void cache_culture(Il2CppThread *thisPtr, Il2CppObject *culture, int start_idx);


    void Thread::ClrState(Il2CppThread* thisPtr, uint32_t state)
    {
        il2cpp::vm::Thread::ClrState(thisPtr, (il2cpp::vm::ThreadState)state);
    }

    Il2CppThread * Thread::CurrentThread_internal(void)
    {
        return il2cpp::vm::Thread::Current();
    }

    int32_t Thread::GetDomainID()
    {
        return il2cpp::vm::Domain::GetCurrent()->domain_id;
    }

    uint32_t Thread::GetState(Il2CppThread * thisPtr)
    {
        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->synch_cs);
        return thisPtr->GetInternalThread()->state;
    }

    bool Thread::Join_internal(Il2CppThread * thisPtr, int32_t ms, void* thread)
    {
        // Throw ThreadStateException if thread has not been started yet.
        if (il2cpp::vm::Thread::GetState(thisPtr) & vm::kThreadStateUnstarted)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetThreadStateException("Thread has not been started."));

        // Mark current thread as blocked.
        Il2CppThread* currentThread = il2cpp::vm::Thread::Current();
        SetState(currentThread, vm::kThreadStateWaitSleepJoin);

        // Join with other thread.
        il2cpp::os::Thread* osThread = (il2cpp::os::Thread*)thisPtr->GetInternalThread()->handle;
        IL2CPP_ASSERT(osThread != NULL);
        il2cpp::os::WaitStatus status = osThread->Join(ms);

        // Unblock current thread.
        ClrState(currentThread, vm::kThreadStateWaitSleepJoin);

        if (status == kWaitStatusSuccess)
            return true;

        return false;
    }

    void Thread::ResetAbort_internal()
    {
        il2cpp::vm::Thread::ResetAbort(il2cpp::vm::Thread::Current());
    }

    Il2CppString* Thread::GetName_internal(Il2CppThread* thisPtr)
    {
        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->synch_cs);

        if (thisPtr->GetInternalThread()->name_len == 0)
            return NULL;

        return il2cpp::vm::String::NewUtf16(thisPtr->GetInternalThread()->name, thisPtr->GetInternalThread()->name_len);
    }

    void Thread::SetName_internal(Il2CppThread* thisPtr, Il2CppString* name)
    {
        vm::Thread::SetName(thisPtr, name);
    }

    void Thread::SetState(Il2CppThread* thisPtr, uint32_t state)
    {
        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->synch_cs);
        il2cpp::vm::Thread::SetState(thisPtr, (il2cpp::vm::ThreadState)state);
    }

    void Thread::Sleep_internal(int32_t milliseconds)
    {
        Il2CppThread* thread = il2cpp::vm::Thread::Current();
        SetState(thread, vm::kThreadStateWaitSleepJoin);
        il2cpp::os::Thread::Sleep(milliseconds, true);
        ClrState(thread, vm::kThreadStateWaitSleepJoin);
    }

    void Thread::Thread_init(Il2CppThread * thisPtr)
    {
        il2cpp::vm::Thread::Setup(thisPtr);
    }

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

            il2cpp::vm::Thread::Initialize(startData->m_Thread, startData->m_Domain);
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
            il2cpp::vm::Thread::Uninitialize(startData->m_Thread);

            il2cpp::vm::StackTrace::CleanupStackTracesForCurrentThread();
        }

        delete startData->m_Semaphore;
        GarbageCollector::FreeFixed(startData);
    }

    intptr_t Thread::Thread_internal(Il2CppThread * thisPtr, Il2CppDelegate * start)
    {
        IL2CPP_ASSERT(thisPtr->GetInternalThread()->synch_cs != NULL);
        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->synch_cs);

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
            return 0;
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

    static void cache_culture(Il2CppThread *thisPtr, Il2CppObject *culture, int start_idx)
    {
        int i;
        Il2CppObject *obj;
        int free_slot = -1;
        int same_domain_slot = -1;

        il2cpp::os::FastAutoLock lock(thisPtr->GetInternalThread()->synch_cs);

        if (!thisPtr->GetInternalThread()->cached_culture_info)
            IL2CPP_OBJECT_SETREF(thisPtr->GetInternalThread(), cached_culture_info, il2cpp::vm::Array::NewCached(il2cpp_defaults.object_class, NUM_CACHED_CULTURES * 2));

        for (i = start_idx; i < start_idx + NUM_CACHED_CULTURES; ++i)
        {
            obj = il2cpp_array_get(thisPtr->GetInternalThread()->cached_culture_info, Il2CppObject*, i);
            /* Free entry */
            if (!obj)
            {
                free_slot = i;
                /* we continue, because there may be a slot used with the same domain */
                continue;
            }
            /* Replace */
            same_domain_slot = i;
            break;
        }
        if (same_domain_slot >= 0)
            il2cpp_array_setref(thisPtr->GetInternalThread()->cached_culture_info, same_domain_slot, culture);
        else if (free_slot >= 0)
            il2cpp_array_setref(thisPtr->GetInternalThread()->cached_culture_info, free_slot, culture);
        /* we may want to replace an existing entry here, even when no suitable slot is found */
    }

    static Il2CppObject* lookup_cached_culture(Il2CppThread *thisPtr, int32_t start_idx)
    {
        Il2CppObject *res;
        int i;

        if (thisPtr->GetInternalThread()->cached_culture_info)
        {
            for (i = start_idx; i < start_idx + NUM_CACHED_CULTURES; ++i)
            {
                res = il2cpp_array_get(thisPtr->GetInternalThread()->cached_culture_info, Il2CppObject*, i);
                if (res)
                    return res;
            }
        }

        return NULL;
    }

    void Thread::FreeLocalSlotValues(int32_t slot, bool use_thread_local)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Thread::FreeLocalSlotValues);
    }

    mscorlib_System_Globalization_CultureInfo* Thread::GetCachedCurrentCulture(Il2CppThread* thisPtr)
    {
        return (mscorlib_System_Globalization_CultureInfo*)lookup_cached_culture(thisPtr, CULTURES_START_IDX);
    }

    void Thread::SetCachedCurrentCulture(Il2CppThread* thisPtr, Il2CppObject* culture)
    {
        cache_culture(thisPtr, culture, CULTURES_START_IDX);
    }

    mscorlib_System_Globalization_CultureInfo* Thread::GetCachedCurrentUICulture(Il2CppThread* thisPtr)
    {
        return (mscorlib_System_Globalization_CultureInfo*)lookup_cached_culture(thisPtr, UICULTURES_START_IDX);
    }

    void Thread::SetCachedCurrentUICulture(Il2CppThread* thisPtr, Il2CppObject* culture)
    {
        cache_culture(thisPtr, culture, UICULTURES_START_IDX);
    }

    static Il2CppArray* GetSerializedCulture(uint8_t*& data, uint32_t& length)
    {
        if (!data)
            return NULL;

        Il2CppArray* result = il2cpp::vm::Array::New(il2cpp_defaults.byte_class, length);
        memcpy(il2cpp_array_addr(result, uint8_t, 0), data, length);

        return result;
    }

    static void SetSerializedCulture(uint8_t*& data, uint32_t& length, Il2CppArray* culture)
    {
        if (data != NULL)
            delete data;

        int arrayLength = il2cpp::vm::Array::GetByteLength(culture);
        data = new uint8_t[arrayLength];
        length = arrayLength;
        memcpy(data, il2cpp_array_addr(culture, uint8_t, 0), arrayLength);
    }

    int32_t Thread::GetNewManagedId_internal()
    {
        return il2cpp::vm::Thread::GetNewManagedId();
    }

    void Thread::Abort_internal(Il2CppThread* thisPtr, Il2CppObject* stateInfo)
    {
        il2cpp::vm::Thread::RequestAbort(thisPtr);
    }

    Il2CppObject* Thread::GetAbortExceptionState(void* /* System.Threading.Thread */ self)
    {
        NOT_SUPPORTED_IL2CPP(Thread::GetAbortExceptionState, "Thread abortion is currently not implemented on IL2CPP; it is recommended to use safer mechanisms to terminate threads.");
        return 0;
    }

    void Thread::Interrupt_internal(Il2CppThread* thisPtr)
    {
        il2cpp::vm::Thread::RequestInterrupt(thisPtr);
    }

    void Thread::MemoryBarrier_()
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
    }

    void Thread::Resume_internal(void* /* System.Threading.Thread */ self)
    {
        NOT_SUPPORTED_IL2CPP(Thread::Resume_internal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

    void Thread::SpinWait_nop()
    {
    }

    void Thread::Suspend_internal(void* /* System.Threading.Thread */ self)
    {
        NOT_SUPPORTED_IL2CPP(Thread::Suspend_internal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

// Unlike Mono we cannot just do a memory read/write of the correct type and reuse that for the
// floating-point types because we are compiling to C++ and have to account for its type conversion
// rules. For example, if we read a double as a uint64_t and return it as such, the compiler will
// perform a default conversion from uint64_t to double -- whereas what we want is to simply interpret
// the memory contents as a double.

    int8_t Thread::VolatileReadInt8(volatile void* address)
    {
        int8_t tmp = *reinterpret_cast<volatile int8_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int16_t Thread::VolatileReadInt16(volatile void* address)
    {
        int16_t tmp = *reinterpret_cast<volatile int16_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int32_t Thread::VolatileReadInt32(volatile void* address)
    {
        int32_t tmp = *reinterpret_cast<volatile int32_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    int64_t Thread::VolatileReadInt64(volatile void* address)
    {
        int64_t tmp = *reinterpret_cast<volatile int64_t*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    float Thread::VolatileReadFloat(volatile void* address)
    {
        float tmp = *reinterpret_cast<volatile float*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    double Thread::VolatileReadDouble(volatile void* address)
    {
        double tmp = *reinterpret_cast<volatile double*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    void* Thread::VolatileReadPtr(volatile void* address)
    {
        void* tmp = *reinterpret_cast<void* volatile*>(address);
        il2cpp::os::Atomic::FullMemoryBarrier();
        return tmp;
    }

    intptr_t Thread::VolatileReadIntPtr(volatile void* address)
    {
        return reinterpret_cast<intptr_t>(VolatileReadPtr(address));
    }

    void Thread::VolatileWriteInt8(volatile void* address, int8_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int8_t*>(address) = value;
    }

    void Thread::VolatileWriteInt16(volatile void* address, int16_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int16_t*>(address) = value;
    }

    void Thread::VolatileWriteInt32(volatile void* address, int32_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int32_t*>(address) = value;
    }

    void Thread::VolatileWriteInt64(volatile void* address, int64_t value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile int64_t*>(address) = value;
    }

    void Thread::VolatileWriteFloat(volatile void* address, float value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile float*>(address) = value;
    }

    void Thread::VolatileWriteDouble(volatile void* address, double value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<volatile double*>(address) = value;
    }

    void Thread::VolatileWritePtr(volatile void* address, void* value)
    {
        il2cpp::os::Atomic::FullMemoryBarrier();
        *reinterpret_cast<void* volatile*>(address) = value;
    }

    void Thread::VolatileWriteIntPtr(volatile void* address, intptr_t value)
    {
        VolatileWritePtr(address, reinterpret_cast<void*>(value));
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

    bool Thread::YieldInternal()
    {
        return vm::Thread::YieldInternal();
    }

    bool Thread::JoinInternal(Il2CppThread* _this, int32_t millisecondsTimeout)
    {
        return Join_internal(_this, millisecondsTimeout, NULL);
    }

    int32_t Thread::GetPriorityNative(Il2CppThread* _this)
    {
        return il2cpp::vm::Thread::GetPriority(_this);
    }

    int32_t Thread::SystemMaxStackStize()
    {
        return il2cpp::os::Thread::GetMaxStackSize();
    }

    Il2CppString* Thread::GetName_internal40(Il2CppInternalThread* thread)
    {
        il2cpp::os::FastAutoLock lock(thread->synch_cs);

        if (thread->name_len == 0)
            return NULL;

        return il2cpp::vm::String::NewUtf16(thread->name, thread->name_len);
    }

    Il2CppInternalThread* Thread::CurrentInternalThread_internal()
    {
        return CurrentThread_internal()->GetInternalThread();
    }

    int32_t Thread::GetState40(Il2CppInternalThread* thread)
    {
        // There is a chance that the managed thread object can be used from code (like a
        // finalizer) after it has been destroyed. In that case, the objects that
        // the runtime uses to track this thread may have been freed. Try to check for
        // that case here and return early.
        if (thread == NULL || thread->synch_cs == NULL)
            return vm::kThreadStateStopped;

        il2cpp::os::FastAutoLock lock(thread->synch_cs);
        return (il2cpp::vm::ThreadState)thread->state;
    }

    void Thread::Abort_internal40(Il2CppInternalThread* thread, Il2CppObject* stateInfo)
    {
        il2cpp::vm::Thread::RequestAbort(thread);
    }

    void Thread::ClrState40(Il2CppInternalThread* thread, uint32_t clr)
    {
        il2cpp::vm::Thread::ClrState(thread, (il2cpp::vm::ThreadState)clr);
    }

    void Thread::ConstructInternalThread(Il2CppThread* _this)
    {
        os::Thread* osThread = new os::Thread();

        // Create managed object representing the current thread.

        Il2CppInternalThread* internal = (Il2CppInternalThread*)vm::Object::New(il2cpp_defaults.internal_thread_class);
        internal->state = vm::kThreadStateUnstarted;
        internal->handle = osThread;
        internal->tid = osThread->Id();
        internal->synch_cs = new baselib::ReentrantLock;
        internal->apartment_state = il2cpp::os::kApartmentStateUnknown;
        internal->managed_id = GetNewManagedId_internal();
        os::Atomic::CompareExchangePointer<Il2CppInternalThread>(&_this->internal_thread, internal, NULL);
        il2cpp::gc::GarbageCollector::SetWriteBarrier((void**)&_this->internal_thread);
    }

    void Thread::GetStackTraces(Il2CppArray** threads, Il2CppArray** stack_frames)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Thread::GetStackTraces);
        IL2CPP_UNREACHABLE;
    }

    void Thread::InterruptInternal(Il2CppThread* _this)
    {
        Interrupt_internal(_this);
    }

    void Thread::ResetAbortNative(Il2CppObject* _this)
    {
        vm::Thread::ResetAbort(vm::Thread::CurrentInternal());
    }

    void Thread::ResumeInternal(Il2CppObject* _this)
    {
        NOT_SUPPORTED_IL2CPP(Thread::Resume_internal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

    void Thread::SetName_internal40(Il2CppInternalThread* thread, Il2CppString* name)
    {
        il2cpp::os::FastAutoLock lock(thread->synch_cs);

        // Throw if already set.
        if (thread->name_len != 0)
            il2cpp::vm::Exception::Raise(il2cpp::vm::Exception::GetInvalidOperationException("Thread name can only be set once."));

        // Store name.
        thread->name_len = utils::StringUtils::GetLength(name);
        thread->name = il2cpp::utils::StringUtils::StringDuplicate(utils::StringUtils::GetChars(name), thread->name_len);

        // Hand over to OS layer, if thread has been started already.
        if (thread->handle)
        {
            std::string utf8Name = il2cpp::utils::StringUtils::Utf16ToUtf8(thread->name);
            thread->handle->SetName(utf8Name.c_str());
        }
    }

    void Thread::SetPriorityNative(Il2CppThread* _this, int32_t priority)
    {
        vm::Thread::SetPriority(_this, priority);
    }

    void Thread::SetState40(Il2CppInternalThread* thread, uint32_t state)
    {
        vm::Thread::SetState(thread, (il2cpp::vm::ThreadState)state);
    }

    void Thread::SleepInternal(int32_t millisecondsTimeout)
    {
        Sleep_internal(millisecondsTimeout);
    }

    void Thread::SuspendInternal(Il2CppObject* _this)
    {
        NOT_SUPPORTED_IL2CPP(Thread::SuspendInternal, "Thread suspension is obsolete and not supported on IL2CPP.");
    }

    Il2CppThread* Thread::GetCurrentThread()
    {
        return il2cpp::vm::Thread::Current();
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
