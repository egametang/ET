#pragma once

#include <stdint.h>
#include <vector>
#include <string>
#include "il2cpp-config.h"
#include "os/Thread.h"
#include "utils/NonCopyable.h"

struct MethodInfo;

struct Il2CppArray;
struct Il2CppDomain;
struct Il2CppObject;
struct Il2CppThread;
struct Il2CppInternalThread;
struct Il2CppString;

namespace il2cpp
{
namespace vm
{
// System.Threading.ThreadState
    enum ThreadState
    {
        kThreadStateRunning = 0x00000000,
        kThreadStateStopRequested = 0x00000001,
        kThreadStateSuspendRequested = 0x00000002,
        kThreadStateBackground = 0x00000004,
        kThreadStateUnstarted = 0x00000008,
        kThreadStateStopped = 0x00000010,
        kThreadStateWaitSleepJoin = 0x00000020,
        kThreadStateSuspended = 0x00000040,
        kThreadStateAbortRequested = 0x00000080,
        kThreadStateAborted = 0x00000100,

        // This enum is used with the ~ operator to clear values. to avoid undefined
        // behavior in C++, the cleared state of each value should also be present
        // in the enum.
        kThreadStateRunningCleared = ~kThreadStateRunning,
        kThreadStateStopRequestedCleared = ~kThreadStateStopRequested,
        kThreadStateSuspendRequestedCleared = ~kThreadStateSuspendRequested,
        kThreadStateBackgroundCleared = ~kThreadStateBackground,
        kThreadStateUnstartedCleared = ~kThreadStateUnstarted,
        kThreadStateStoppedCleared = ~kThreadStateStopped,
        kThreadStateWaitSleepJoinCleared = ~kThreadStateWaitSleepJoin,
        kThreadStateSuspendedCleared = ~kThreadStateSuspended,
        kThreadStateAbortRequestedCleared = ~kThreadStateAbortRequested,
        kThreadStateAbortedCleared = ~kThreadStateAborted,
    };


// System.Threading.ApartmentState
    enum ThreadApartmentState
    {
        kThreadApartmentStateSTA = 0x00000000,
        kThreadApartmentStateMTA = 0x00000001,
        kThreadApartmentStateUnknown = 0x00000002
    };


    class LIBIL2CPP_CODEGEN_API Thread
    {
    public:
        static std::string GetName(Il2CppInternalThread* thread);
        static void SetName(Il2CppThread* thread, Il2CppString* name);
        static void SetName(Il2CppInternalThread* thread, Il2CppString* name);
        static Il2CppThread* Current();
        static Il2CppThread* Attach(Il2CppDomain *domain);
        static void Detach(Il2CppThread *thread);
        static void WalkFrameStack(Il2CppThread *thread, Il2CppFrameWalkFunc func, void *user_data);
        static Il2CppThread** GetAllAttachedThreads(size_t &size);
        static void AbortAllThreads();
        static Il2CppThread* Main();
        static bool IsVmThread(Il2CppThread *thread);
        static uint64_t GetId(Il2CppThread *thread);
        static uint64_t GetId(Il2CppInternalThread* thread);

        static void RequestInterrupt(Il2CppThread* thread);
        static void CheckCurrentThreadForInterruptAndThrowIfNecessary();

        static bool RequestAbort(Il2CppThread* thread);
        static void CheckCurrentThreadForAbortAndThrowIfNecessary();
        static void ResetAbort(Il2CppThread* thread);
        static bool RequestAbort(Il2CppInternalThread* thread);
        static void ResetAbort(Il2CppInternalThread* thread);
        static void SetPriority(Il2CppThread* thread, int32_t priority);
        static int32_t GetPriority(Il2CppThread* thread);

        struct NativeThreadAbortException {};

    public:
        // internal
        static void Initialize();
        static void Uninitialize();

        static void AdjustStaticData();
        static int32_t AllocThreadStaticData(int32_t size);
        static void FreeThreadStaticData(Il2CppThread *thread);
        static void* GetThreadStaticData(int32_t offset);
        static void* GetThreadStaticDataForThread(int32_t offset, Il2CppThread* thread);
        static void* GetThreadStaticDataForThread(int32_t offset, Il2CppInternalThread* thread);

        static void Register(Il2CppThread *thread);
        static void Unregister(Il2CppThread *thread);

        static void SetupInternalManagedThread(Il2CppThread* thread, os::Thread* osThread);

        /// Initialize and register thread.
        /// NOTE: Must be called on thread!
        static void InitializeManagedThread(Il2CppThread *thread, Il2CppDomain* domain);
        static void UninitializeManagedThread(Il2CppThread *thread);

        static void SetMain(Il2CppThread* thread);

        static void SetState(Il2CppThread *thread, ThreadState value);
        static ThreadState GetState(Il2CppThread *thread);
        static void ClrState(Il2CppThread* thread, ThreadState clr);

        static void FullMemoryBarrier();

        static int32_t GetNewManagedId();

        static Il2CppInternalThread* CurrentInternal();

        static void ClrState(Il2CppInternalThread* thread, ThreadState clr);
        static void SetState(Il2CppInternalThread *thread, ThreadState value);
        static ThreadState GetState(Il2CppInternalThread *thread);
        static bool TestState(Il2CppInternalThread* thread, ThreadState value);

        static Il2CppInternalThread* CreateInternal(void(*func)(void*), void* arg, bool threadpool_thread, uint32_t stack_size);

        static void Stop(Il2CppInternalThread* thread);

        static void Sleep(uint32_t ms);

        static bool YieldInternal();

        static void SetDefaultAffinityMask(int64_t affinityMask);

    private:
        static Il2CppThread* s_MainThread;
    };

    class ThreadStateSetter : il2cpp::utils::NonCopyable
    {
    public:
        ThreadStateSetter(ThreadState state) : m_State(state)
        {
            m_Thread = il2cpp::vm::Thread::Current();
            Thread::SetState(m_Thread, m_State);
        }

        ~ThreadStateSetter()
        {
            Thread::ClrState(m_Thread, m_State);
        }

    private:
        ThreadState m_State;
        Il2CppThread* m_Thread;
    };
} /* namespace vm */
} /* namespace il2cpp */
