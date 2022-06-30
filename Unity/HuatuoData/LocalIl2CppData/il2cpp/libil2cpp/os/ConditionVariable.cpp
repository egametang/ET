#include "il2cpp-config.h"
#include "os/ConditionVariable.h"
#include "os/Mutex.h"

#if IL2CPP_SUPPORT_THREADS

#if IL2CPP_THREADS_WIN32
#include "os/Win32/ConditionVariableImpl.h"
#elif IL2CPP_THREADS_PTHREAD
#include "os/Posix/ConditionVariableImpl.h"
#else
#include "os/ConditionVariableImpl.h"
#endif


namespace il2cpp
{
namespace os
{
    ConditionVariable::ConditionVariable()
        : m_ConditionVariable(new ConditionVariableImpl())
    {
    }

    ConditionVariable::~ConditionVariable()
    {
        delete m_ConditionVariable;
    }

    int ConditionVariable::Wait(FastMutex* lock)
    {
        return m_ConditionVariable->Wait(lock->GetImpl());
    }

    int ConditionVariable::TimedWait(FastMutex* lock, uint32_t timeout_ms)
    {
        return m_ConditionVariable->TimedWait(lock->GetImpl(), timeout_ms);
    }

    void ConditionVariable::Broadcast()
    {
        m_ConditionVariable->Broadcast();
    }

    void ConditionVariable::Signal()
    {
        m_ConditionVariable->Signal();
    }
}
}

#else

namespace il2cpp
{
namespace os
{
    ConditionVariable::ConditionVariable()
    {
    }

    ConditionVariable::~ConditionVariable()
    {
    }

    int ConditionVariable::Wait(FastMutex* lock)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return 0;
    }

    int ConditionVariable::TimedWait(FastMutex* lock, uint32_t timeout_ms)
    {
        IL2CPP_ASSERT(0 && "Threads are not enabled for this platform.");
        return 0;
    }

    void ConditionVariable::Broadcast()
    {
    }

    void ConditionVariable::Signal()
    {
    }
}
}

#endif
