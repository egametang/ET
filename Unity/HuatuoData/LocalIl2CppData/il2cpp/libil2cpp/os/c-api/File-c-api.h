#pragma once

#include "OSGlobalEnums.h"

#if defined(__cplusplus)
#include "os/ErrorCodes.h"
#include "os/File.h"

typedef il2cpp::os::FileHandle UnityPalFileHandle;

#else

typedef struct UnityPalFileHandle UnityPalFileHandle;

#endif //__cplusplus

typedef struct
{
    char* name;
    int32_t attributes;
    int64_t length;
    int64_t creation_time;
    int64_t last_access_time;
    int64_t last_write_time;
} UnityPalFileStat;

#if defined(__cplusplus)
extern "C"
{
#endif

int32_t UnityPalIsatty(UnityPalFileHandle* fileHandle);
UnityPalFileHandle* UnityPalGetStdInput();
UnityPalFileHandle* UnityPalGetStdOutput();
UnityPalFileHandle* UnityPalGetStdError();
int32_t UnityPalCreatePipe(UnityPalFileHandle** read_handle, UnityPalFileHandle** write_handle);
int32_t UnityPalCreatePipe_with_error(UnityPalFileHandle** read_handle, UnityPalFileHandle** write_handle, int* error);
FileType UnityPalGetFileType(UnityPalFileHandle* handle);
UnityPalFileAttributes UnityPalGetFileAttributes(const char* path, int* error);
int32_t UnityPalSetFileAttributes(const char* path, UnityPalFileAttributes attributes, int* error);
int32_t UnityPalGetFileStat(const char* path, UnityPalFileStat * stat, int* error);
int32_t UnityPalCopyFile(const char* src, const char* dest, int32_t overwrite, int* error);
int32_t UnityPalMoveFile(const char* src, const char* dest, int* error);
int32_t UnityPalDeleteFile(const char* path, int *error);
int32_t UnityPalReplaceFile(const char* sourceFileName, const char* destinationFileName, const char* destinationBackupFileName, int32_t ignoreMetadataErrors, int* error);
UnityPalFileHandle* UnityPalOpen(const char* path, int openMode, int accessMode, int shareMode, int options, int *error);
int32_t UnityPalClose(UnityPalFileHandle* handle, int *error);
int32_t UnityPalSetFileTime(UnityPalFileHandle* handle, int64_t creation_time, int64_t last_access_time, int64_t last_write_time, int* error);
int64_t UnityPalGetLength(UnityPalFileHandle* handle, int *error);
int32_t UnityPalSetLength(UnityPalFileHandle* handle, int64_t length, int *error);
int32_t UnityPalTruncate(UnityPalFileHandle* handle, int *error);
int64_t UnityPalSeek(UnityPalFileHandle* handle, int64_t offset, int origin, int *error);
int UnityPalRead(UnityPalFileHandle* handle, char *dest, int count, int *error);
int32_t UnityPalWrite(UnityPalFileHandle* handle, const char* buffer, int count, int *error);
int32_t UnityPalFlush(UnityPalFileHandle* handle, int* error);
void UnityPalLock(UnityPalFileHandle* handle, int64_t position, int64_t length, int* error);
void UnityPalUnlock(UnityPalFileHandle* handle, int64_t position, int64_t length, int* error);
int32_t UnityPalDuplicateHandle(UnityPalFileHandle* source_process_handle, UnityPalFileHandle* source_handle, UnityPalFileHandle* target_process_handle,
    UnityPalFileHandle** target_handle, int access, int inherit, int options, int* error);
int32_t UnityPalIsExecutable(const char* filename);

#if defined(__cplusplus)
}
#endif
