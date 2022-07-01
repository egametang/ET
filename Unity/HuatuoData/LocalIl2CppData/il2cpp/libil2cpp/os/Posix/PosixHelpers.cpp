#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <sys/errno.h>

#include "os/Posix/PosixHelpers.h"

namespace il2cpp
{
namespace os
{
namespace posix
{
    int Poll(pollfd* handles, int numHandles, int timeout)
    {
        int32_t ret = 0;
        time_t start = time(NULL);

        do
        {
            ret = poll(handles, numHandles, timeout);

            if (timeout > 0 && ret < 0)
            {
                const int32_t err = errno;
                const int32_t sec = time(NULL) - start;

                timeout -= sec * 1000;

                if (timeout < 0)
                    timeout = 0;

                errno = err;
            }
        }
#if IL2CPP_TARGET_LINUX
        while (ret == -1 && (errno == EINTR || errno == EAGAIN));
#else
        while (ret == -1 && errno == EINTR);
#endif

#if IL2CPP_TARGET_LINUX
        // On Linux, socket will report POLLERR if the other end has been closed, in addition to normal POLLHUP
        // From man page:
        // POLLERR
        //   Error condition(only returned in revents; ignored in events).
        //   This bit is also set for a file descriptor referring to the
        //   write end of a pipe when the read end has been closed.
        //
        // Mac and Windows doesn't do it, so we zero out that bit if POLLHUP is set on Linux to get consistent behaviour
        for (int i = 0; i < numHandles; i++)
        {
            if ((handles[i].revents & POLLERR) && (handles[i].revents & POLLHUP))
                handles[i].revents &= ~POLLERR & handles[i].events;
        }
#endif

        return ret;
    }
}
}
}

#endif // IL2CPP_TARGET_POSIX
