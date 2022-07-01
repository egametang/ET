#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppString;

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Threading
{
    class LIBIL2CPP_CODEGEN_API Semaphore
    {
    public:
        static intptr_t CreateSemaphore_internal40(int32_t initialCount, int32_t maximumCount, Il2CppString* name, int32_t* errorCode);
        static bool ReleaseSemaphore_internal40(intptr_t handlePtr, int32_t releaseCount, int32_t* previousCount);
        static intptr_t OpenSemaphore_internal(Il2CppString* name, int32_t rights, int32_t* error);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
