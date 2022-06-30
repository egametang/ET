#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_CRASH_HELPERS

#include "os/CrashHelpers.h"

#include <cstdlib>

namespace il2cpp
{
namespace os
{
    void CrashHelpers::CrashImpl()
    {
        abort();
    }
}
}

#endif
