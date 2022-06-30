#include "il2cpp-config.h"
#include "vm/Path.h"
#include "os/Path.h"

namespace il2cpp
{
namespace vm
{
    static std::string s_TempPath;

    void Path::SetTempPath(const char* path)
    {
        s_TempPath = path;
    }

    std::string Path::GetTempPath()
    {
        if (!s_TempPath.empty())
            return s_TempPath;
        return os::Path::GetTempPath();
    }
} /* namespace vm */
} /* namespace il2cpp */
