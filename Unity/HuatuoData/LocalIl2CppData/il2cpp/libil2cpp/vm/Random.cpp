#include "il2cpp-config.h"
#include "vm/Random.h"

#include "os/Cryptography.h"

namespace il2cpp
{
namespace vm
{
    bool Random::Open()
    {
        return il2cpp::os::Cryptography::OpenCryptographyProvider();
    }

    void* Random::Create()
    {
        il2cpp::os::Cryptography::OpenCryptographyProvider();
        return il2cpp::os::Cryptography::GetCryptographyProvider();
    }

    void Random::Free(void* handle)
    {
        il2cpp::os::Cryptography::ReleaseCryptographyProvider(handle);
    }

/**
* mono_rand_try_get_bytes:
* @handle: A pointer to an RNG handle. Handle is set to NULL on failure.
* @buffer: A buffer into which to write random data.
* @buffer_size: Number of bytes to write into buffer.
* @error: Set on error.
*
* Returns: FALSE on failure and sets @error, TRUE on success.
*
* Extracts bytes from an RNG handle.
*/
    bool Random::TryGetBytes(void* *handle, unsigned char *buffer, int buffer_size)
    {
        IL2CPP_ASSERT(handle);
        void* provider = *handle;

        if (!il2cpp::os::Cryptography::FillBufferWithRandomBytes(provider, buffer_size, buffer))
        {
            il2cpp::os::Cryptography::ReleaseCryptographyProvider(provider);
            /* we may have lost our context with CryptoAPI, but all hope isn't lost yet! */
            provider = il2cpp::os::Cryptography::GetCryptographyProvider();
            if (!il2cpp::os::Cryptography::FillBufferWithRandomBytes(provider, buffer_size, buffer))
            {
                il2cpp::os::Cryptography::ReleaseCryptographyProvider(provider);
                *handle = 0;
                //mono_error_set_execution_engine(error, "Failed to gen random bytes (%d)", GetLastError());
                return false;
            }
        }

        return true;
    }

/**
* mono_rand_try_get_uint32:
* @handle: A pointer to an RNG handle. Handle is set to NULL on failure.
* @val: A pointer to a 32-bit unsigned int, to which the result will be written.
* @min: Result will be greater than or equal to this value.
* @max: Result will be less than or equal to this value.
*
* Returns: FALSE on failure, TRUE on success.
*
* Extracts one 32-bit unsigned int from an RNG handle.
*/
    bool Random::TryGetUnsignedInt32(void* *handle, uint32_t *val, uint32_t min, uint32_t max)
    {
        IL2CPP_ASSERT(val);
        if (!TryGetBytes(handle, (unsigned char*)val, sizeof(uint32_t)))
            return false;

        double randomDouble = ((double)*val) / (((double)UINT32_MAX) + 1); // Range is [0,1)
        *val = (uint32_t)(randomDouble * (max - min + 1) + min);

        IL2CPP_ASSERT(*val >= min);
        IL2CPP_ASSERT(*val <= max);

        return true;
    }

    uint32_t Random::Next(void** handle, uint32_t min, uint32_t max)
    {
        uint32_t val;
        bool ok = TryGetUnsignedInt32(handle, &val, min, max);

        IL2CPP_ASSERT(ok);

        return val;
    }
} /* namespace vm */
} /* namespace il2cpp */
