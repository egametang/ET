#include "os/c-api/il2cpp-config-platforms.h"
#if IL2CPP_PLATFORM_SUPPORTS_CPU_INFO

#if IL2CPP_TARGET_WINDOWS

#include "os/CpuInfo.h"
#include "utils/Memory.h"

#include "WindowsHeaders.h"

struct Il2CppCpuUsageState
{
    uint64_t kernel_time;
    uint64_t user_time;
    uint64_t idle_time;
};

namespace il2cpp
{
namespace os
{
    void* CpuInfo::Create()
    {
        return IL2CPP_MALLOC_ZERO(sizeof(Il2CppCpuUsageState));
    }

    int32_t CpuInfo::Usage(void* previous)
    {
        Il2CppCpuUsageState* prev = (Il2CppCpuUsageState*)previous;
        int32_t cpu_usage = 0;
        int64_t cpu_total_time;
        int64_t cpu_busy_time;

        uint64_t idle_time;
        uint64_t kernel_time;
        uint64_t user_time;

        ::GetSystemTimes((FILETIME*)&idle_time, (FILETIME*)&kernel_time, (FILETIME*)&user_time);

        cpu_total_time = (int64_t)((user_time - (prev ? prev->user_time : 0)) + (kernel_time - (prev ? prev->kernel_time : 0)));
        cpu_busy_time = (int64_t)(cpu_total_time - (idle_time - (prev ? prev->idle_time : 0)));

        if (prev)
        {
            prev->idle_time = idle_time;
            prev->kernel_time = kernel_time;
            prev->user_time = user_time;
        }

        if (cpu_total_time > 0 && cpu_busy_time > 0)
            cpu_usage = (int32_t)(cpu_busy_time * 100 / cpu_total_time);

        return cpu_usage;
    }
}
}

#endif
#endif
