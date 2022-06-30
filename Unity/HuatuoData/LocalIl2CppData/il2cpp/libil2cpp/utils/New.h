#pragma once

#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE
#include <malloc.h>
#else
#include <stdlib.h>
#endif
#include <new>

inline void* operator new(size_t size, int alignment)
{
    void* result = NULL;
    #if IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE
    result = _aligned_malloc(size, alignment);
    #elif IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_PSP2
    result = memalign(alignment, size);
    #else
    if (posix_memalign(&result, size, alignment))
        result = NULL;
    #endif
    if (!result)
        throw std::bad_alloc();
    return result;
}

#if IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE // Visual C++ warns if new is overridden but delete is not.
inline void operator delete(void* ptr, int alignment) throw ()
{
    free(ptr);
}

#endif
