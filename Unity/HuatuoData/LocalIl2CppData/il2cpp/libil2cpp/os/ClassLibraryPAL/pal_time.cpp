#include "il2cpp-config.h"
#include "pal_platform.h"

#if IL2CPP_USES_POSIX_CLASS_LIBRARY_PAL

struct TimeValPair;
#include <sys/time.h>
#include <errno.h>

extern "C"
{
    // Items needed by mscorlib
    IL2CPP_EXPORT int32_t SystemNative_UTimes(const char* path, TimeValPair* times);
}

typedef struct TimeValPair
{
    int64_t AcTimeSec;
    int64_t AcTimeUSec;
    int64_t ModTimeSec;
    int64_t ModTimeUSec;
} TimeValPair;

static void ConvertTimeValPair(const TimeValPair* pal, struct timeval native[2])
{
    native[0].tv_sec = (long)(pal->AcTimeSec);
    native[0].tv_usec = (long)(pal->AcTimeUSec);
    native[1].tv_sec = (long)(pal->ModTimeSec);
    native[1].tv_usec = (long)(pal->ModTimeUSec);
}

template<typename TInt>
static inline bool CheckInterrupted(TInt result)
{
    return result < 0 && errno == EINTR;
}

int32_t SystemNative_UTimes(const char* path, TimeValPair* times)
{
    IL2CPP_ASSERT(times != NULL);

    struct timeval temp[2];
    ConvertTimeValPair(times, temp);

    int32_t result;
    while (CheckInterrupted(result = utimes(path, temp)))
        ;
    return result;
}

#endif
