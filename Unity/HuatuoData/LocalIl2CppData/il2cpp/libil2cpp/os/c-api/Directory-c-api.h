#pragma once

#include "Error-c-api.h"
#include <stdint.h>

#if defined(__cplusplus)
#include "os/Directory.h"
typedef il2cpp::os::Directory::FindHandle UnityPalFindHandle;

#else

typedef struct UnityPalFindHandle UnityPalFindHandle;

#endif

#if defined(__cplusplus)
extern "C"
{
#endif

const char* UnityPalDirectoryGetCurrent(int* error);
int32_t UnityPalDirectorySetCurrent(const char* path, int* error);
int32_t UnityPalDirectoryCreate(const char* path, int *error);
int32_t UnityPalDirectoryRemove(const char* path, int *error);

void UnityPalDirectoryGetFileSystemEntries(const char* path, const char* pathWithPattern, int32_t attrs, int32_t mask, int* error, char*** entries, int32_t* numEntries);


UnityPalFindHandle* UnityPalDirectoryFindHandleNew(const char* searchPathWithPattern);
void UnityPalDirectoryFindHandleDelete(UnityPalFindHandle* object);

int32_t UnityPalDirectoryCloseOSHandle(UnityPalFindHandle* object);
void* UnityPalDirectoryGetOSHandle(UnityPalFindHandle* object);

UnityPalErrorCode UnityPalDirectoryFindFirstFile(UnityPalFindHandle* findHandle, const char* searchPathWithPattern, char** resultFileName, int32_t* resultAttributes);
UnityPalErrorCode UnityPalDirectoryFindNextFile(UnityPalFindHandle*  findHandle, char** resultFileName, int32_t* resultAttributes);

#if defined(__cplusplus)
}
#endif
