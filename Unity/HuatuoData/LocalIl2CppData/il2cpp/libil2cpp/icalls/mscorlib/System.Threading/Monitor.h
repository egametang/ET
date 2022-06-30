#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
struct Il2CppObject;

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
    class LIBIL2CPP_CODEGEN_API Monitor
    {
    public:
        static bool Monitor_test_owner(Il2CppObject* obj);
        static bool Monitor_test_synchronised(Il2CppObject* obj);
        static bool Monitor_wait(Il2CppObject* obj, int32_t ms);
        static void Enter(Il2CppObject* obj);
        static void Exit(Il2CppObject* obj);
        static void Monitor_pulse(Il2CppObject* obj);
        static void Monitor_pulse_all(Il2CppObject* obj);
        static void try_enter_with_atomic_var(Il2CppObject* obj, int32_t millisecondsTimeout, bool* lockTaken);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
