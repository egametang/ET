#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS
#include "WindowsHelpers.H"

#include "os/Encoding.h"

namespace il2cpp
{
namespace os
{
namespace Encoding
{
    std::string GetCharSet()
    {
        static char buf[14];
        sprintf(buf, "CP%u", GetACP());
        return std::string(buf);
    }
}
}
}

#endif
