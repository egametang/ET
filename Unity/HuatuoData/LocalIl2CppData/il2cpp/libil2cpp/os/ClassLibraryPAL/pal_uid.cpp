#include "il2cpp-config.h"
#include "pal_platform.h"

#if IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL

#include "pal_platform.h"

#include <unistd.h>

extern "C"
{
    // Items needed by mscorlib
    IL2CPP_EXPORT uint32_t SystemNative_GetEUid(void);
    IL2CPP_EXPORT uint32_t SystemNative_GetEGid(void);
}

uint32_t SystemNative_GetEUid(void)
{
    return geteuid();
}

uint32_t SystemNative_GetEGid()
{
    return getegid();
}

#endif
