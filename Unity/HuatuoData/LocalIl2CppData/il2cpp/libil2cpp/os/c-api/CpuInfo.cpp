#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/CpuInfo.h"
#include "os/c-api/CpuInfo-c-api.h"

extern "C"
{
    void* UnityPalCpuInfoCreate()
    {
        return il2cpp::os::CpuInfo::Create();
    }

    int32_t UnityPalCpuInfoUsage(void* previous)
    {
        return il2cpp::os::CpuInfo::Usage(previous);
    }
}

#endif
