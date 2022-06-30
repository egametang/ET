#include "os/c-api/il2cpp-config-platforms.h"

#if IL2CPP_TARGET_LINUX && !RUNTIME_TINY

#include "os/SystemCertificates.h"

namespace il2cpp
{
namespace os
{
    void* SystemCertificates::OpenSystemRootStore()
    {
        return 0;
    }

    int SystemCertificates::EnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data)
    {
        // Default location for linux CA
        const char* path = "/etc/ssl/certs/ca-certificates.crt";

        if (*iter == 0)
        {
            *data = (void*)(path);
            *size = sizeof((char*)path);
            *format = DATATYPE_FILE;
            *iter = (void*)1;
            return 1;
        }

        return 0;
    }

    void SystemCertificates::CloseSystemRootStore(void* cStore)
    {
    }
}
}

#endif
