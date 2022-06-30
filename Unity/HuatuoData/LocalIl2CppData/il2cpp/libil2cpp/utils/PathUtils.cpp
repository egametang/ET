#include "il2cpp-config.h"
#include "utils/PathUtils.h"
#include <string>

namespace il2cpp
{
namespace utils
{
namespace PathUtils
{
    std::string BasenameNoExtension(const std::string& path)
    {
        if (path.empty())
            return ".";

        std::string base = Basename(path);

        const size_t pos = base.rfind('.');

        // No extension.
        if (pos == std::string::npos)
            return base;

        return base.substr(0, pos);
    }

    std::string PathNoExtension(const std::string& path)
    {
        const size_t pos = path.rfind('.');

        // No extension.
        if (pos == std::string::npos)
            return path;

        return path.substr(0, pos);
    }
}
} /* utils */
} /* il2cpp */
