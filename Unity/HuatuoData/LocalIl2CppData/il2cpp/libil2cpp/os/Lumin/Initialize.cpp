#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include "os/Initialize.h"

namespace il2cpp
{ namespace os
{ namespace lumin
{
    extern void LifecycleInit();
    extern void InitializeFileHandles();
    extern void CleanupFileHandles();

namespace automation { extern void WaitForAppThread(); }
}}}

void il2cpp::os::Initialize()
{
    il2cpp::os::lumin::LifecycleInit();
    il2cpp::os::lumin::InitializeFileHandles();
}

void il2cpp::os::Uninitialize()
{
    il2cpp::os::lumin::CleanupFileHandles();
#if IL2CPP_TARGET_LUMIN_AUTOMATION
    il2cpp::os::lumin::automation::WaitForAppThread();
#endif
}

#endif
