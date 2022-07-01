#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHeaders.h"

#undef FindFirstFile
#undef FindNextFile

#include "os/Directory.h"
#include "os/ErrorCodes.h"
#include "utils/StringUtils.h"
#include "utils/PathUtils.h"

#if IL2CPP_TARGET_WINRT
#include "os/WinRT/BrokeredFileSystem.h"
#endif

static inline int DirectoryWin32ErrorToErrorCode(DWORD win32ErrorCode)
{
    return win32ErrorCode;
}

using namespace il2cpp::utils::PathUtils;

namespace il2cpp
{
namespace os
{
    std::string Directory::GetCurrent(int *error)
    {
        UTF16String buf;
        int len, res_len;

        len = MAX_PATH + 1;
        buf.resize(len, 0);

        *error = ERROR_SUCCESS;

        res_len = ::GetCurrentDirectory(len, (LPWSTR)buf.c_str());
        if (res_len > len) /*buf is too small.*/
        {
            int old_res_len = res_len;
            buf.resize(res_len, 0);
            res_len = ::GetCurrentDirectory(res_len, (LPWSTR)buf.c_str()) == old_res_len;
        }

        std::string directory;

        if (res_len)
        {
            len = 0;
            while (buf[len])
                ++len;

            directory = il2cpp::utils::StringUtils::Utf16ToUtf8(buf.c_str());
        }
        else
        {
            *error = DirectoryWin32ErrorToErrorCode(::GetLastError());
        }

        return directory;
    }

    bool Directory::SetCurrent(const std::string& path, int* error)
    {
        *error = kErrorCodeSuccess;

        const UTF16String utf16Path(il2cpp::utils::StringUtils::Utf8ToUtf16(path.c_str()));
        if (::SetCurrentDirectory((LPWSTR)utf16Path.c_str()))
            return true;

        *error = DirectoryWin32ErrorToErrorCode(::GetLastError());
        return false;
    }

    bool Directory::Create(const std::string& path, int *error)
    {
        *error = kErrorCodeSuccess;

        const UTF16String utf16Path(il2cpp::utils::StringUtils::Utf8ToUtf16(path.c_str()));
        if (::CreateDirectory((LPWSTR)utf16Path.c_str(), NULL))
            return true;

        auto lastError = ::GetLastError();

#if IL2CPP_TARGET_WINRT
        if (lastError == ERROR_ACCESS_DENIED)
        {
            *error = BrokeredFileSystem::CreateDirectoryW(utf16Path);
            return *error == kErrorCodeSuccess;
        }
#endif

        *error = DirectoryWin32ErrorToErrorCode(lastError);
        return false;
    }

    bool Directory::Remove(const std::string& path, int *error)
    {
        *error = kErrorCodeSuccess;

        const UTF16String utf16Path(il2cpp::utils::StringUtils::Utf8ToUtf16(path.c_str()));
        if (::RemoveDirectory((LPWSTR)utf16Path.c_str()))
            return true;

        auto lastError = ::GetLastError();

#if IL2CPP_TARGET_WINRT
        if (lastError == ERROR_ACCESS_DENIED)
        {
            *error = BrokeredFileSystem::RemoveDirectoryW(utf16Path);
            return *error == kErrorCodeSuccess;
        }
#endif

        *error = DirectoryWin32ErrorToErrorCode(lastError);
        return false;
    }

    std::set<std::string> Directory::GetFileSystemEntries(const std::string& path, const std::string& pathWithPattern, int32_t attrs, int32_t mask, int* error)
    {
        *error = kErrorCodeSuccess;
        std::set<std::string> files;
        WIN32_FIND_DATA ffd;
        const UTF16String utf16Path(il2cpp::utils::StringUtils::Utf8ToUtf16(pathWithPattern));

        HANDLE handle = ::FindFirstFileExW((LPCWSTR)utf16Path.c_str(), FindExInfoStandard, &ffd, FindExSearchNameMatch, NULL, 0);
        if (INVALID_HANDLE_VALUE == handle)
        {
            auto lastError = ::GetLastError();

#if IL2CPP_TARGET_WINRT
            if (lastError == ERROR_ACCESS_DENIED)
                return BrokeredFileSystem::GetFileSystemEntries(utils::StringUtils::Utf8ToUtf16(path), utf16Path, attrs, mask, error);
#endif

            // Following the Mono implementation, do not treat a directory with no files as an error.
            int errorCode = DirectoryWin32ErrorToErrorCode(lastError);
            if (errorCode != ERROR_FILE_NOT_FOUND)
                *error = errorCode;
            return files;
        }

        do
        {
            const std::string fileName(il2cpp::utils::StringUtils::Utf16ToUtf8(ffd.cFileName));

            if ((fileName.length() == 1 && fileName.at(0) == '.') ||
                (fileName.length() == 2 && fileName.at(0) == '.' && fileName.at(1) == '.'))
                continue;

            if ((ffd.dwFileAttributes & mask) == attrs)
            {
                files.insert(Combine(path, fileName));
            }
        }
        while (::FindNextFileW(handle, &ffd) != 0);

        ::FindClose(handle);

        return files;
    }

    Directory::FindHandle::FindHandle(const utils::StringView<Il2CppNativeChar>& searchPathWithPattern) :
        osHandle(INVALID_HANDLE_VALUE),
        handleFlags(os::kNoFindHandleFlags),
        directoryPath(il2cpp::utils::PathUtils::DirectoryName(searchPathWithPattern)),
        pattern(il2cpp::utils::PathUtils::Basename(searchPathWithPattern))
    {
    }

    Directory::FindHandle::~FindHandle()
    {
        IL2CPP_ASSERT(osHandle == INVALID_HANDLE_VALUE);
    }

    int32_t Directory::FindHandle::CloseOSHandle()
    {
        int32_t result = os::kErrorCodeSuccess;

        if (osHandle != INVALID_HANDLE_VALUE)
        {
#if IL2CPP_TARGET_WINRT
            if (handleFlags & kUseBrokeredFileSystem)
            {
                result = BrokeredFileSystem::FindClose(osHandle);
            }
            else
#endif
            {
                result = ::FindClose(osHandle);
            }

            osHandle = INVALID_HANDLE_VALUE;
        }

        return result;
    }

    os::ErrorCode Directory::FindFirstFile(FindHandle* findHandle, const utils::StringView<Il2CppNativeChar>& searchPathWithPattern, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
        WIN32_FIND_DATA findData;
        HANDLE handle = FindFirstFileExW(searchPathWithPattern.Str(), FindExInfoStandard, &findData, FindExSearchNameMatch, NULL, 0);

        if (handle != INVALID_HANDLE_VALUE)
        {
            findHandle->SetOSHandle(handle);
            *resultFileName = findData.cFileName;
            *resultAttributes = findData.dwFileAttributes;
            return os::kErrorCodeSuccess;
        }
        else
        {
            auto lastError = GetLastError();

#if IL2CPP_TARGET_WINRT
            if (lastError == ERROR_ACCESS_DENIED)
                return BrokeredFileSystem::FindFirstFileW(findHandle, searchPathWithPattern, resultFileName, resultAttributes);
#endif

            return static_cast<os::ErrorCode>(lastError);
        }
    }

    os::ErrorCode Directory::FindNextFile(FindHandle* findHandle, Il2CppNativeString* resultFileName, int32_t* resultAttributes)
    {
#if IL2CPP_TARGET_WINRT
        if (findHandle->handleFlags & kUseBrokeredFileSystem)
            return BrokeredFileSystem::FindNextFileW(findHandle, resultFileName, resultAttributes);
#endif

        WIN32_FIND_DATA findData;
        if (FindNextFileW(findHandle->osHandle, &findData) == FALSE)
            return static_cast<os::ErrorCode>(GetLastError());

        *resultFileName = findData.cFileName;
        *resultAttributes = findData.dwFileAttributes;
        return os::kErrorCodeSuccess;
    }
}
}

#endif
