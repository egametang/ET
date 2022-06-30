#include "il2cpp-config.h"

#if IL2CPP_TARGET_DARWIN

#include <sys/types.h>
#if !IL2CPP_TARGET_IOS
#include <libproc.h>
#endif
#include <unistd.h>
#include <stdlib.h>
#include <pthread.h>

#include "os/Thread.h"

namespace il2cpp
{
namespace os
{
    bool Thread::GetCurrentThreadStackBounds(void** low, void** high)
    {
#if !IL2CPP_TARGET_IOS
        pthread_t self = pthread_self();
        *high = pthread_get_stackaddr_np(self);
        size_t stackSize = pthread_get_stacksize_np(self);

        *low = (void*)((uintptr_t)high - stackSize);

        return true;
#else
        return false;
#endif
    }
}
}

#endif
