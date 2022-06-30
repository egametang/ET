#pragma once

#if defined(__cplusplus)
extern "C"
{
#endif

void UnityPalLocaleInitialize();
void UnityPalLocaleUnInitialize();
char* UnityPalGetLocale();

#if defined(__cplusplus)
}
#endif
