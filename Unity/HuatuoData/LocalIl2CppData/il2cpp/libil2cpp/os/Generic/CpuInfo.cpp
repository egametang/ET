#include "os/c-api/il2cpp-config-platforms.h"
#if !IL2CPP_PLATFORM_SUPPORTS_CPU_INFO

#include "os/CpuInfo.h"

namespace il2cpp
{
namespace os
{
    void* CpuInfo::Create()
    {
        return NULL;
    }

    int32_t CpuInfo::Usage(void* previous)
    {
        return 0;
    }
}
}

#endif
