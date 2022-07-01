#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE

#include "os/Win32/WindowsHeaders.h"
#include "os/Cryptography.h"

namespace il2cpp
{
namespace os
{
// This has to be non-null value because the return value of NULL from GetCryptographyProvider means it failed
    void* const kCryptographyProvider = reinterpret_cast<void*>(0x12345678);

    void* Cryptography::GetCryptographyProvider()
    {
        return kCryptographyProvider;
    }

    bool Cryptography::OpenCryptographyProvider()
    {
        return true;
    }

    void Cryptography::ReleaseCryptographyProvider(void* provider)
    {
        // Do nothing, since we never allocated it
    }

    bool Cryptography::FillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data)
    {
        NO_UNUSED_WARNING(provider);
        return SUCCEEDED(BCryptGenRandom(NULL, data, length, BCRYPT_USE_SYSTEM_PREFERRED_RNG));
    }
}
}

#endif
