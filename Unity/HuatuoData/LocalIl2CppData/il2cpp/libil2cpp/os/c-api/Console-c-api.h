#pragma once

#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

int32_t UnityPalConsoleInternalKeyAvailable(int32_t ms_timeout);
int32_t UnityPalConsoleSetBreak(int32_t wantBreak);
int32_t UnityPalConsoleSetEcho(int32_t wantEcho);
int32_t UnityPalConsoleTtySetup(const char* keypadXmit, const char* teardown, uint8_t* control_characters, int32_t** size);

#if defined(__cplusplus)
}
#endif
