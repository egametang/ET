#pragma once

#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

char* UnityPalGetExecutablePath();
char* UnityPalGetTempPath();
int32_t UnityPalIsAbsolutePath(const char* path);
char* UnityPalBasename(const char* path);
char* UnityPalDirectoryName(const char* path);

#if defined(__cplusplus)
}
#endif
