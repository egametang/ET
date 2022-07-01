#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

#undef CopyFile
#undef DeleteFile
#undef MoveFile
#undef ReplaceFile
#undef GetFileAttributes
#undef SetFileAttributes
#undef CreatePipe
#undef GetTempPath
#undef FindNextFile
#undef FindFirstFile

struct Il2CppArray;
struct Il2CppString;

typedef int32_t MonoIOError;
typedef int32_t FileAttributes;

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
    struct FileStat
    {
        int32_t attributes;
        int64_t length;
        int64_t creation_time;
        int64_t last_access_time;
        int64_t last_write_time;
    };

    class LIBIL2CPP_CODEGEN_API MonoIO
    {
    public:
        static bool Close(intptr_t handle, int *error);
        static bool CopyFile(Il2CppString* src, Il2CppString* dest, bool overwrite, MonoIOError* error);
        static bool CopyFile40(Il2CppChar* src, Il2CppChar* dest, bool overwrite, MonoIOError* error);
        static bool CreateDirectory(Il2CppString* path, MonoIOError* error);
        static bool CreateDirectory40(Il2CppChar* path, MonoIOError* error);
        static bool CreatePipe(intptr_t* read_handle, intptr_t* write_handle);
        static bool DeleteFile(Il2CppString* path, MonoIOError* error);
        static bool DeleteFile40(Il2CppChar* path, MonoIOError* error);
        static bool DuplicateHandle(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options);
        static bool Flush(intptr_t handle, MonoIOError* error);
        static Il2CppString* GetCurrentDirectory(MonoIOError* error);
        static FileAttributes GetFileAttributes(Il2CppString* path, MonoIOError* error);
        static FileAttributes GetFileAttributes40(Il2CppChar* path, MonoIOError* error);
        static bool GetFileStat(Il2CppString* path, FileStat * stat, int32_t* error);
        static bool GetFileStat40(Il2CppChar* path, FileStat * stat, int32_t* error);
        static Il2CppArray* GetFileSystemEntries(Il2CppString* path, Il2CppString* path_with_pattern, int32_t attrs, int32_t mask, MonoIOError* error);
        static int GetFileType(intptr_t handle, int *error);
        static int64_t GetLength(intptr_t handle, int *error);
        static int32_t GetTempPath(Il2CppString** path);
        static void Lock(intptr_t handle, int64_t position, int64_t length, MonoIOError* error);
        static bool MoveFile(Il2CppString* src, Il2CppString* dest, MonoIOError* error);
        static bool MoveFile40(Il2CppChar* src, Il2CppChar* dest, MonoIOError* error);
        static intptr_t Open(Il2CppString *filename, int mode, int access_mode, int share, int options, int *error);
        static intptr_t Open40(Il2CppChar *filename, int mode, int access_mode, int share, int options, int *error);
        static int Read(intptr_t handle, Il2CppArray *dest, int dest_offset, int count, int *error);
        static bool RemoveDirectory(Il2CppString* path, MonoIOError* error);
        static bool RemoveDirectory40(Il2CppChar* path, MonoIOError* error);
        static bool ReplaceFile(Il2CppString* sourceFileName, Il2CppString* destinationFileName, Il2CppString* destinationBackupFileName, bool ignoreMetadataErrors, MonoIOError* error);
        static bool ReplaceFile40(Il2CppChar* sourceFileName, Il2CppChar* destinationFileName, Il2CppChar* destinationBackupFileName, bool ignoreMetadataErrors, MonoIOError* error);
        static int64_t Seek(intptr_t handle, int64_t offset, int origin, int *error);
        static bool SetCurrentDirectory(Il2CppString* path, int* error);
        static bool SetCurrentDirectory40(Il2CppChar* path, int* error);
        static bool SetFileAttributes(Il2CppString* path, FileAttributes attrs, MonoIOError* error);
        static bool SetFileAttributes40(Il2CppChar* path, FileAttributes attrs, MonoIOError* error);
        static bool SetFileTime(intptr_t handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, MonoIOError* error);
        static bool SetLength(intptr_t handle, int64_t length, int *error);
        static void Unlock(intptr_t handle, int64_t position, int64_t length, MonoIOError* error);
        static int Write(intptr_t handle, Il2CppArray * src, int src_offset, int count, int * error);
        static Il2CppChar get_AltDirectorySeparatorChar(void);
        static intptr_t get_ConsoleError(void);
        static intptr_t get_ConsoleInput(void);
        static intptr_t get_ConsoleOutput(void);
        static Il2CppChar get_DirectorySeparatorChar(void);
        static Il2CppChar get_PathSeparator(void);
        static Il2CppChar get_VolumeSeparatorChar(void);
        static bool CreatePipe40(intptr_t* read_handle, intptr_t* write_handle, MonoIOError* error);
        static bool DuplicateHandle40(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options, MonoIOError* error);
        static bool RemapPath(Il2CppString* path, Il2CppString** newPath);

        static int32_t FindClose(intptr_t handle);
        static Il2CppString* FindFirst(Il2CppString* path, Il2CppString* pathWithPattern, int32_t* resultAttributes, MonoIOError* error, intptr_t* handle);
        static Il2CppString* FindNext(intptr_t handle, int32_t* result_attr, MonoIOError* error);
        static void DumpHandles();

        static bool FindCloseFile(intptr_t hnd);
        static bool FindNextFile(intptr_t hnd, Il2CppString** fileName, int32_t* fileAttr, int32_t* error);
        static intptr_t FindFirstFile(Il2CppChar* path_with_pattern, Il2CppString** fileName, int32_t* fileAttr, int32_t* error);
    };
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
