#include "il2cpp-config.h"
#include "os/CrashHelpers.h"

#if IL2CPP_TARGET_POSIX && !IL2CPP_USE_GENERIC_CRASH_HELPERS

namespace il2cpp
{
namespace os
{
    void CrashHelpers::CrashImpl()
    {
        __builtin_trap();
    }
}
}

#endif
