#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_ENVIRONMENT && IL2CPP_TARGET_POSIX && !RUNTIME_TINY

#include "os/Encoding.h"

#ifdef HAVE_LANGINFO_H
#include <langinfo.h>
#endif

namespace il2cpp
{
namespace os
{
namespace Encoding
{
    std::string GetCharSet()
    {
#if HAVE_LANGINFO_H
        return nl_langinfo(CODESET);
#else
        return "UTF-8";
#endif
    }
}
}
}

#endif
