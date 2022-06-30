#pragma once
#include "il2cpp-config.h"


#include <stdint.h>
#include <string>
#include "os/ErrorCodes.h"
#include "os/c-api/OSGlobalEnums.h"
#include "utils/Expected.h"

#undef CopyFile
#undef DeleteFile
#undef MoveFile
#undef ReplaceFile
#undef GetFileAttributes
#undef SetFileAttributes
#undef CreatePipe

namespace il2cpp
{
namespace os
{
    // File enums and structs
    struct FileHandle;

    struct FileStat
    {
        std::string name;
        int32_t attributes;
        int64_t length;
        int64_t creation_time;
        int64_t last_access_time;
        int64_t last_write_time;
    };

    class LIBIL2CPP_CODEGEN_API File
    {
    public:


        static utils::Expected<bool> Isatty(FileHandle* fileHandle);
        static FileHandle* GetStdInput();
        static FileHandle* GetStdOutput();
        static FileHandle* GetStdError();
        static utils::Expected<bool> CreatePipe(FileHandle** read_handle, FileHandle** write_handle);
        static utils::Expected<bool> CreatePipe(FileHandle** read_handle, FileHandle** write_handle, int* error);
        static FileType GetFileType(FileHandle* handle);
        static UnityPalFileAttributes GetFileAttributes(const std::string& path, int* error);
        static bool SetFileAttributes(const std::string& path, UnityPalFileAttributes attributes, int* error);
        static bool GetFileStat(const std::string& path, FileStat * stat, int* error);
        static bool CopyFile(const std::string& src, const std::string& dest, bool overwrite, int* error);
        static bool MoveFile(const std::string& src, const std::string& dest, int* error);
        static bool DeleteFile(const std::string& path, int *error);
        static bool ReplaceFile(const std::string& sourceFileName, const std::string& destinationFileName, const std::string& destinationBackupFileName, bool ignoreMetadataErrors, int* error);
        static FileHandle* Open(const std::string& path, int openMode, int accessMode, int shareMode, int options, int *error);
        static bool Close(FileHandle* handle, int *error);
        static bool SetFileTime(FileHandle* handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int* error);
        static int64_t GetLength(FileHandle* handle, int *error);
        static bool SetLength(FileHandle* handle, int64_t length, int *error);
        static int64_t Seek(FileHandle* handle, int64_t offset, int origin, int *error);
        static int Read(FileHandle* handle, char *dest, int count, int *error);
        static int32_t Write(FileHandle* handle, const char* buffer, int count, int *error);
        static bool Flush(FileHandle* handle, int* error);
        static void Lock(FileHandle* handle,  int64_t position, int64_t length, int* error);
        static void Unlock(FileHandle* handle,  int64_t position, int64_t length, int* error);
        static utils::Expected<bool> IsExecutable(const std::string& path);
        static bool Truncate(FileHandle* handle, int *error);
        static bool Cancel(FileHandle* handle);

        static bool DuplicateHandle(FileHandle* source_process_handle, FileHandle* source_handle, FileHandle* target_process_handle,
            FileHandle** target_handle, int access, int inherit, int options, int* error);

        static bool IsHandleOpenFileHandle(intptr_t lookup);
    };
}
}
