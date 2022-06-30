#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_ENVIRONMENT && IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <cassert>
#include "os/FileSystemWatcher.h"

namespace il2cpp
{
namespace os
{
namespace FileSystemWatcher
{
    int IsSupported()
    {
        return 0;
    }
}
}
}

#endif
