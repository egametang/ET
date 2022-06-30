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
#include "vm/Profiler.h"
#include "vm/String.h"
#include "vm/Exception.h"
#include "utils/dynamic_array.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

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
    bool MonoIO::Cancel_internal(intptr_t handle, int32_t* error)
    {
        return il2cpp::os::File::Cancel((il2cpp::os::FileHandle*)handle);
    }

    bool MonoIO::Close(intptr_t handle, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;

        return il2cpp::os::File::Close(fileHandle, error);
    }

    bool MonoIO::CopyFile(Il2CppChar* path, Il2CppChar* dest, bool overwrite, int32_t* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(path));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest));

        return il2cpp::os::File::CopyFile(srcString, destString, overwrite, error);
    }

    bool MonoIO::CreateDirectory(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::Directory::Create(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::CreatePipe(intptr_t* read_handle, intptr_t* write_handle, int32_t* error)
    {
        il2cpp::os::FileHandle** input = (il2cpp::os::FileHandle**)read_handle;
        il2cpp::os::FileHandle** output = (il2cpp::os::FileHandle**)write_handle;

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
        vm::Exception::Raise(vm::Exception::GetNotSupportedException("Pipes are not supported on WinRT based platforms."));
#else
        auto result = il2cpp::os::File::CreatePipe(input, output, error);
        vm::Exception::RaiseIfError(result.GetError());
        return result.Get();
#endif
    }

    bool MonoIO::DeleteFile(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::File::DeleteFile(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::DuplicateHandle(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options, int32_t* error)
    {
        il2cpp::os::FileHandle* spHandle = (il2cpp::os::FileHandle*)source_process_handle;
        il2cpp::os::FileHandle* sHandle = (il2cpp::os::FileHandle*)source_handle;
        il2cpp::os::FileHandle* tpHandle = (il2cpp::os::FileHandle*)target_process_handle;
        il2cpp::os::FileHandle** tHandle = (il2cpp::os::FileHandle**)target_handle;
        return il2cpp::os::File::DuplicateHandle(spHandle, sHandle, tpHandle, tHandle, access, inherit, options, error);
    }

    static utils::dynamic_array<os::Directory::FindHandle*> s_OpenFindHandles;
    static baselib::ReentrantLock s_OpenFindHandlesMutex;

    static int32_t CloseFindHandle(os::Directory::FindHandle* findHandle)
    {
        int32_t result = findHandle->CloseOSHandle();

        findHandle->~FindHandle();
        utils::Memory::Free(findHandle);

        return result;
    }

    bool MonoIO::FindCloseFile(intptr_t hnd)
    {
        auto possibleFindHandle = reinterpret_cast<os::Directory::FindHandle*>(hnd);

        // Manually managed the mutex here because we don't want to hold the lock during a call to
        // CloseOSFindHandleDirectly, as that call can be expensive.
        s_OpenFindHandlesMutex.Acquire();

        auto knownFindHandle = std::find(s_OpenFindHandles.begin(), s_OpenFindHandles.end(), possibleFindHandle);
        if (knownFindHandle == s_OpenFindHandles.end())
        {
            s_OpenFindHandlesMutex.Release();
            // We did not find the handle in the list of ones the VM allocated - assume it is
            // a directly allocated OS handle.
            return os::Directory::CloseOSFindHandleDirectly(hnd);
        }
        else
        {
            s_OpenFindHandles.erase(knownFindHandle);
            s_OpenFindHandlesMutex.Release();
            return CloseFindHandle(possibleFindHandle);
        }
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

    bool MonoIO::Flush(intptr_t handle, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Flush(fileHandle, error);
    }

    bool MonoIO::GetFileStat(Il2CppChar* path, FileStat* stat, int32_t* error)
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

    bool MonoIO::MoveFile(Il2CppChar* path, Il2CppChar* dest, int32_t* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(path));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(dest));

        return il2cpp::os::File::MoveFile(srcString, destString, error);
    }

    bool MonoIO::RemapPath(Il2CppString* path, Il2CppString** newPath)
    {
        *newPath = NULL;
        return false;
    }

    bool MonoIO::RemoveDirectory(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::Directory::Remove(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::ReplaceFile(Il2CppChar* sourceFileName, Il2CppChar* destinationFileName, Il2CppChar* destinationBackupFileName, bool ignoreMetadataErrors, int32_t* error)
    {
        const std::string srcString(il2cpp::utils::StringUtils::Utf16ToUtf8(sourceFileName));
        const std::string destString(il2cpp::utils::StringUtils::Utf16ToUtf8(destinationFileName));
        const std::string destBackupString(destinationBackupFileName ? il2cpp::utils::StringUtils::Utf16ToUtf8(destinationBackupFileName) : "");

        return il2cpp::os::File::ReplaceFile(srcString, destString, destBackupString, ignoreMetadataErrors, error);
    }

    bool MonoIO::SetCurrentDirectory(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::Directory::SetCurrent(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    bool MonoIO::SetFileAttributes(Il2CppChar* path, int32_t attrs, int32_t* error)
    {
        return il2cpp::os::File::SetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path), (UnityPalFileAttributes)attrs, error);
    }

    bool MonoIO::SetFileTime(intptr_t handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::SetFileTime(fileHandle, creation_time, last_access_time, last_write_time, error);
    }

    bool MonoIO::SetLength(intptr_t handle, int64_t length, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::SetLength(fileHandle, length, error);
    }

    Il2CppChar MonoIO::get_AltDirectorySeparatorChar()
    {
        return '/'; /* slash, same as DirectorySeparatorChar */
    }

    Il2CppChar MonoIO::get_DirectorySeparatorChar()
    {
        return IL2CPP_DIR_SEPARATOR;
    }

    Il2CppChar MonoIO::get_PathSeparator()
    {
#if IL2CPP_TARGET_WINDOWS
        return ';'; /* semicolon */
#else
        return ':'; /* colon */
#endif
    }

    Il2CppChar MonoIO::get_VolumeSeparatorChar()
    {
#if IL2CPP_TARGET_WINDOWS
        return ':'; /* colon */
#else
        return '/'; /* forward slash */
#endif
    }

    int32_t MonoIO::Read(intptr_t handle, Il2CppArray* dest, int32_t dest_offset, int32_t count, int32_t* error)
    {
        IL2CPP_ASSERT(dest != NULL);

        *error = 0; // ERROR_SUCCESS

        if (((uint32_t)dest_offset + count) > il2cpp::vm::Array::GetLength(dest))
            return 0;

        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;

        char *buffer = il2cpp_array_addr(dest, char, dest_offset); \

        int bytesRead = il2cpp::os::File::Read(h, buffer, count, error);

#if IL2CPP_ENABLE_PROFILER
        if (vm::Profiler::ProfileFileIO())
            vm::Profiler::FileIO(IL2CPP_PROFILE_FILEIO_READ, bytesRead);
#endif
        if (*error != 0)
            return -1;
        return bytesRead;
    }

    int32_t MonoIO::Write(intptr_t handle, Il2CppArray* src, int32_t src_offset, int32_t count, int32_t* error)
    {
        IL2CPP_ASSERT(src != NULL);

        *error = 0; // ERROR_SUCCESS

        if ((uint32_t)(src_offset + count) > il2cpp::vm::Array::GetLength(src))
            return 0;

        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        char *buffer = il2cpp_array_addr(src, char, src_offset);

        int bytesWritten = il2cpp::os::File::Write(h, buffer, count, error);

#if IL2CPP_ENABLE_PROFILER
        if (vm::Profiler::ProfileFileIO())
            vm::Profiler::FileIO(IL2CPP_PROFILE_FILEIO_WRITE, bytesWritten);
#endif

        return bytesWritten;
    }

    int64_t MonoIO::GetLength(intptr_t handle, int32_t* error)
    {
        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::GetLength(h, error);
    }

    int64_t MonoIO::Seek(intptr_t handle, int64_t offset, int32_t origin, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Seek(fileHandle, offset, origin, error);
    }

    intptr_t MonoIO::FindFirstFile(Il2CppChar* pathWithPattern, Il2CppString** fileName, int32_t* fileAttr, int32_t* error)
    {
        DECLARE_IL2CPP_CHAR_PTR_AS_STRING_VIEW_OF_NATIVE_CHARS(pathWithPatternNative, pathWithPattern);
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

        // Keep track of the handles we allocated, so we can tell later if this is an OS handle
        // or one we allocated.
        os::FastAutoLock lock(&s_OpenFindHandlesMutex);
        s_OpenFindHandles.push_back(findHandle);

        return reinterpret_cast<intptr_t>(findHandle);
    }

    intptr_t MonoIO::get_ConsoleError()
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdError();
        return ret;
    }

    intptr_t MonoIO::get_ConsoleInput()
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdInput();
        return ret;
    }

    intptr_t MonoIO::get_ConsoleOutput()
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::GetStdOutput();
        return ret;
    }

    intptr_t MonoIO::Open(Il2CppChar* filename, int32_t mode, int32_t access, int32_t share, int32_t options, int32_t* error)
    {
        intptr_t ret;
        ret = (intptr_t)il2cpp::os::File::Open(il2cpp::utils::StringUtils::Utf16ToUtf8(filename), mode, access, share, options, error);
        return ret;
    }

    int32_t MonoIO::GetFileAttributes(Il2CppChar* path, int32_t* error)
    {
        return il2cpp::os::File::GetFileAttributes(il2cpp::utils::StringUtils::Utf16ToUtf8(path), error);
    }

    int32_t MonoIO::GetFileType(intptr_t handle, int32_t* error)
    {
        il2cpp::os::FileHandle* h = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::GetFileType(h);
    }

    Il2CppString* MonoIO::GetCurrentDirectory(int32_t* error)
    {
        return vm::String::New(il2cpp::os::Directory::GetCurrent(error).c_str());
    }

    void MonoIO::DumpHandles()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(MonoIO::DumpHandles);
        IL2CPP_UNREACHABLE;
    }

    void MonoIO::Lock(intptr_t handle, int64_t position, int64_t length, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Lock(fileHandle, position, length, error);
    }

    void MonoIO::Unlock(intptr_t handle, int64_t position, int64_t length, int32_t* error)
    {
        il2cpp::os::FileHandle* fileHandle = (il2cpp::os::FileHandle*)handle;
        return il2cpp::os::File::Unlock(fileHandle, position, length, error);
    }
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
