#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include <errno.h>

#include "os/LastError.h"

namespace il2cpp
{
namespace os
{
    uint32_t LastError::GetLastError()
    {
        return errno;
    }
} /* namespace os */
} /* namespace il2cpp*/

#endif
