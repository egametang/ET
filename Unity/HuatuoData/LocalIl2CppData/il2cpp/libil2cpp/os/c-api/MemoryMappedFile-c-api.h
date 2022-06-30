#pragma once

#include "File-c-api.h"

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalMemoryMappedFileMap(UnityPalFileHandle* file);
void UnityPalMemoryMappedFileUnmap(void* address);

void* UnityPalMemoryMappedFileMapWithParams(UnityPalFileHandle* file, int64_t length, int64_t offset);
void UnityPalMemoryMappedFileUnmapWithParams(void* address, int64_t length);

#if defined(__cplusplus)
}
#endif
