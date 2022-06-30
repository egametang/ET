#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/SystemCertificates.h"
#include "os/c-api/SystemCertificates-c-api.h"

extern "C"
{
    void* UnityPalSystemCertificatesOpenSystemRootStore()
    {
        return il2cpp::os::SystemCertificates::OpenSystemRootStore();
    }

    int UnityPalSystemCertificatesEnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data)
    {
        return il2cpp::os::SystemCertificates::EnumSystemCertificates(certStore, iter, format, size, data);
    }

    void UnityPalSystemCertificatesCloseSystemRootStore(void* cStore)
    {
        il2cpp::os::SystemCertificates::CloseSystemRootStore(cStore);
    }
}

#endif
