#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/Path.h"
#include "utils/PathUtils.h"
#include "Allocator.h"

extern "C"
{
    const char* UnityPalGetTempPath()
    {
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::os::Path::GetTempPath());
    }

    const char* UnityPalGetExecutablePath()
    {
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::os::Path::GetExecutablePath());
    }

    int32_t UnityPalIsAbsolutePath(const char* path)
    {
        if (path == NULL)
            return 0;
        std::string path_string = path;
        return il2cpp::os::Path::IsAbsolute(path_string);
    }

    char* UnityPalBasename(const char* path)
    {
        if (path == NULL)
            return NULL;
        std::string pathString = path;
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::utils::PathUtils::Basename(pathString));
    }

    char* UnityPalDirectoryName(const char* path)
    {
        if (path == NULL)
            return NULL;
        std::string pathString = path;
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::utils::PathUtils::DirectoryName(pathString));
    }
}

#endif
