#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "gc/GarbageCollector.h"
#include "gc/WriteBarrier.h"
#include "icalls/mscorlib/System.IO/MonoIO.h"
#include "os/Directory.h"
#include "os/ErrorCodes.h"
#include "os/File.h"
#include "utils/PathUtils.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Path.h"
#include "vm/String.h"
#include "vm/Exception.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace IO
{
    Il2CppChar MonoIO::get_AltDirectorySeparatorChar(void)
    {
#if IL2CPP_COMPILER_MSVC
        return '/'; /* forward slash */
#else
        return '/'; /* slash, same as DirectorySeparatorChar */
#endif
    }

    bool MonoIO::Close(intptr_t handle, int *error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;

        return il2cpp::os::File::Close(fileHandle, error);
    }

    bool MonoIO::CopyFile(Il2CppString *src, Il2CppString *dest, bool overwrite, int *error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(src->chars));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest->chars));

        return il2cpp::os::File::CopyFile(srcString, destString, overwrite, error);
    }

    bool MonoIO::CopyFile40(Il2CppChar *src, Il2CppChar *dest, bool overwrite, int *error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(src));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest));

        return il2cpp::os::File::CopyFile(srcString, destString, overwrite, error);
    }

    intptr_t MonoIO::get_ConsoleError(void)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdError();
        return ret;
    }

    intptr_t  MonoIO::get_ConsoleInput(void)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdInput();
        return ret;
    }

    intptr_t  MonoIO::get_ConsoleOutput(void)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdOutput();
        return ret;
    }

    bool MonoIO::CreateDirectory(Il2CppString* path, int32_t* error)
    {
        return il2cpp::os::Directory::Create(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), error);
    }

    bool MonoIO::CreateDirectory40(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::Directory::Create(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::DeleteFile(Il2CppString *path, int *error)
    {
        return il2cpp::os::File::DeleteFile(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), error);
    }

    bool MonoIO::DeleteFile40(Il2CppChar *path, int *error)
    {
        return il2cpp::os::File::DeleteFile(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    Il2CppChar MonoIO::get_DirectorySeparatorChar(void)
    {
        return IL2CPP_DIR_SEPARATOR;
    }

    Il2CppString * MonoIO::GetCurrentDirectory(int *error)
    {
        return vm::String::New(il2cpp::os::Directory::GetCurrent(error).c_str());
    }

    int MonoIO::GetFileAttributes(Il2CppString* path, int* error)
    {
        return il2cpp::os::File::GetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), error);
    }

    int MonoIO::GetFileAttributes40(Il2CppChar* path, int* error)
    {
        return il2cpp::os::File::GetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::GetFileStat(Il2CppString* path, FileStat * stat, int32_t* error)
    {
        os::FileStat fileStat;

        const bool ret = il2cpp::os::File::GetFileStat(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), &fileStat, error);

        if (ret)
        {
            stat->attributes = fileStat.attributes;
            stat->length = fileStat.length;
            stat->creation_time = fileStat.creation_time;
            stat->last_access_time = fileStat.last_access_time;
            stat->last_write_time = fileStat.last_write_time;
        }

        return ret;
    }

    bool MonoIO::GetFileStat40(Il2CppChar* path, FileStat * stat, int32_t* error)
    {
        os::FileStat fileStat;

        const bool ret = il2cpp::os::File::GetFileStat(il2cpp::utils::StringUtils::Utf16ToUtf8(path), &fileStat, error);

        if (ret)
        {
            stat->attributes = fileStat.attributes;
            stat->length = fileStat.length;
            stat->creation_time = fileStat.creation_time;
            stat->last_access_time = fileStat.last_access_time;
            stat->last_write_time = fileStat.last_write_time;
        }

        return ret;
    }

    int MonoIO::GetFileType(intptr_t handle, int *error)
    {
        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::GetFileType(h);
    }

    int64_t MonoIO::GetLength(intptr_t handle, int *error)
    {
        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::GetLength(h, error);
    }

    intptr_t MonoIO::Open(Il2CppString *filename, int mode, int access_mode, int share, int options, int *error)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::Open(il2cpp::utils::StringUtils::Utf16ToUtf8(filename->chars), mode, access_mode, share, options, error);
        return ret;
    }

    intptr_t MonoIO::Open40(Il2CppChar *filename, int mode, int access_mode, int share, int options, int *error)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::Open(il2cpp::utils::StringUtils::Utf16ToUtf8(filename), mode, access_mode, share, options, error);
        return ret;
    }

    Il2CppChar MonoIO::get_PathSeparator(void)
    {
#if IL2CPP_COMPILER_MSVC
        return ';'; /* semicolon */
#else
        return ':'; /* colon */
#endif
    }

    int MonoIO::Read(intptr_t handle, Il2CppArray *dest, int dest_offset, int count, int *error)
    {
        IL2CPP_ASSERT(dest != NULL);

        *error = 0; // ERROR_SUCCESS

        if (((uint32_t)dest_offset + count) > il2cpp::vm::Array::GetLength(dest))
            return 0;

        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;

        char *buffer = il2cpp_array_addr(dest, char, dest_offset);

        int bytesRead = il2cpp::os::File::Read(h, buffer, count, error);
        if (*error != 0)
            return -1;
        return bytesRead;
    }

    bool MonoIO::SetCurrentDirectory(Il2CppString* path, int* error)
    {
        return il2cpp::os::Directory::SetCurrent(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), error);
    }

    bool MonoIO::SetCurrentDirectory40(Il2CppChar* path, int* error)
    {
        return il2cpp::os::Directory::SetCurrent(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::SetLength(intptr_t handle, int64_t length, int *error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::SetLength(fileHandle, length, error);
    }

    int64_t MonoIO::Seek(intptr_t handle, int64_t offset, int origin, int *error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Seek(fileHandle, offset, origin, error);
    }

    int MonoIO::Write(intptr_t handle, Il2CppArray * src, int src_offset, int count, int * error)
    {
        IL2CPP_ASSERT(src != NULL);

        *error = 0; // ERROR_SUCCESS

        if ((uint32_t)(src_offset + count) > il2cpp::vm::Array::GetLength(src))
            return 0;

        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        char *buffer = il2cpp_array_addr(src, char, src_offset);

        return il2cpp::os::File::Write(h, buffer, count, error);
    }

    Il2CppChar MonoIO::get_VolumeSeparatorChar(void)
    {
#if IL2CPP_COMPILER_MSVC
        return ':'; /* colon */
#else
        return '/'; /* forward slash */
#endif
    }

    bool MonoIO::RemoveDirectory(Il2CppString* path, MonoIOError* error)
    {
        return il2cpp::os::Directory::Remove(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), error);
    }

    bool MonoIO::RemoveDirectory40(Il2CppChar* path, MonoIOError* error)
    {
        return il2cpp::os::Directory::Remove(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    Il2CppArray* MonoIO::GetFileSystemEntries(Il2CppString* path, Il2CppString* path_with_pattern, int32_t attrs, int32_t mask, MonoIOError* error)
    {
        const std::string pathString(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars));
        const std::string pathPatternString(il2cpp::utils::StringUtils::Utf16ToUtf8(path_with_pattern->chars));

        const std::set<std::string> entries(il2cpp::os::Directory::GetFileSystemEntries(pathString, pathPatternString, attrs, mask, error));

        Il2CppClass *klass = il2cpp::vm::Class::GetArrayClass(il2cpp_defaults.string_class, 1);
        Il2CppArray* array = (Il2CppArray*)il2cpp::vm::Array::NewSpecific(klass, (il2cpp_array_size_t)entries.size());

        size_t index = 0;

        for (std::set<std::string>::const_iterator entry = entries.begin(), end = entries.end(); entry  != end; ++entry)
            il2cpp_array_setref(array, index++, il2cpp::vm::String::New(entry->c_str()));

        return array;
    }

    bool MonoIO::MoveFile(Il2CppString* src, Il2CppString* dest, MonoIOError* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(src->chars));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest->chars));

        return il2cpp::os::File::MoveFile(srcString, destString, error);
    }

    bool MonoIO::MoveFile40(Il2CppChar* src, Il2CppChar* dest, MonoIOError* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(src));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest));

        return il2cpp::os::File::MoveFile(srcString, destString, error);
    }

    bool MonoIO::ReplaceFile(Il2CppString* sourceFileName, Il2CppString* destinationFileName, Il2CppString* destinationBackupFileName, bool ignoreMetadataErrors, MonoIOError* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(sourceFileName->chars));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(destinationFileName->chars));
        const std::string destBackupString(destinationBackupFileName ? il2cpp::utils::StringUtils::Utf16ToUtf8(destinationBackupFileName->chars) : "");

        return il2cpp::os::File::ReplaceFile(srcString, destString, destBackupString, ignoreMetadataErrors, error);
    }

    bool MonoIO::ReplaceFile40(Il2CppChar* sourceFileName, Il2CppChar* destinationFileName, Il2CppChar* destinationBackupFileName, bool ignoreMetadataErrors, MonoIOError* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(sourceFileName));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(destinationFileName));
        const std::string destBackupString(destinationBackupFileName ? il2cpp::utils::StringUtils::Utf16ToUtf8(destinationBackupFileName) : "");

        return il2cpp::os::File::ReplaceFile(srcString, destString, destBackupString, ignoreMetadataErrors, error);
    }

    bool MonoIO::SetFileAttributes(Il2CppString* path, FileAttributes attrs, MonoIOError* error)
    {
        return il2cpp::os::File::SetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path->chars), (UnityPalFileAttributes)attrs, error);
    }

    bool MonoIO::SetFileAttributes40(Il2CppChar* path, FileAttributes attrs, MonoIOError* error)
    {
        return il2cpp::os::File::SetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path), (UnityPalFileAttributes)attrs, error);
    }

    bool MonoIO::Flush(intptr_t handle, MonoIOError* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Flush(fileHandle, error);
    }

    bool MonoIO::SetFileTime(intptr_t handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, MonoIOError* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::SetFileTime(fileHandle, creation_time, last_access_time, last_write_time, error);
    }

    void MonoIO::Lock(intptr_t handle, int64_t position, int64_t length, MonoIOError* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Lock(fileHandle, position, length, error);
    }

    void MonoIO::Unlock(intptr_t handle, int64_t position, int64_t length, MonoIOError* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Unlock(fileHandle, position, length, error);
    }

    bool MonoIO::CreatePipe(intptr_t* read_handle, intptr_t* write_handle)
    {
        MonoIOError error;
        return CreatePipe40(read_handle, write_handle, &error);
    }

    bool MonoIO::DuplicateHandle(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options)
    {
        MonoIOError error;
        return DuplicateHandle40(source_process_handle, source_handle, target_process_handle, target_handle, access, inherit, options, &error);
    }

// This is never called from Mono.
    int32_t MonoIO::GetTempPath(Il2CppString** path)
    {
        const std::string tempPath(il2cpp::vm::Path::GetTempPath());
        *path = vm::String::New(tempPath.c_str());
        return utils::StringUtils::GetLength(*path);
    }

    bool MonoIO::CreatePipe40(intptr_t* read_handle, intptr_t* write_handle, MonoIOError* error)
    {
        il2cpp::os::FileHandle** input = (il2cpp::os::FileHandle**)read_handle;
        il2cpp::os::FileHandle** output = (il2cpp::os::FileHandle**)write_handle;

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
        vm::Exception::Raise(vm::Exception::GetNotSupportedException("Pipes are not supported on WinRT based platforms."));
#else
        return il2cpp::os::File::CreatePipe(input, output, error);
#endif
    }

    bool MonoIO::DuplicateHandle40(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options, MonoIOError* error)
    {
        il2cpp::os::FileHandle* spHandle = (il2cpp::os::FileHandle*)source_process_handle;
        il2cpp::os::FileHandle* sHandle = (il2cpp::os::FileHandle*)source_handle;
        il2cpp::os::FileHandle* tpHandle = (il2cpp::os::FileHandle*)target_process_handle;
        il2cpp::os::FileHandle** tHandle = (il2cpp::os::FileHandle**)target_handle;
        return il2cpp::os::File::DuplicateHandle(spHandle, sHandle, tpHandle, tHandle, access, inherit, options, error);
    }

    bool MonoIO::RemapPath(Il2CppString* path, Il2CppString** newPath)
    {
        *newPath = NULL;
        return false;
    }

    static int32_t CloseFindHandle(os::Directory::FindHandle* findHandle)
    {
        int32_t result = findHandle->CloseOSHandle();

        findHandle->~FindHandle();
        utils::Memory::Free(findHandle);

        return result;
    }

    int32_t MonoIO::FindClose(intptr_t handle)
    {
        return CloseFindHandle(reinterpret_cast<os::Directory::FindHandle*>(handle));
    }

    Il2CppString* PrepareFindResult(os::Directory::FindHandle* findHandle, Il2CppNativeString& fileName, int32_t* resultAttributes, MonoIOError* error)
    {
        while (fileName.empty() || fileName == IL2CPP_NATIVE_STRING(".") || fileName == IL2CPP_NATIVE_STRING(".."))
        {
            os::ErrorCode findError = os::Directory::FindNextFile(findHandle, &fileName, resultAttributes);
            if (findError != os::kErrorCodeSuccess)
            {
                if (findError != os::kErrorCodeNoMoreFiles)
                    *error = findError;

                return NULL;
            }
        }

        // Convert file name to UTF16
        DECLARE_NATIVE_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(fileNameIl2CppChars, fileName);
        DECLARE_NATIVE_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(directoryNameIl2CppChars, findHandle->directoryPath);

        // Allocate result string
        Il2CppString* result = vm::String::NewSize(static_cast<int32_t>(directoryNameIl2CppChars.Length() + fileNameIl2CppChars.Length() + 1));
        Il2CppChar* targetBuffer = utils::StringUtils::GetChars(result);

        // Copy in directory name
        memcpy(targetBuffer, directoryNameIl2CppChars.Str(), sizeof(Il2CppChar) * directoryNameIl2CppChars.Length());

        // Copy in directory separator
        targetBuffer[directoryNameIl2CppChars.Length()] = IL2CPP_DIR_SEPARATOR;

        // Copy in file name
        memcpy(targetBuffer + directoryNameIl2CppChars.Length() + 1, fileNameIl2CppChars.Str(), sizeof(Il2CppChar) * fileNameIl2CppChars.Length());

        *error = os::kErrorCodeSuccess;
        return result;
    }

    Il2CppString* MonoIO::FindFirst(Il2CppString* path, Il2CppString* pathWithPattern, int32_t* resultAttributes, MonoIOError* error, intptr_t* handle)
    {
        DECLARE_IL2CPP_STRING_AS_STRING_VIEW_OF_NATIVE_CHARS(pathWithPatternNative, pathWithPattern);
        os::Directory::FindHandle* findHandle = new(utils::Memory::Malloc(sizeof(os::Directory::FindHandle))) os::Directory::FindHandle(pathWithPatternNative);

        Il2CppNativeString fileName;
        os::ErrorCode findError = os::Directory::FindFirstFile(findHandle, pathWithPatternNative, &fileName, resultAttributes);
        if (findError != os::kErrorCodeSuccess)
        {
            // mscorlib expects no error if we didn't find any files
            if (findError != os::kErrorCodeFileNotFound && findError != os::kErrorCodeNoMoreFiles)
                *error = findError;

            return NULL;
        }

        *handle = reinterpret_cast<intptr_t>(findHandle);
        Il2CppString* result = PrepareFindResult(findHandle, fileName, resultAttributes, error);

        if (result == NULL)
        {
            *handle = 0;
            CloseFindHandle(findHandle);
        }

        return result;
    }

    Il2CppString* MonoIO::FindNext(intptr_t handle, int32_t* resultAttributes, MonoIOError* error)
    {
        Il2CppNativeString fileName;
        os::Directory::FindHandle* findHandle = reinterpret_cast<os::Directory::FindHandle*>(handle);
        return PrepareFindResult(findHandle, fileName, resultAttributes, error);
    }

    void MonoIO::DumpHandles()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(MonoIO::DumpHandles);
        IL2CPP_UNREACHABLE;
    }

    bool MonoIO::FindCloseFile(intptr_t hnd)
    {
        return CloseFindHandle(reinterpret_cast<os::Directory::FindHandle*>(hnd));
    }

    bool MonoIO::FindNextFile(intptr_t hnd, Il2CppString** fileName, int32_t* fileAttr, int32_t* error)
    {
        Il2CppNativeString fileNameNative;
        os::Directory::FindHandle* findHandle = reinterpret_cast<os::Directory::FindHandle*>(hnd);

        while (fileNameNative.empty() || fileNameNative == IL2CPP_NATIVE_STRING(".") || fileNameNative == IL2CPP_NATIVE_STRING(".."))
        {
            os::ErrorCode findError = os::Directory::FindNextFile(findHandle, &fileNameNative, fileAttr);
            if (findError != os::kErrorCodeSuccess)
            {
                *error = findError;
                return false;
            }
        }

        DECLARE_NATIVE_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(fileNameNativeUtf16, fileNameNative);
        *fileName = vm::String::NewUtf16(fileNameNativeUtf16);
        gc::GarbageCollector::SetWriteBarrier((void**)fileName);
        return true;
    }

    intptr_t MonoIO::FindFirstFile(Il2CppChar* path_with_pattern, Il2CppString** fileName, int32_t* fileAttr, int32_t* error)
    {
        DECLARE_IL2CPP_CHAR_PTR_AS_STRING_VIEW_OF_NATIVE_CHARS(pathWithPatternNative, path_with_pattern);
        os::Directory::FindHandle* findHandle = new(utils::Memory::Malloc(sizeof(os::Directory::FindHandle))) os::Directory::FindHandle(pathWithPatternNative);

        Il2CppNativeString fileNameNative;
        os::ErrorCode findError = os::Directory::FindFirstFile(findHandle, pathWithPatternNative, &fileNameNative, fileAttr);
        if (findError != os::kErrorCodeSuccess)
        {
            *error = findError;

            CloseFindHandle(findHandle);
            return 0;
        }

        while (fileNameNative.empty() || fileNameNative == IL2CPP_NATIVE_STRING(".") || fileNameNative == IL2CPP_NATIVE_STRING(".."))
        {
            os::ErrorCode findError = os::Directory::FindNextFile(findHandle, &fileNameNative, fileAttr);
            if (findError != os::kErrorCodeSuccess)
            {
                *error = findError;

                CloseFindHandle(findHandle);
                return 0;
            }
        }

        DECLARE_NATIVE_STRING_AS_STRING_VIEW_OF_IL2CPP_CHARS(fileNameNativeUtf16, fileNameNative);
        *fileName = vm::String::NewUtf16(fileNameNativeUtf16);
        gc::GarbageCollector::SetWriteBarrier((void**)fileName);

        return reinterpret_cast<intptr_t>(findHandle);
    }
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
