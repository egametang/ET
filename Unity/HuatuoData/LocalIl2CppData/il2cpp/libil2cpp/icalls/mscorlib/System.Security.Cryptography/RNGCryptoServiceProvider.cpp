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
    bool RNGCryptoServiceProvider::RngOpen()
    {
        return os::Cryptography::OpenCryptographyProvider();
    }

    intptr_t RNGCryptoServiceProvider::RngGetBytes(intptr_t handle, uint8_t* data, intptr_t data_length)
    {
        IL2CPP_ASSERT(data || !data_length);

        if (!os::Cryptography::FillBufferWithRandomBytes(reinterpret_cast<void*>(handle), data_length, data))
        {
            os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(handle));
            handle = RngInitialize(NULL, 0);

            if (!os::Cryptography::FillBufferWithRandomBytes(reinterpret_cast<void*>(handle), data_length, data))
            {
                os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(handle));
                return 0;
            }
        }

        return handle;
    }

    intptr_t RNGCryptoServiceProvider::RngInitialize(uint8_t* seed, intptr_t seed_length)
    {
        void* provider = os::Cryptography::GetCryptographyProvider();

        if ((provider != 0) && seed)
        {
            unsigned char* data = (unsigned char*)IL2CPP_MALLOC(seed_length);
            if (data)
            {
                memcpy(data, seed, seed_length);
                os::Cryptography::FillBufferWithRandomBytes(provider, seed_length, data);
                memset(data, 0, seed_length);
                IL2CPP_FREE(data);
            }
        }

        return reinterpret_cast<intptr_t>(provider);
    }

    void RNGCryptoServiceProvider::RngClose(intptr_t handle)
    {
        os::Cryptography::ReleaseCryptographyProvider(reinterpret_cast<void*>(handle));
    }
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
