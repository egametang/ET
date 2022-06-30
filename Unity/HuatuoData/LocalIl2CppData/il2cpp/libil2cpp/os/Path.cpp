#include "il2cpp-config.h"
#include "os/Path.h"
#include <string>

#if IL2CPP_USE_GENERIC_ENVIRONMENT
#include "os/Path.h"
#include <string>

namespace il2cpp
{
namespace os
{
    std::string Path::GetExecutablePath()
    {
        return std::string("<NotImplemented>");
    }

    std::string Path::GetTempPath()
    {
        return std::string("<NotImplemented>");
    }

    bool Path::IsAbsolute(const std::string& path)
    {
        return false;
    }
}
}

#endif
