#pragma once

#include "il2cpp-object-internals.h"

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
        static int32_t SignalAndWait_Internal(intptr_t toSignal, intptr_t toWaitOn, int32_t ms);
        static int32_t Wait_internal(intptr_t* handles, int32_t numHandles, bool waitAll, int32_t ms);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
