#include "il2cpp-config.h"
#include "pal_platform.h"

#if IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL

#if IL2CPP_HAVE_SYS_UN
#include <sys/un.h>
#endif

extern "C"
{
    IL2CPP_EXPORT void SystemNative_GetDomainSocketSizes(int32_t* pathOffset, int32_t* pathSize, int32_t* addressSize);
}

void SystemNative_GetDomainSocketSizes(int32_t* pathOffset, int32_t* pathSize, int32_t* addressSize)
{
    IL2CPP_ASSERT(pathOffset != NULL);
    IL2CPP_ASSERT(pathSize != NULL);
    IL2CPP_ASSERT(addressSize != NULL);

#if IL2CPP_HAVE_SYS_UN
    struct sockaddr_un domainSocket;

    *pathOffset = offsetof(struct sockaddr_un, sun_path);
    *pathSize = sizeof(domainSocket.sun_path);
    *addressSize = sizeof(domainSocket);
#else
    *pathOffset = 0;
    *pathSize = 0;
    *addressSize = 0;
#endif
}

#endif // IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL
