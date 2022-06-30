#pragma once

#if defined(__cplusplus)
#include "os/LibraryLoader.h"
typedef Il2CppMethodPointer UnityPalMethodPointer;
#else
typedef void (*UnityPalMethodPointer)();
#endif

#if defined(__cplusplus)
extern "C"
{
#endif

void* UnityPalLibraryLoaderLoadDynamicLibrary(const char* nativeDynamicLibrary, int flags);
void UnityPalLibraryLoaderCleanupLoadedLibraries();
UnityPalMethodPointer UnityPalLibraryLoaderGetFunctionPointer(void* dynamicLibrary, const char* functionName);
int32_t UnityPalLibraryLoaderCloseLoadedLibrary(void** dynamicLibrary);

#if defined(__cplusplus)
}
#endif
