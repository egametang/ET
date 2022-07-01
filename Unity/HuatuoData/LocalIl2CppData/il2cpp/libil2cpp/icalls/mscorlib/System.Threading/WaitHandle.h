#pragma once

#include <stdint.h>
#include "il2cpp-object-internals.h"
#include "il2cpp-config.h"

struct Il2CppArray;
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
    class LIBIL2CPP_CODEGEN_API WaitHandle
    {
    public:
        static bool SignalAndWait_Internal(intptr_t toSignal, intptr_t toWaitOn, int32_t ms, bool exitContext);
        static bool WaitAll_internal(Il2CppArray* handles, int32_t ms, bool exitContext);
        static int32_t WaitAny_internal(Il2CppArray* handles, int32_t ms, bool exitContext);
        static bool WaitOne_internal(Il2CppObject* unused, intptr_t handlePtr, int32_t ms, bool exitContext);
        static int32_t SignalAndWait_Internal40(intptr_t toSignal, intptr_t toWaitOn, int32_t ms);
        static int32_t Wait_internal(void* *handles, int32_t numhandles, bool waitall, int32_t timeout);

    private:
        static const int m_waitIntervalMs = 10;
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
