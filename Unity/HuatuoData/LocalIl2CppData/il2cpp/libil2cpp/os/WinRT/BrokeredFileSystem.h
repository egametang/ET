#pragma once

#if IL2CPP_TARGET_WINRT

#include "il2cpp-string-types.h"
#include "os/c-api/OSGlobalEnums.h"
#include "os/Directory.h"
#include "os/ErrorCodes.h"
#include "os/File.h"

#include <cstdint>
#include <set>

namespace il2cpp
{
namespace os
{
    class BrokeredFileSystem
    {
    public:
        // Folders
        static int CreateDirectoryW(const UTF16String& path);
        static int RemoveDirectoryW(const UTF16String& path);
        static std::set<std::string> GetFileSystemEntries(const UTF16String& path, const UTF16String& pathWithPattern, int32_t attributes, int32_t attributeMask, int* error);
        static os::ErrorCode FindFirstFileW(Directory::FindHandle* findHandle, const utils::StringView<Il2CppNativeChar>& searchPathWithPattern, Il2CppNativeString* resultFileName, int32_t* resultAttributes);
        static os::ErrorCode FindNextFileW(Directory::FindHandle* findHandle, Il2CppNativeString* resultFileName, int32_t* resultAttributes);
        static os::ErrorCode FindClose(void* osHandle);

        // Files
        static bool CopyFileW(const UTF16String& source, const UTF16String& destination, bool overwrite, int* error);
        static bool MoveFileW(const UTF16String& source, const UTF16String& destination, int* error);
        static int DeleteFileW(const UTF16String& path);
        static UnityPalFileAttributes GetFileAttributesW(const UTF16String& path, int* error);
        static bool SetFileAttributesW(const UTF16String& path, UnityPalFileAttributes attributes, int* error);
        static bool GetFileStat(const std::string& utf8Path, const UTF16String& path, FileStat* stat, int* error);
        static FileHandle* Open(const UTF16String& path, uint32_t desiredAccess, uint32_t shareMode, uint32_t creationDisposition, uint32_t flagsAndAttributes, int* error);

        // Cleanup
        static void CleanupStatics();
    };
}
}

#endif // IL2CPP_TARGET_WINRT
