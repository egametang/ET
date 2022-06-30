#pragma once

#include <stdint.h>
#include "os/c-api/Process_c_api.h"

#if defined(__cplusplus)
extern "C"
{
#endif

int32_t UnityPalNativeCloseProcess(UnityPalProcessHandle* handle);
int32_t UnityPalNativeGetExitCodeProcess(UnityPalProcessHandle* handle, int32_t* exitCode);
int32_t UnityPalNativeGetCurrentProcessId();
UnityPalProcessHandle* UnityPalNativeGetCurrentProcess();

#if defined(__cplusplus)
}
#endif
