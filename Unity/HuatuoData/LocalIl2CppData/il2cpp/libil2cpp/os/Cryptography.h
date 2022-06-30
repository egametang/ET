#pragma once

namespace il2cpp
{
namespace os
{
    class Cryptography
    {
    public:
        /* Returns a handle the cryptography provider to use in other calls on this API. */
        static void* GetCryptographyProvider();

        /* Open the cryptogrpahy provider. */
        static bool OpenCryptographyProvider();

        /* Indicate that the cyrptography provider is no longer in use. */
        static void ReleaseCryptographyProvider(void* provider);

        /* Use the provider to fill the buffer with cryptographically random bytes. */
        static bool FillBufferWithRandomBytes(void* provider, intptr_t length, unsigned char* data);
    };
}
}
