#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_THREAD

#include "os/Thread.h"

namespace il2cpp
{
namespace os
{
    bool Thread::GetCurrentThreadStackBounds(void** low, void** high)
    {
        return false;
    }
}
}

#endif
