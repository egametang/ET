#pragma once
#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalCpuInfoCreate();
int32_t UnityPalCpuInfoUsage(void* previous);

#if defined(__cplusplus)
}
#endif
