#pragma once

#if IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT

namespace il2cpp
{
namespace os
{
    class SynchronizationContext
    {
    public:
        static Il2CppObject* GetForCurrentThread();
        static void Post(Il2CppObject* context, SynchronizationContextCallback callback, intptr_t arg);
    };
}
}

#endif
