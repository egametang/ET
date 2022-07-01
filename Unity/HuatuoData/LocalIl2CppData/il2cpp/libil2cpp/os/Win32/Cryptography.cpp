#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS_DESKTOP || IL2CPP_TARGET_WINDOWS_GAMES

#include "os/Win32/WindowsHeaders.h"
#include "os/Cryptography.h"

#include <Wincrypt.h>

namespace il2cpp
{
namespace os
{
    void* Cryptography::GetCryptographyProvider()
    {
        HCRYPTPROV provider = 0;

        if (!CryptAcquireContext(&provider, NULL, NULL, PROV_INTEL_SEC, CRYPT_VERIFYCONTEXT))
        {
            if (!CryptAcquireContext(&provider, NULL, NULL, PROV_RSA_FULL, CRYPT_VERIFYCONTEXT))
            {
                provider = 0;
            }
        }

        return (void*)provider;
    }

    bool Cryptography::OpenCryptographyProvider()
    {
        return false;
    }

    void Cryptography::ReleaseCryptographyProvider(void* provider)
    {
        CryptReleaseContext((HCRYPTPROV)provider, 0);
    }

    bool Cryptography::FillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data)
    {
        return CryptGenRandom((HCRYPTPROV)provider, length, data) == TRUE;
    }
}
}

#endif
