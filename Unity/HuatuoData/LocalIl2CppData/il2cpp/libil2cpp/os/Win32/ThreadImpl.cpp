#include "il2cpp-config.h"

#if !IL2CPP_THREADS_STD && IL2CPP_THREADS_WIN32

#include "ThreadImpl.h"
#include "os/ThreadLocalValue.h"
#include "os/Time.h"
#include "WindowsHelpers.h"
#include "il2cpp-vm-support.h"

namespace il2cpp
{
namespace os
{
    struct ThreadImplStartData
    {
        Thread::StartFunc m_StartFunc;
        void* m_StartArg;
        volatile DWORD* m_ThreadId;
    };

    static DWORD WINAPI ThreadStartWrapper(LPVOID arg)
    {
        ThreadImplStartData startData = *(ThreadImplStartData*)arg;
        free(arg);
        *startData.m_ThreadId = GetCurrentThreadId();
        startData.m_StartFunc(startData.m_StartArg);
        return 0;
    }

    ThreadImpl::ThreadImpl()
        : m_ThreadHandle(0), m_ThreadId(0), m_StackSize(IL2CPP_DEFAULT_STACK_SIZE), m_ApartmentState(kApartmentStateUnknown), m_Priority(kThreadPriorityNormal)
    {
    }

    ThreadImpl::~ThreadImpl()
    {
        if (m_ThreadHandle != NULL)
            CloseHandle(m_ThreadHandle);
    }

    size_t ThreadImpl::Id()
    {
        return m_ThreadId;
    }

    void ThreadImpl::SetName(const char* name)
    {
        // http://msdn.microsoft.com/en-us/library/xcb2z8hs.aspx

        const DWORD MS_VC_EXCEPTION = 0x406D1388;

    #pragma pack(push,8)
        typedef struct tagTHREADNAME_INFO
        {
            DWORD dwType; // Must be 0x1000.
            LPCSTR szName; // Pointer to name (in user addr space).
            DWORD dwThreadID; // Thread ID (-1=caller thread).
            DWORD dwFlags; // Reserved for future use, must be zero.
        } THREADNAME_INFO;
    #pragma pack(pop)

        THREADNAME_INFO info;
        info.dwType = 0x1000;
        info.szName = name;
        info.dwThreadID = static_cast<DWORD>(Id());
        info.dwFlags = 0;

        __try
        {
            RaiseException(MS_VC_EXCEPTION, 0, sizeof(info) / sizeof(ULONG_PTR), (ULONG_PTR*)&info);
        }
        __except (EXCEPTION_EXECUTE_HANDLER)
        {
        }
    }

    void ThreadImpl::SetPriority(ThreadPriority priority)
    {
        if (m_ThreadHandle == NULL)
            m_Priority = priority;
        else
        {
            int ret = ::SetThreadPriority(m_ThreadHandle, priority - 2);
            IL2CPP_ASSERT(ret);
        }
    }

    ThreadPriority ThreadImpl::GetPriority()
    {
        if (m_ThreadHandle == NULL)
            return m_Priority;
        int ret = ::GetThreadPriority(m_ThreadHandle) + 2;
        IL2CPP_ASSERT(ret != THREAD_PRIORITY_ERROR_RETURN);
        return (ThreadPriority)ret;
    }

    ErrorCode ThreadImpl::Run(Thread::StartFunc func, void* arg, int64_t affinityMask)
    {
        // It might happen that func will start executing and will try to access m_ThreadId before CreateThread gets a chance to assign it.
        // Therefore m_ThreadId is assigned both by this thread and from the newly created thread (race condition could go the other way too).

        ThreadImplStartData* startData = (ThreadImplStartData*)malloc(sizeof(ThreadImplStartData));
        startData->m_StartFunc = func;
        startData->m_StartArg = arg;
        startData->m_ThreadId = &m_ThreadId;

        // Create thread.
        DWORD threadId;
        HANDLE threadHandle = ::CreateThread(NULL, m_StackSize, &ThreadStartWrapper, startData, STACK_SIZE_PARAM_IS_A_RESERVATION, &threadId);

        if (!threadHandle)
            return kErrorCodeGenFailure;

#if IL2CPP_TARGET_WINDOWS_GAMES || IL2CPP_TARGET_XBOXONE
        if (affinityMask != Thread::kThreadAffinityAll)
            SetThreadAffinityMask(threadHandle, static_cast<DWORD_PTR>(affinityMask));
#endif

        m_ThreadHandle = threadHandle;
        m_ThreadId = threadId;

        return kErrorCodeSuccess;
    }

    void ThreadImpl::Sleep(uint32_t ms, bool interruptible)
    {
        uint32_t remainingWaitTime = ms;
        while (true)
        {
            uint32_t startWaitTime = os::Time::GetTicksMillisecondsMonotonic();
            DWORD sleepResult = ::SleepEx(remainingWaitTime, interruptible);

            if (sleepResult == WAIT_IO_COMPLETION)
            {
                uint32_t waitedTime = os::Time::GetTicksMillisecondsMonotonic() - startWaitTime;
                if (waitedTime >= remainingWaitTime)
                    return;

                remainingWaitTime -= waitedTime;
                continue;
            }

            break;
        }
    }

    void ThreadImpl::QueueUserAPC(Thread::APCFunc func, void* context)
    {
        ::QueueUserAPC(reinterpret_cast<PAPCFUNC>(func), m_ThreadHandle, reinterpret_cast<ULONG_PTR>(context));
    }

    int ThreadImpl::GetMaxStackSize()
    {
        return INT_MAX;
    }

namespace
{
    // It would be nice to always use CoGetApartmentType but it's only available on Windows 7 and later.
    // That's why we check for function at runtime and do a fallback on Windows XP.
    // CoGetApartmentType is always available in Windows Store Apps.

    typedef HRESULT (STDAPICALLTYPE * CoGetApartmentTypeFunc)(APTTYPE* type, APTTYPEQUALIFIER* qualifier);

    ApartmentState GetApartmentWindows7(CoGetApartmentTypeFunc coGetApartmentType, bool* implicit)
    {
        *implicit = false;

        APTTYPE type;
        APTTYPEQUALIFIER qualifier;
        const HRESULT hr = coGetApartmentType(&type, &qualifier);
        if (FAILED(hr))
        {
            IL2CPP_ASSERT(CO_E_NOTINITIALIZED == hr);
            return kApartmentStateUnknown;
        }

        switch (type)
        {
            case APTTYPE_STA:
            case APTTYPE_MAINSTA:
                return kApartmentStateInSTA;

            case APTTYPE_MTA:
                *implicit = (APTTYPEQUALIFIER_IMPLICIT_MTA == qualifier);
                return kApartmentStateInMTA;

            case APTTYPE_NA:
                switch (qualifier)
                {
                    case APTTYPEQUALIFIER_NA_ON_STA:
                    case APTTYPEQUALIFIER_NA_ON_MAINSTA:
                        return kApartmentStateInSTA;

                    case APTTYPEQUALIFIER_NA_ON_MTA:
                        return kApartmentStateInMTA;

                    case APTTYPEQUALIFIER_NA_ON_IMPLICIT_MTA:
                        *implicit = true;
                        return kApartmentStateInMTA;
                }
                break;
        }

        IL2CPP_ASSERT(0 && "CoGetApartmentType returned unexpected value.");
        return kApartmentStateUnknown;
    }

#if IL2CPP_TARGET_WINDOWS_DESKTOP

    ApartmentState GetApartmentWindowsXp(bool* implicit)
    {
        *implicit = false;

        IUnknown* context = nullptr;
        HRESULT hr = CoGetContextToken(reinterpret_cast<ULONG_PTR*>(&context));
        if (SUCCEEDED(hr))
        {
            IComThreadingInfo* info;
            hr = context->QueryInterface(&info);
            if (SUCCEEDED(hr))
            {
                THDTYPE type;
                hr = info->GetCurrentThreadType(&type);
                if (SUCCEEDED(hr))
                {
                    // THDTYPE_PROCESSMESSAGES means that we are in STA thread.
                    // Otherwise it's an MTA thread. We are not sure at this moment if CoInitializeEx has been called explicitly on this thread
                    // or if it has been implicitly made MTA by a CoInitialize call on another thread.
                    if (THDTYPE_PROCESSMESSAGES == type)
                        return kApartmentStateInSTA;

                    // Assume implicit. Even if it's explicit, we'll handle the case correctly by checking CoInitializeEx return value.
                    *implicit = true;
                    return kApartmentStateInMTA;
                }

                info->Release();
            }

            // No need to release context.
        }

        return kApartmentStateUnknown;
    }

    class CoGetApartmentTypeHelper
    {
    private:
        HMODULE _library;
        CoGetApartmentTypeFunc _func;

    public:
        inline CoGetApartmentTypeHelper()
        {
            _library = LoadLibraryW(L"ole32.dll");
            Assert(_library);
            _func = reinterpret_cast<CoGetApartmentTypeFunc>(GetProcAddress(_library, "CoGetApartmentType"));
        }

        inline ~CoGetApartmentTypeHelper()
        {
            FreeLibrary(_library);
        }

        inline CoGetApartmentTypeFunc GetFunc() const { return _func; }
    };

    inline ApartmentState GetApartmentImpl(bool* implicit)
    {
        static CoGetApartmentTypeHelper coGetApartmentTypeHelper;
        const CoGetApartmentTypeFunc func = coGetApartmentTypeHelper.GetFunc();
        return func ? GetApartmentWindows7(func, implicit) : GetApartmentWindowsXp(implicit);
    }

#else

    inline ApartmentState GetApartmentImpl(bool* implicit)
    {
        return GetApartmentWindows7(CoGetApartmentType, implicit);
    }

#endif
}

    ApartmentState ThreadImpl::GetApartment()
    {
        Assert(GetCurrentThreadId() == m_ThreadId);

        ApartmentState state = static_cast<ApartmentState>(m_ApartmentState & ~kApartmentStateCoInitialized);

        if (kApartmentStateUnknown == state)
        {
            bool implicit;
            state = GetApartmentImpl(&implicit);
            if (!implicit)
                m_ApartmentState = state;
        }

        return state;
    }

    ApartmentState ThreadImpl::GetExplicitApartment()
    {
        return static_cast<ApartmentState>(m_ApartmentState & ~kApartmentStateCoInitialized);
    }

    ApartmentState ThreadImpl::SetApartment(ApartmentState state)
    {
        Assert(GetCurrentThreadId() == m_ThreadId);

        // Unknown state uninitializes COM.
        if (kApartmentStateUnknown == state)
        {
            if (m_ApartmentState & kApartmentStateCoInitialized)
            {
                CoUninitialize();
                m_ApartmentState = kApartmentStateUnknown;
            }

            return GetApartment();
        }

        // Initialize apartment state. Ignore result of this function because it will return MTA value for both implicit and explicit apartment.
        // On the other hand m_ApartmentState will only be set to MTA if it was initialized explicitly with CoInitializeEx.
        GetApartment();

        ApartmentState currentState = static_cast<ApartmentState>(m_ApartmentState & ~kApartmentStateCoInitialized);

        if (kApartmentStateUnknown != currentState)
        {
            Assert(state == currentState);
            return currentState;
        }

#if IL2CPP_TARGET_XBOXONE
        if (state == kApartmentStateInSTA)
        {
            // Only assert in debug.. we wouldn't want to bring down the application in Release config
            IL2CPP_ASSERT(false && "STA apartment state is not supported on Xbox One");
            state = kApartmentStateInMTA;
        }
#endif

        HRESULT hr = CoInitializeEx(nullptr, (kApartmentStateInSTA == state) ? COINIT_APARTMENTTHREADED : COINIT_MULTITHREADED);
        if (SUCCEEDED(hr))
        {
            m_ApartmentState = state;
            if (S_OK == hr)
                m_ApartmentState = static_cast<ApartmentState>(m_ApartmentState | kApartmentStateCoInitialized);
            else
                CoUninitialize(); // Someone has already called correct CoInitialize. Don't leave incorrect reference count.
        }
        else if (RPC_E_CHANGED_MODE == hr)
        {
            // CoInitialize has already been called with a different apartment state.
            m_ApartmentState = (kApartmentStateInSTA == state) ? kApartmentStateInMTA : kApartmentStateInSTA;
        }
        else
        {
            IL2CPP_VM_RAISE_COM_EXCEPTION(hr, true);
        }

        return GetApartment();
    }

    void ThreadImpl::SetExplicitApartment(ApartmentState state)
    {
        Assert(!(m_ApartmentState & kApartmentStateCoInitialized));
        m_ApartmentState = state;
    }

    size_t ThreadImpl::CurrentThreadId()
    {
        return GetCurrentThreadId();
    }

    ThreadImpl* ThreadImpl::CreateForCurrentThread()
    {
        ThreadImpl* thread = new ThreadImpl();
        BOOL duplicateResult = DuplicateHandle(::GetCurrentProcess(), ::GetCurrentThread(), ::GetCurrentProcess(), &thread->m_ThreadHandle, 0, FALSE, DUPLICATE_SAME_ACCESS);
        Assert(duplicateResult && "DuplicateHandle failed.");
        thread->m_ThreadId = ::GetCurrentThreadId();
        return thread;
    }

    bool ThreadImpl::YieldInternal()
    {
        return SwitchToThread();
    }

#if IL2CPP_HAS_NATIVE_THREAD_CLEANUP

    static Thread::ThreadCleanupFunc s_ThreadCleanupFunction;
    static ThreadLocalValue s_ThreadCleanupArguments;

    void ThreadImpl::SetNativeThreadCleanup(Thread::ThreadCleanupFunc cleanupFunction)
    {
        s_ThreadCleanupFunction = cleanupFunction;
    }

    void ThreadImpl::RegisterCurrentThreadForCleanup(void* arg)
    {
        s_ThreadCleanupArguments.SetValue(arg);
    }

    void ThreadImpl::UnregisterCurrentThreadForCleanup()
    {
        s_ThreadCleanupArguments.SetValue(NULL);
    }

    void ThreadImpl::OnCurrentThreadExiting()
    {
        Thread::ThreadCleanupFunc cleanupFunction = s_ThreadCleanupFunction;
        if (cleanupFunction == NULL)
            return;

        void* threadCleanupArgument = NULL;
        s_ThreadCleanupArguments.GetValue(&threadCleanupArgument);

        if (threadCleanupArgument != NULL)
            cleanupFunction(threadCleanupArgument);
    }

#endif
}
}

#endif
