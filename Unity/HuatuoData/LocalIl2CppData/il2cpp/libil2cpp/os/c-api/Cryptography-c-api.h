#pragma once

#include <stdint.h>

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalGetCryptographyProvider();

int32_t UnityPalOpenCryptographyProvider();

void UnityPalReleaseCryptographyProvider(void* provider);

int32_t UnityPalCryptographyFillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data);

#if defined(__cplusplus)
}
#endif
