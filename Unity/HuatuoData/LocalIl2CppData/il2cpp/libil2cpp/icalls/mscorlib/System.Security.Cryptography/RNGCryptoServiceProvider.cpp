#include "il2cpp-config.h"
#include "il2cpp-api.h"
#include "vm/Array.h"
#include "os/Cryptography.h"
#include "utils/Memory.h"
#include "icalls/mscorlib/System.Security.Cryptography/RNGCryptoServiceProvider.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Security
{
namespace Cryptography
{
    void RNGCryptoServiceProvider::RngClose(intptr_t provider)
    {
        os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(provider));
    }

    intptr_t RNGCryptoServiceProvider::RngGetBytes(intptr_t provider, Il2CppArray *data)
    {
        uint32_t len = il2cpp_array_length(data);
        unsigned char* buf = il2cpp_array_addr(data, unsigned char, 0);

        if (!os::Cryptography::FillBufferWithRandomBytes(reinterpret_cast<void*>(provider), len, buf))
        {
            os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(provider));
            provider = RngInitialize(NULL);

            if (!os::Cryptography::FillBufferWithRandomBytes(reinterpret_cast<void*>(provider), len, buf))
            {
                os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(provider));
                return 0;
            }
        }

        return provider;
    }

    intptr_t RNGCryptoServiceProvider::RngInitialize(Il2CppArray *seed)
    {
        void* provider = os::Cryptography::GetCryptographyProvider();

        if ((provider != 0) && seed)
        {
            uint32_t len = il2cpp_array_length(seed);
            unsigned char *buf = il2cpp_array_addr(seed, unsigned char, 0);
            unsigned char* data = (unsigned char*)IL2CPP_MALLOC(len);
            if (data)
            {
                memcpy(data, buf, len);
                os::Cryptography::FillBufferWithRandomBytes(provider, len, data);
                memset(data, 0, len);
                IL2CPP_FREE(data);
            }
        }

        return reinterpret_cast<intptr_t>(provider);
    }

    bool RNGCryptoServiceProvider::RngOpen()
    {
        return os::Cryptography::OpenCryptographyProvider();
    }
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
