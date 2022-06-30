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

    typedef int32_t MutexRights;

    class LIBIL2CPP_CODEGEN_API Mutex
    {
    public:
        static bool ReleaseMutex_internal(intptr_t handle);
        static intptr_t CreateMutex_icall(bool initiallyOwned, Il2CppChar* name, int32_t name_length, bool* created);
        static intptr_t OpenMutex_icall(Il2CppChar* name, int32_t name_length, int32_t rights, int32_t* error);
    };
} /* namespace Threading */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
