#include "il2cpp-config.h"
#include "OSSpecificSynchronizationContext.h"
#include "os/SynchronizationContext.h"

using il2cpp::icalls::mscorlib::System::Threading::OSSpecificSynchronizationContext;

Il2CppObject* OSSpecificSynchronizationContext::GetOSContext()
{
#if IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT
    return il2cpp::os::SynchronizationContext::GetForCurrentThread();
#else
    return NULL;
#endif
}

void OSSpecificSynchronizationContext::PostInternal(Il2CppObject* context, SynchronizationContextCallback callback, intptr_t arg)
{
#if IL2CPP_HAS_OS_SYNCHRONIZATION_CONTEXT
    il2cpp::os::SynchronizationContext::Post(context, callback, arg);
#endif
}
