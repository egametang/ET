#pragma once

#include <stddef.h>

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalAlignedAlloc(size_t size, size_t alignment);
void* UnityPalAlignedReAlloc(void* memory, size_t newSize, size_t alignment);
void UnityPalAlignedFree(void* memory);

#if defined(__cplusplus)
}
#endif
