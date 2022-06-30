#include "il2cpp-config.h"
#include "pal_platform.h"

#if IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL

#define IL2CPP_HAVE_O_CLOEXEC 1

#include <stdlib.h>
#include <fcntl.h>
#include <errno.h>
#include <stdio.h>
#include <time.h>
#include <unistd.h>

extern "C"
{
    // Items needed by mscorlib
    IL2CPP_EXPORT void SystemNative_GetNonCryptographicallySecureRandomBytes(uint8_t* buffer, int32_t bufferLength);
}

void SystemNative_GetNonCryptographicallySecureRandomBytes(uint8_t* buffer, int32_t bufferLength)
{
    IL2CPP_ASSERT(buffer != NULL);

#if IL2CPP_HAVE_ARC4RANDOM_BUF
    arc4random_buf(buffer, (size_t)bufferLength);
#else
    static volatile int rand_des = -1;
    long num = 0;
    static bool sMissingDevURandom;
    static bool sInitializedMRand;

#if !IL2CPP_HAVE_NO_UDEV_RANDOM
    if (!sMissingDevURandom)
    {
        if (rand_des == -1)
        {
            int fd;

            do
            {
#if IL2CPP_HAVE_O_CLOEXEC
                fd = open("/dev/urandom", O_RDONLY, O_CLOEXEC);
#else
                fd = open("/dev/urandom", O_RDONLY);
                fcntl(fd, F_SETFD, FD_CLOEXEC);
#endif
            }
            while ((fd == -1) && (errno == EINTR));

            if (fd != -1)
            {
                if (!__sync_bool_compare_and_swap(&rand_des, -1, fd))
                {
                    // Another thread has already set the rand_des
                    close(fd);
                }
            }
            else if (errno == ENOENT)
            {
                sMissingDevURandom = true;
            }
        }

        if (rand_des != -1)
        {
            int32_t offset = 0;
            do
            {
                ssize_t n = read(rand_des, buffer + offset , (size_t)(bufferLength - offset));
                if (n == -1)
                {
                    if (errno == EINTR)
                    {
                        continue;
                    }

                    IL2CPP_ASSERT(false && "read from /dev/urandom has failed");
                    break;
                }

                offset += n;
            }
            while (offset != bufferLength);
        }
    }
#endif // !IL2CPP_HAVE_NO_UDEV_RANDOM

    if (!sInitializedMRand)
    {
        srand48(time(NULL));
        sInitializedMRand = true;
    }

    // always xor srand48 over the whole buffer to get some randomness
    // in case /dev/urandom is not really random

    for (int i = 0; i < bufferLength; i++)
    {
        if (i % 4 == 0)
        {
            num = lrand48();
        }

        *(buffer + i) ^= num;
        num >>= 8;
    }
#endif // IL2CPP_HAVE_ARC4RANDOM_BUF
}

#endif
