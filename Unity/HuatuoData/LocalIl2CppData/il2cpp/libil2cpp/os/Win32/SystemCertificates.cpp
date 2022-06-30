#include "os/c-api/il2cpp-config-platforms.h"

#if IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINRT || IL2CPP_TARGET_WINDOWS_GAMES

#include "os/SystemCertificates.h"
#include "WindowsHeaders.h"

namespace il2cpp
{
namespace os
{
    void* SystemCertificates::OpenSystemRootStore()
    {
        HCERTSTORE hStore = CertOpenStore(CERT_STORE_PROV_SYSTEM, 0, NULL, CERT_STORE_READONLY_FLAG, L"ROOT");
        if (hStore == NULL)
            return 0;

        return hStore;
    }

    int SystemCertificates::EnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data)
    {
        HCERTSTORE hStore = (HCERTSTORE)certStore;
        *format = DATATYPE_INTPTR;

        // Build list of system certificates
        PCCERT_CONTEXT pContext = (PCCERT_CONTEXT)*iter;
        if (pContext = CertEnumCertificatesInStore(hStore, pContext))
        {
            *iter = (void*)pContext;
            *data = pContext->pbCertEncoded;
            *size = pContext->cbCertEncoded;
            return TRUE;
        }
        else if (*iter)
        {
            CertFreeCertificateContext((PCCERT_CONTEXT)*iter);
        }

        return FALSE;
    }

    void SystemCertificates::CloseSystemRootStore(void* cStore)
    {
        CertCloseStore((HCERTSTORE)cStore, 0);
    }
}
}

#endif // IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINRT || IL2CPP_TARGET_WINDOWS_GAMES
