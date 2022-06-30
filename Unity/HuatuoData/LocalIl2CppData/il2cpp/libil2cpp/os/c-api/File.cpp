#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/File.h"
#include "os/c-api/File-c-api.h"
#include "os/c-api/Allocator.h"

extern "C"
{
    int32_t UnityPalIsatty(UnityPalFileHandle* fileHandle)
    {
        return il2cpp::os::File::Isatty(fileHandle).Get();
    }

    UnityPalFileHandle* UnityPalGetStdInput()
    {
        return il2cpp::os::File::GetStdInput();
    }

    UnityPalFileHandle* UnityPalGetStdOutput()
    {
        return il2cpp::os::File::GetStdOutput();
    }

    UnityPalFileHandle* UnityPalGetStdError()
    {
        return il2cpp::os::File::GetStdError();
    }

    int32_t UnityPalCreatePipe(UnityPalFileHandle** read_handle, UnityPalFileHandle** write_handle)
    {
        return il2cpp::os::File::CreatePipe(read_handle, write_handle).Get();
    }

    int32_t UnityPalCreatePipe_With_Error(UnityPalFileHandle** read_handle, UnityPalFileHandle** write_handle, int* error)
    {
        return il2cpp::os::File::CreatePipe(read_handle, write_handle, error).Get();
    }

    FileType UnityPalGetFileType(UnityPalFileHandle* handle)
    {
        return il2cpp::os::File::GetFileType(handle);
    }

    UnityPalFileAttributes UnityPalGetFileAttributes(const char* path, int* error)
    {
        return il2cpp::os::File::GetFileAttributes(path, error);
    }

    int32_t UnityPalSetFileAttributes(const char* path, UnityPalFileAttributes attributes, int* error)
    {
        return il2cpp::os::File::SetFileAttributes(path, attributes, error);
    }

    int32_t UnityPalGetFileStat(const char* path, UnityPalFileStat * stat, int* error)
    {
        il2cpp::os::FileStat cppStat;
        bool result = il2cpp::os::File::GetFileStat(path, &cppStat, error);

        stat->name = Allocator::CopyToAllocatedStringBuffer(cppStat.name);
        stat->attributes = cppStat.attributes;
        stat->creation_time = cppStat.creation_time;
        stat->last_access_time = cppStat.last_access_time;
        stat->last_write_time = cppStat.last_write_time;
        stat->length = cppStat.length;

        return result;
    }

    int32_t UnityPalCopyFile(const char* src, const char* dest, int32_t overwrite, int* error)
    {
        return il2cpp::os::File::CopyFile(src, dest, overwrite, error);
    }

    int32_t UnityPalMoveFile(const char* src, const char* dest, int* error)
    {
        return il2cpp::os::File::MoveFile(src, dest, error);
    }

    int32_t UnityPalDeleteFile(const char* path, int *error)
    {
        return il2cpp::os::File::DeleteFile(path, error);
    }

    int32_t UnityPalReplaceFile(const char* sourceFileName, const char* destinationFileName, const char* destinationBackupFileName, int32_t ignoreMetadataErrors, int* error)
    {
        // It is legal for any of these paramteres to be NULL, need
        // to check to prevent bad NULL ptr issues.

        std::string source;
        std::string dest;
        std::string destbackup;

        if (sourceFileName != NULL)
        {
            source = sourceFileName;
        }

        if (destinationFileName != NULL)
        {
            dest = destinationFileName;
        }

        if (destinationBackupFileName != NULL)
        {
            destbackup = destinationBackupFileName;
        }

        return il2cpp::os::File::ReplaceFile(source, dest, destbackup, ignoreMetadataErrors, error);
    }

    UnityPalFileHandle* UnityPalOpen(const char* path, int openMode, int accessMode, int shareMode, int options, int *error)
    {
        int localError;
        il2cpp::os::FileHandle* handle = il2cpp::os::File::Open(path, openMode, accessMode, shareMode, options, &localError);
        if (error != NULL)
            *error = localError;

        if (localError != il2cpp::os::kErrorCodeSuccess)
            return NULL;

        return handle;
    }

    int32_t UnityPalClose(UnityPalFileHandle* handle, int *error)
    {
        return il2cpp::os::File::Close(handle, error);
    }

    int32_t UnityPalSetFileTime(UnityPalFileHandle* handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int* error)
    {
        return il2cpp::os::File::SetFileTime(handle, creation_time, last_access_time, last_write_time, error);
    }

    int64_t UnityPalGetLength(UnityPalFileHandle* handle, int *error)
    {
        return il2cpp::os::File::GetLength(handle, error);
    }

    int32_t UnityPalTruncate(UnityPalFileHandle* handle, int *error)
    {
        return il2cpp::os::File::Truncate(handle, error);
    }

    int32_t UnityPalSetLength(UnityPalFileHandle* handle, int64_t length, int *error)
    {
        return il2cpp::os::File::SetLength(handle, length, error);
    }

    int64_t UnityPalSeek(UnityPalFileHandle* handle, int64_t offset, int origin, int *error)
    {
        return il2cpp::os::File::Seek(handle, offset, origin, error);
    }

    int UnityPalRead(UnityPalFileHandle* handle, char *dest, int count, int *error)
    {
        return il2cpp::os::File::Read(handle, dest, count, error);
    }

    int32_t UnityPalWrite(UnityPalFileHandle* handle, const char* buffer, int count, int *error)
    {
        return il2cpp::os::File::Write(handle, buffer, count, error);
    }

    int32_t UnityPalFlush(UnityPalFileHandle* handle, int* error)
    {
        return il2cpp::os::File::Flush(handle, error);
    }

    void UnityPalLock(UnityPalFileHandle* handle, int64_t position, int64_t length, int* error)
    {
        return il2cpp::os::File::Lock(handle, position, length, error);
    }

    void UnityPalUnlock(UnityPalFileHandle* handle, int64_t position, int64_t length, int* error)
    {
        return il2cpp::os::File::Unlock(handle, position, length, error);
    }

    int32_t UnityPalDuplicateHandle(UnityPalFileHandle* source_process_handle, UnityPalFileHandle* source_handle, UnityPalFileHandle* target_process_handle,
        UnityPalFileHandle** target_handle, int access, int inherit, int options, int* error)
    {
        return il2cpp::os::File::DuplicateHandle(source_process_handle, source_handle, target_process_handle, target_handle, access, inherit, options, error);
    }

    int32_t UnityPalIsExecutable(const char* filename)
    {
        return il2cpp::os::File::IsExecutable(filename).Get();
    }
}

#endif
