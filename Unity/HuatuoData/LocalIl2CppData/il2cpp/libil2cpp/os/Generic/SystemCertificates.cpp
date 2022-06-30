#include "os/c-api/il2cpp-config-platforms.h"

#if !IL2CPP_PLATFORM_SUPPORTS_SYSTEM_CERTIFICATES
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
        return 0;
    }

    void SystemCertificates::CloseSystemRootStore(void* cStore)
    {
    }
}
}

#endif
