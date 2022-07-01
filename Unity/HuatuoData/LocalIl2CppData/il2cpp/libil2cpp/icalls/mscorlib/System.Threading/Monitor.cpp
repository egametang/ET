#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "icalls/mscorlib/System.Threading/Monitor.h"
#include "vm/Monitor.h"
#include "vm/Exception.h"

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
    void Monitor::Enter(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        il2cpp::vm::Monitor::Enter(obj);
    }

    void Monitor::Exit(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        il2cpp::vm::Monitor::Exit(obj);
    }

    bool Monitor::Monitor_test_synchronised(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        return il2cpp::vm::Monitor::IsAcquired(obj);
    }

    void Monitor::Monitor_pulse(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        il2cpp::vm::Monitor::Pulse(obj);
    }

    void Monitor::Monitor_pulse_all(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        il2cpp::vm::Monitor::PulseAll(obj);
    }

    bool Monitor::Monitor_wait(Il2CppObject* obj, int32_t ms)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        return il2cpp::vm::Monitor::TryWait(obj, ms);
    }

    bool Monitor::Monitor_try_enter(Il2CppObject* obj, int32_t ms)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        return il2cpp::vm::Monitor::TryEnter(obj, ms);
    }

    bool Monitor::Monitor_test_owner(Il2CppObject* obj)
    {
        IL2CPP_CHECK_ARG_NULL(obj);
        return il2cpp::vm::Monitor::IsAcquired(obj);
    }

    void Monitor::enter_with_atomic_var(Il2CppObject* obj, bool* lockTaken)
    {
        IL2CPP_CHECK_ARG_NULL(obj);

        if (*lockTaken)
            vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("lockTaken", "lockTaken must be false"));

        il2cpp::vm::Monitor::Enter(obj);
        *lockTaken = true;
    }

    void Monitor::try_enter_with_atomic_var(Il2CppObject* obj, int32_t millisecondsTimeout, bool* lockTaken)
    {
        if (*lockTaken)
            vm::Exception::Raise(il2cpp::vm::Exception::GetArgumentException("lockTaken", "lockTaken must be false"));

        *lockTaken = Monitor_try_enter(obj, millisecondsTimeout);
    }
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
