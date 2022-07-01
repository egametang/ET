#pragma once

#if IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include <pthread.h>
#include <time.h>
#include <sys/poll.h>
#include "os/Thread.h"
#include "os/Socket.h"

namespace il2cpp
{
namespace os
{
namespace posix
{
    inline timespec Ticks100NanosecondsToTimespec(int64_t ticks)
    {
        timespec result;
        result.tv_sec = ticks / 10000000;
        result.tv_nsec = (ticks % 10000000) * 100;
        return result;
    }

    inline timespec MillisecondsToTimespec(uint32_t ms)
    {
        timespec result;
        result.tv_sec = ms / 1000;
        result.tv_nsec = (ms % 1000) * 1000000;
        return result;
    }

    inline Thread::ThreadId PosixThreadIdToThreadId(pthread_t thread)
    {
        Thread::ThreadId threadId = 0;
        memcpy(&threadId, &thread, std::min(sizeof(threadId), sizeof(thread)));
        return threadId;
    }

    struct PosixAutoLock
    {
        pthread_mutex_t* mutex;
        PosixAutoLock(pthread_mutex_t* m)
            : mutex(m) { pthread_mutex_lock(mutex); }
        ~PosixAutoLock()
        { pthread_mutex_unlock(mutex); }
    };


    inline short PollFlagsToPollEvents(PollFlags flags)
    {
        return (short)flags;
    }

    inline PollFlags PollEventsToPollFlags(short events)
    {
        return (PollFlags)events;
    }

    int Poll(pollfd* handles, int numHandles, int timeout);
}
}
}

#endif // IL2CPP_TARGET_POSIX
