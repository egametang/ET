#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/Cryptography-c-api.h"
#include "os/Cryptography.h"

extern "C"
{
    void* UnityPalGetCryptographyProvider()
    {
        return il2cpp::os::Cryptography::GetCryptographyProvider();
    }

    int32_t UnityPalOpenCryptographyProvider()
    {
        return il2cpp::os::Cryptography::OpenCryptographyProvider();
    }

    void UnityPalReleaseCryptographyProvider(void* provider)
    {
        return il2cpp::os::Cryptography::ReleaseCryptographyProvider(provider);
    }

    int32_t UnityPalCryptographyFillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data)
    {
        return il2cpp::os::Cryptography::FillBufferWithRandomBytes(provider, length, data);
    }
}

#endif
