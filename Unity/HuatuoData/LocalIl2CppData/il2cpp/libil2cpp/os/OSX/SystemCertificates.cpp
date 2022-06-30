#include "il2cpp-config.h"

#if IL2CPP_TARGET_OSX

#include "os/SystemCertificates.h"
#include <Security/SecTrust.h>
#include <Security/SecCertificate.h>
#include <Security/SecImportExport.h>

namespace il2cpp
{
namespace os
{
    void* SystemCertificates::OpenSystemRootStore()
    {
        CFArrayRef anchors = NULL;
        OSStatus s;

        s = SecTrustCopyAnchorCertificates(&anchors);

        IL2CPP_ASSERT(s == noErr);

        return (void*)anchors;
    }

    int SystemCertificates::EnumSystemCertificates(void* certStore, void** iter, int *format, int* size, void** data)
    {
        OSStatus s;
        CFDataRef certData;
        int numCerts = (int)CFArrayGetCount((CFArrayRef)certStore);
        *format = DATATYPE_STRING;
        // Order matters when it comes to certificates need to read in reverse
        int currentCert = numCerts;
        if (*iter != NULL)
        {
            currentCert = static_cast<int>(reinterpret_cast<intptr_t>(*iter));
        }

        SecCertificateRef cert = (SecCertificateRef)CFArrayGetValueAtIndex((CFArrayRef)certStore, (currentCert - 1));

        s = SecItemExport(cert, kSecFormatPEMSequence, kSecItemPemArmour, NULL, &certData);

        if (s == errSecSuccess)
        {
            char* certPEMStr = (char*)CFDataGetBytePtr(certData);
            *iter = reinterpret_cast<void*>(static_cast<intptr_t>((currentCert - 1)));
            *data = certPEMStr;
            *size = sizeof(certPEMStr);
            if ((currentCert - 1) > 0)
            {
                return TRUE;
            }
        }

        return FALSE;
    }

    void SystemCertificates::CloseSystemRootStore(void* cStore)
    {
        CFRelease((CFArrayRef)cStore);
    }
}
}

#endif
