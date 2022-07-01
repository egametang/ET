#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/Directory.h"
#include "os/ErrorCodes.h"
#include "os/File.h"
#include "os/Posix/Error.h"
#include "utils/DirectoryUtils.h"
#include "utils/Memory.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"
#include <errno.h>
#include <dirent.h>
#include <stdint.h>
#include <stdlib.h>
#include <unistd.h>
#include <sys/errno.h>
#include <sys/stat.h>
#include <sys/types.h>

namespace il2cpp
{
namespace os
{
    std::string Directory::GetCurrent(int *error)
    {
        char buf[PATH_MAX + 1];
        // Note: not all implementations would allocate a buffer when passing 0 to getcwd, as we used to do.
        // this does *not* seem to be part of the POSIX spec:
        // http://pubs.opengroup.org/onlinepubs/000095399/functions/getcwd.html
        char* cwd = getcwd(buf, PATH_MAX + 1);

        if (cwd == NULL)
        {
            *error = FileErrnoToErrorCode(errno);
            return std::string();
        }

        std::string directory(cwd);

        *error = kErrorCodeSuccess;
        return directory;
    }

    bool Directory::SetCurrent(const std::string& path, int *error)
    {
        const int ret = chdir(path.c_str());

        if (ret == -1)
        {
            *error = FileErrnoToErrorCode(errno);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool Directory::Create(const std::string& path, int *error)
    {
        const int ret = mkdir(path.c_str(), 0777);

        if (ret == -1)
        {
            *error = PathErrnoToErrorCode(path, errno);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    bool Directory::Remove(const std::string& path, int *error)
    {
        const int ret = rmdir(path.c_str());

        if (ret == -1)
        {
            *error = PathErrnoToErrorCode(path, errno);
            return false;
        }

        *error = kErrorCodeSuccess;
        return true;
    }

    static void DirectoryGlob(DIR *dir, const std::string& pattern, std::set<std::string>& result)
    {
        if (pattern.empty())
            return;

        std::string matchPattern = il2cpp::utils::CollapseAdjacentStars(pattern);

        struct dirent *entry;

        while ((entry = readdir(dir)) != NULL)
        {
            const std::string filename(entry->d_name);

            if (!il2cpp::utils::Match(filename, matchPattern))
                continue;

            result.insert(filename);
        }
    }

    static bool DirectoryGlob(const std::string& directoryPath, const std::string& pattern, std::set<std::string>& result, int* error)
    {
        DIR* dir = opendir(directoryPath.c_str());

        if (dir == NULL)
        {
            *error = PathErrnoToErrorCode(directoryPath, errno);
            return false;
        }

        DirectoryGlob(dir, pattern, result);

        closedir(dir);

        return true;
    }

    std::set<std::string> Directory::GetFileSystemEntries(const std::string& path, const std::string& pathWithPattern, int32_t attributes, int32_t mask, int* error)
    {
        const std::string directoryPath(il2cpp::utils::PathUtils::DirectoryName(pathWithPattern));
        const std::string pattern(il2cpp::utils::PathUtils::Basename(pathWithPattern));

        std::set<std::string> globResult;

        if (DirectoryGlob(directoryPath, pattern, globResult, error) == false)
            return std::set<std::string>();

        std::set<std::string> result;

        for (std::set<std::string>::const_iterator it = globResult.begin(), end = globResult.end(); it != end; ++it)
        {
            const std::string& filename = *it;

            if (filename == "." || filename == "..")
                continue;

            const std::string path(directoryPath + IL2CPP_DIR_SEPARATOR + filename);

            int attributeError;
            const int32_t pathAttributes = static_cast<int32_t>(File::GetFileAttributes(path, &attributeError));

            if (attributeError != kErrorCodeSuccess)
                continue;

            if ((pathAttributes & mask) == attributes)
                result.insert(path);
        }


        *error = kErrorCodeSuccess;
        return result;
    }

    Directory::FindHandle::FindHandle(const utils::StringView<Il2CppNativeChar>& searchPathWithPattern) :
        osHandle(NULL),
        handleFlags(os::kNoFindHandleFlags)
    {
        directoryPath = il2cpp::utils::PathUtils::DirectoryName(searchPathWithPattern);
        pattern = il2cpp::utils::PathUtils::Basename(searchPathWithPattern);
        pattern = il2cpp::utils::CollapseAdjacentStars(pattern);
    }

    Directory::FindHandle::~FindHandle()
    {
        IL2CPP_ASSERT(osHandle == NULL);
    }

    int32_t Directory::FindHandle::CloseOSHandle()
    {
        int32_t result = os::kErrorCodeSuccess;

        if (osHandle != NULL)
        {
            int32_t ret = closedir(static_cast<DIR*>(osHandle));
            if (ret != 0)
                result = FileErrnoToErrorCode(errno);

            osHandle = NULL;
        }

        return result;
    }

    os::ErrorCode Directory::FindFirstFile(FindHandle* findHandle, const utils::StringView<Il2CppNativeChar>& searchPathWithPattern, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        DIR* dir = opendir(findHandle->directoryPath.c_str());
        if (dir == NULL)
            return PathErrnoToErrorCode(findHandle->directoryPath, errno);

        findHandle->SetOSHandle(dir);
        return FindNextFile(findHandle, resultFileName, resultAttributes);
    }

    os::ErrorCode Directory::FindNextFile(FindHandle* findHandle, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        errno = 0;

        dirent* entry;
        while ((entry = readdir(static_cast<DIR*>(findHandle->osHandle))) != NULL)
        {
            const Il2CppNativeString filename(entry->d_name);

            if (il2cpp::utils::Match(filename, findHandle->pattern))
            {
                const Il2CppNativeString path = utils::PathUtils::Combine(findHandle->directoryPath, filename);

                int attributeError;
                const int32_t pathAttributes = static_cast<int32_t>(File::GetFileAttributes(path, &attributeError));

                if (attributeError == kErrorCodeSuccess)
                {
                    *resultFileName = filename;
                    *resultAttributes = pathAttributes;
                    return os::kErrorCodeSuccess;
                }
            }
        }

        if (errno != 0)
            return FileErrnoToErrorCode(errno);

        return os::kErrorCodeNoMoreFiles;
    }
}
}

#endif
