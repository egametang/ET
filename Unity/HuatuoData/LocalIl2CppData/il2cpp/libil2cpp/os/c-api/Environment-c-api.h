#pragma once

#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

char* UnityPalGetOsUserName();
char* UnityPalGetMachineName();
char* UnityPalGetEnvironmentVariable(const char* name);
void UnityPalSetEnvironmentVariable(const char* name, const char* value);
char* UnityPalGetHomeDirectory();
int32_t UnityPalGetProcessorCount();

#if defined(__cplusplus)
}
#endif
