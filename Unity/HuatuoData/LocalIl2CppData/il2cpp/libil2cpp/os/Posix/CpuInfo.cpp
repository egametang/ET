#include "os/c-api/il2cpp-config-platforms.h"
#if IL2CPP_PLATFORM_SUPPORTS_CPU_INFO

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/CpuInfo.h"
#include <stdlib.h>
#include <stdio.h>
#include "utils/Memory.h"
#include "os/Time.h"
#include "os/Environment.h"


#include <sys/resource.h>
#include <sys/param.h>
#if IL2CPP_TARGET_DARWIN
#include <sys/sysctl.h>
#endif

#include <time.h>

#if IL2CPP_TARGET_LINUX
#include <sys/time.h>
#include <sys/resource.h>
#endif

struct Il2CppCpuUsageState
{
    int64_t kernel_time;
    int64_t user_time;
    int64_t current_time;
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

        struct rusage resource_usage;
        int64_t current_time;
        int64_t kernel_time;
        int64_t user_time;

        if (getrusage(RUSAGE_SELF, &resource_usage) == -1)
        {
            return -1;
        }

        current_time = os::Time::GetTicks100NanosecondsMonotonic();
        kernel_time = resource_usage.ru_stime.tv_sec * 1000 * 1000 * 10 + resource_usage.ru_stime.tv_usec * 10;
        user_time = resource_usage.ru_utime.tv_sec * 1000 * 1000 * 10 + resource_usage.ru_utime.tv_usec * 10;

        cpu_busy_time = (user_time - (prev ? prev->user_time : 0)) + (kernel_time - (prev ? prev->kernel_time : 0));
        cpu_total_time = (current_time - (prev ? prev->current_time : 0)) * il2cpp::os::Environment::GetProcessorCount();

        if (prev)
        {
            prev->kernel_time = kernel_time;
            prev->user_time = user_time;
            prev->current_time = current_time;
        }

        if (cpu_total_time > 0 && cpu_busy_time > 0)
            cpu_usage = (int32_t)(cpu_busy_time * 100 / cpu_total_time);

        return cpu_usage;
    }
}
}

#endif
#endif
