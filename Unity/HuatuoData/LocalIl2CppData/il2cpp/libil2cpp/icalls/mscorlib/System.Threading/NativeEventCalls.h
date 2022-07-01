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

        static intptr_t CreateEvent_internal(bool manual, bool initial, Il2CppString* name, int32_t* errorCode);
        static intptr_t OpenEvent_internal(Il2CppString* name, EventWaitHandleRights rights, int32_t* errorCode);

        static bool ResetEvent_internal(intptr_t handlePtr);
        static bool SetEvent_internal(intptr_t handlePtr);
        static void CloseEvent_internal(intptr_t handlePtr);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
