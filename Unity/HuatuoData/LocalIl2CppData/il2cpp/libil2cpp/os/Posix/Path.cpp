#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY && !IL2CPP_TARGET_PS4
#include "os/Environment.h"
#include "os/Path.h"
#include <string>

#if defined(__APPLE__)
#include "mach-o/dyld.h"
#include "utils/PathUtils.h"
#elif IL2CPP_TARGET_LINUX || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LUMIN
#include <linux/limits.h>
#include <sys/stat.h>
#include <sys/types.h>
#include <unistd.h>
#include <stdio.h>
#endif

#if IL2CPP_TARGET_LUMIN
namespace il2cpp { namespace os { namespace lumin { extern std::string GetPackageTempPath(); } } }
#endif

namespace il2cpp
{
namespace os
{
#if __ENABLE_UNITY_PLUGIN__
    std::string Path::GetApplicationPath()
    {
        std::string application_path = "";
#if defined(__APPLE__)
        do
        {
            std::string executbale_path = GetExecutablePath();
            if (executbale_path.empty())
            {
                break;
            }
            size_t str_pos = 0;
            if ((str_pos = executbale_path.find_last_of('/')) == std::string::npos)
            {
                break;
            }
            std::string macos_dir = executbale_path.substr(0, str_pos);

            if ((str_pos = macos_dir.find_last_of('/')) == std::string::npos)
            {
                break;
            }
            application_path = macos_dir.substr(0, str_pos);
        }
        while (false);
#endif
        return application_path;
    }

#endif
#if __ENABLE_UNITY_PLUGIN__
    std::string Path::GetFrameworksPath()
    {
        std::string frameworks_path = "";
#if defined(__APPLE__)
        std::string application_path = GetApplicationPath();
        if (!application_path.empty())
        {
            frameworks_path = utils::PathUtils::Combine(application_path, utils::StringView<char>("Frameworks"));
        }
#endif
        return frameworks_path;
    }

#endif
    std::string Path::GetExecutablePath()
    {
#if defined(__APPLE__)
        char path[1024];
        uint32_t size = sizeof(path);
        if (_NSGetExecutablePath(path, &size) == 0)
            return path;

        std::string result;
        result.resize(size + 1);
        _NSGetExecutablePath(&result[0], &size);
        return result;
#elif IL2CPP_TARGET_LINUX || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LUMIN
        char path[PATH_MAX];
        char dest[PATH_MAX + 1];
        //readlink does not null terminate
        memset(dest, 0, PATH_MAX + 1);
        struct stat info;
        pid_t pid = getpid();
        sprintf(path, "/proc/%d/exe", pid);
        if (readlink(path, dest, PATH_MAX) == -1)
            return std::string();
        return dest;
#else
        return std::string();
#endif
    }

    std::string Path::GetTempPath()
    {
        static const char* tmpdirs[] = { "TMPDIR", "TMP", "TEMP", NULL};

        for (size_t i = 0; tmpdirs[i] != NULL; ++i)
        {
            std::string tmpdir = Environment::GetEnvironmentVariable(tmpdirs[i]);

            if (!tmpdir.empty())
                return tmpdir;
        }

#if IL2CPP_TARGET_ANDROID
        return std::string("/data/local/tmp");
#elif IL2CPP_TARGET_LUMIN
        return il2cpp::os::lumin::GetPackageTempPath();
#else
        return std::string("/tmp");
#endif
    }

    bool Path::IsAbsolute(const std::string& path)
    {
        return path[0] == '/';
    }
}
}

#endif
