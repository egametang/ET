#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include <sys/sysinfo.h>
#include <sys/time.h>
#include <sys/types.h>
#include <unistd.h>
#include <stdio.h>

#include "os/Process.h"
#include "utils/Logging.h"

#include "il2cpp-vm-support.h"

struct ProcessHandle
{
    pid_t pid;
};

namespace il2cpp
{
namespace os
{
    const int _sc_clk_tck = sysconf(_SC_CLK_TCK);

    typedef struct
    {
        char name[256];
        unsigned long utime;
        unsigned long stime;
        unsigned long long starttime;
    } ProcessStatInfo;

    typedef struct
    {
        unsigned vmpeak;
        unsigned vmsize;
        unsigned vmhwm;
        unsigned vmrss;
        unsigned vmdata;
        unsigned vmstk;
        unsigned vmswap;
    } ProcessStatusInfo;

    static bool GetProcessStatInfo(int pid, ProcessStatInfo *result)
    {
        char buffer[4096], *open_paren, *close_paren;
        FILE *fp;

        sprintf(buffer, "/proc/%d/stat", pid);
        fp = fopen(buffer, "r");
        if (fp == NULL)
        {
            return false;
        }

        fgets(buffer, sizeof(buffer) - 1, fp);
        fclose(fp);

        buffer[sizeof(buffer) - 1] = 0;

        open_paren = strchr(buffer, '(');
        if (open_paren == NULL)
        {
            return false;
        }

        close_paren = strrchr(open_paren + 1, ')');
        if (close_paren == NULL)
        {
            return false;
        }

        *close_paren = 0;
        strncpy(result->name, open_paren + 1, sizeof(result->name) - 1);
        result->name[sizeof(result->name) - 1] = 0;

        sscanf(close_paren + 1, " %*c %*d %*d %*d %*d %*d %*u %*lu %*lu %*lu %*lu %lu %lu %*ld %*ld %*ld %*ld %*ld %*ld %llu",
            &result->utime,
            &result->stime,
            &result->starttime);

        return true;
    }

    #define GPSI_STARTSWITH(s) (strncasecmp(buffer, s, sizeof(s) - 1) == 0)

    static bool GetProcessStatusInfo(int pid, ProcessStatusInfo *result)
    {
        char buffer[1024], *p;
        FILE *fp;

        sprintf(buffer, "/proc/%d/status", pid);
        fp = fopen(buffer, "r");
        if (fp == NULL)
        {
            return false;
        }

        memset(result, 0, sizeof(*result));
        while (fgets(buffer, sizeof(buffer) - 1, fp))
        {
            buffer[sizeof(buffer) - 1] = 0;

            if (GPSI_STARTSWITH("VmPeak:"))
            {
                sscanf(buffer + sizeof("VmPeak:") - 1, "%u", &result->vmpeak);
            }
            else if (GPSI_STARTSWITH("VmSize:"))
            {
                sscanf(buffer + sizeof("VmSize:") - 1, "%u", &result->vmsize);
            }
            else if (GPSI_STARTSWITH("VmHWM:"))
            {
                sscanf(buffer + sizeof("VmHWM:") - 1, "%u", &result->vmhwm);
            }
            else if (GPSI_STARTSWITH("VmRSS:"))
            {
                sscanf(buffer + sizeof("VmRSS:") - 1, "%u", &result->vmrss);
            }
            else if (GPSI_STARTSWITH("VmData:"))
            {
                sscanf(buffer + sizeof("VmData:") - 1, "%u", &result->vmdata);
            }
            else if (GPSI_STARTSWITH("VmStk:"))
            {
                sscanf(buffer + sizeof("VmStk:") - 1, "%u", &result->vmstk);
            }
            else if (GPSI_STARTSWITH("VmSwap:"))
            {
                sscanf(buffer + sizeof("VmSwap:") - 1, "%u", &result->vmswap);
            }
        }

        fclose(fp);

        return true;
    }

    #undef GPSI_STARTSWITH

    int Process::GetCurrentProcessId()
    {
        return getpid();
    }

    ProcessHandle* Process::GetProcess(int processId)
    {
        // If/when we implement the CreateProcess_internal icall we will likely
        // need to so something smarter here to find the process if we did
        // not create it and return a known pseudo-handle. For now this
        // is sufficient though.
        return (ProcessHandle*)(intptr_t)processId;
    }

    void Process::FreeProcess(ProcessHandle* handle)
    {
        // We have nothing to do here.
    }

    std::string Process::GetProcessName(ProcessHandle* handle)
    {
        intptr_t pid = (intptr_t)handle;
        ProcessStatInfo psi;

        if (GetProcessStatInfo(pid, &psi))
        {
            return std::string(psi.name);
        }
        else
        {
            return std::string();
        }
    }

    int64_t Process::Times(ProcessHandle* handle, int32_t type)
    {
        int pid = (intptr_t)handle;
        ProcessStatInfo psi;
        int64_t result = 0;

        if (GetProcessStatInfo(pid, &psi))
        {
            result = (int64_t)psi.utime * (10000000 / _sc_clk_tck);
            if (type == 2) // Total process time
            {
                result += (int64_t)psi.stime * (10000000 / _sc_clk_tck);
            }
        }

        return result;
    }

    int64_t Process::StartTime(ProcessHandle* handle)
    {
        int pid = (intptr_t)handle;
        ProcessStatInfo psi;

        if (GetProcessStatInfo(pid, &psi))
        {
            struct sysinfo si;
            if (sysinfo(&si) == 0)
            {
                const int64_t unix_time_to_filetime = 11644473600ll;
                int64_t boot_time = (int64_t)time(NULL) - si.uptime + unix_time_to_filetime;
                return (boot_time + psi.starttime / _sc_clk_tck) * 10000000ll;
            }
        }

        return 0;
    }

    int64_t Process::GetProcessData(int32_t pid, int32_t data_type, int32_t* error)
    {
        ProcessStatusInfo psi;

        if (GetProcessStatusInfo(pid, &psi))
        {
            switch (data_type)
            {
                case 4: // WorkingSet64
                    return (int64_t)psi.vmrss * 1024;

                case 5: // PeakWorkingSet64
                    return (int64_t)psi.vmhwm * 1024;

                case 6: // PrivateMemorySize64
                    return (int64_t)(psi.vmdata + psi.vmstk) * 1024;

                case 7: // VirtualMemorySize64
                    return (int64_t)psi.vmsize * 1024;

                case 8: // PeakVirtualMemorySize64
                    return (int64_t)psi.vmpeak * 1024;

                default:
                {
                    char buf[256];
                    sprintf(buf, "Process::GetProcessData: Unknown data_type %d", data_type);
                    utils::Logging::Write(buf);
                    break;
                }
            }
        }

        return 0;
    }
}
}

#endif
