#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/Cryptography.h"
#include <errno.h>
#include <unistd.h>
#include <fcntl.h>

#ifndef NAME_DEV_URANDOM
#define NAME_DEV_URANDOM "/dev/urandom"
#endif

static int64_t file = -1;

namespace il2cpp
{
namespace os
{
    void* Cryptography::GetCryptographyProvider()
    {
        return (file < 0) ? NULL : (void*)file;
    }

    bool Cryptography::OpenCryptographyProvider()
    {
#ifdef NAME_DEV_URANDOM
        file = open(NAME_DEV_URANDOM, O_RDONLY);
#endif

#ifdef NAME_DEV_RANDOM
        if (file < 0)
            file = open(NAME_DEV_RANDOM, O_RDONLY);
#endif
        return true;
    }

    void Cryptography::ReleaseCryptographyProvider(void* provider)
    {
    }

    bool Cryptography::FillBufferWithRandomBytes(void* provider, uint32_t length, unsigned char* data)
    {
        int count = 0;
        ssize_t err;

        // Make sure the provider is correct, or we may end up reading from the wrong file.
        // If provider happens to be 0 we will read from stdin and hang!
        if ((int64_t)provider != file)
            return false;

        do
        {
            err = read((int)(size_t)provider, data + count, length - count);
            if (err < 0)
            {
                if (errno == EINTR)
                    continue;
                break;
            }
            count += err;
        }
        while (count < length);

        return err >= 0;
    }
}
}

#endif
