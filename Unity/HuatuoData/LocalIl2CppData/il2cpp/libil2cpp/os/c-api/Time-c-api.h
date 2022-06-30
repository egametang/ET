#pragma once

#include "il2cpp-config-platforms.h"

#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

uint32_t UnityPalGetTicksMillisecondsMonotonic();
int64_t STDCALL UnityPalGetTicks100NanosecondsMonotonic();
int64_t UnityPalGetTicks100NanosecondsDateTime();
int64_t STDCALL UnityPalGetSystemTimeAsFileTime();

#if defined(__cplusplus)
}
#endif
