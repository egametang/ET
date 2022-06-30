#pragma once

#include <stdint.h>
#include "il2cpp-object-internals.h"
#include "il2cpp-config.h"

struct Il2CppString;

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
    struct MonoIOError;

    typedef int32_t EventWaitHandleRights;

    class LIBIL2CPP_CODEGEN_API NativeEventCalls
    {
    public:
        static bool ResetEvent_internal(intptr_t handle);
        static bool SetEvent_internal(intptr_t handle);
        static intptr_t CreateEvent_icall(bool manual, bool initial, Il2CppChar* name, int32_t name_length, int32_t* errorCode);
        static void CloseEvent_internal(intptr_t handle);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
