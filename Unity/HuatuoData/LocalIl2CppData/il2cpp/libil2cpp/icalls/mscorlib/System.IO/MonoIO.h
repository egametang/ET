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
        static bool Cancel_internal(intptr_t handle, int32_t* error);
        static bool Close(intptr_t handle, int32_t* error);
        static bool CopyFile(Il2CppChar* path, Il2CppChar* dest, bool overwrite, int32_t* error);
        static bool CreateDirectory(Il2CppChar* path, int32_t* error);
        static bool CreatePipe(intptr_t* read_handle, intptr_t* write_handle, int32_t* error);
        static bool DeleteFile(Il2CppChar* path, int32_t* error);
        static bool DuplicateHandle(intptr_t source_process_handle, intptr_t source_handle, intptr_t target_process_handle, intptr_t* target_handle, int32_t access, int32_t inherit, int32_t options, int32_t* error);
        static bool FindCloseFile(intptr_t hnd);
        static bool FindNextFile(intptr_t hnd, Il2CppString** fileName, int32_t* fileAttr, int32_t* error);
        static bool Flush(intptr_t handle, int32_t* error);
        static bool GetFileStat(Il2CppChar* path, FileStat* stat, int32_t* error);
        static bool MoveFile(Il2CppChar* path, Il2CppChar* dest, int32_t* error);
        static bool RemapPath(Il2CppString* path, Il2CppString** newPath);
        static bool RemoveDirectory(Il2CppChar* path, int32_t* error);
        static bool ReplaceFile(Il2CppChar* sourceFileName, Il2CppChar* destinationFileName, Il2CppChar* destinationBackupFileName, bool ignoreMetadataErrors, int32_t* error);
        static bool SetCurrentDirectory(Il2CppChar* path, int32_t* error);
        static bool SetFileAttributes(Il2CppChar* path, int32_t attrs, int32_t* error);
        static bool SetFileTime(intptr_t handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int32_t* error);
        static bool SetLength(intptr_t handle, int64_t length, int32_t* error);
        static Il2CppChar get_AltDirectorySeparatorChar();
        static Il2CppChar get_DirectorySeparatorChar();
        static Il2CppChar get_PathSeparator();
        static Il2CppChar get_VolumeSeparatorChar();
        static int32_t Read(intptr_t handle, Il2CppArray* dest, int32_t dest_offset, int32_t count, int32_t* error);
        static int32_t Write(intptr_t handle, Il2CppArray* src, int32_t src_offset, int32_t count, int32_t* error);
        static int64_t GetLength(intptr_t handle, int32_t* error);
        static int64_t Seek(intptr_t handle, int64_t offset, int32_t origin, int32_t* error);
        static intptr_t FindFirstFile(Il2CppChar* pathWithPattern, Il2CppString** fileName, int32_t* fileAttr, int32_t* error);
        static intptr_t get_ConsoleError();
        static intptr_t get_ConsoleInput();
        static intptr_t get_ConsoleOutput();
        static intptr_t Open(Il2CppChar* filename, int32_t mode, int32_t access, int32_t share, int32_t options, int32_t* error);
        static int32_t GetFileAttributes(Il2CppChar* path, int32_t* error);
        static int32_t GetFileType(intptr_t handle, int32_t* error);
        static Il2CppString* GetCurrentDirectory(int32_t* error);
        static void DumpHandles();
        static void Lock(intptr_t handle, int64_t position, int64_t length, int32_t* error);
        static void Unlock(intptr_t handle, int64_t position, int64_t length, int32_t* error);
    };
} /* namespace IO */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
