#pragma once

#include "il2cpp-object-internals.h"

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
        static bool ReleaseSemaphore_internal(intptr_t handle, int32_t releaseCount, int32_t* previousCount);
        static intptr_t CreateSemaphore_icall(int32_t initialCount, int32_t maximumCount, Il2CppChar* name, int32_t name_length, int32_t* errorCode);
        static intptr_t OpenSemaphore_icall(Il2CppChar* name, int32_t name_length, int32_t rights, int32_t* errorCode);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
