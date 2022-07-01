#include "il2cpp-config.h"
#include "os/Path.h"
#include "os/Environment.h"
#include "utils/Runtime.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"
#include "utils/Environment.h"

namespace il2cpp
{
namespace utils
{
    NORETURN void Runtime::Abort()
    {
        os::Environment::Abort();
    }

    static std::string s_DataDirFallback;

    static std::string s_DataDir;

    void Runtime::SetDataDir(const char *path)
    {
        s_DataDir = path;
    }

    std::string Runtime::GetDataDir()
    {
#if !RUNTIME_TINY
        // use explicit value if set
        if (s_DataDir.size() > 0)
            return s_DataDir;

        std::string executablePath = os::Path::GetExecutablePath();
        if (!executablePath.empty())
            return PathUtils::Combine(PathUtils::DirectoryName(executablePath), StringView<char>(IL2CPP_DEFAULT_DATA_DIR_PATH_STR));

        if (s_DataDirFallback.size() == 0 && Environment::GetNumMainArgs() > 0)
        {
            std::string main = StringUtils::Utf16ToUtf8(Environment::GetMainArgs()[0]);
            s_DataDirFallback = PathUtils::DirectoryName(main);
        }

        return s_DataDirFallback;
#else
        return std::string();
#endif
    }
} // utils
} // il2cpp
