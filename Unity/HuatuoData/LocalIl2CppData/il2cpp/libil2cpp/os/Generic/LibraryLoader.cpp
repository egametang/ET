#include "il2cpp-config.h"


#if !IL2CPP_TARGET_WINDOWS

#include "os/LibraryLoader.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    bool LibraryLoader::EntryNameMatches(const il2cpp::utils::StringView<char>& hardcodedEntryPoint, const il2cpp::utils::StringView<char>& entryPoint)
    {
        return hardcodedEntryPoint.Length() == entryPoint.Length() && strncmp(hardcodedEntryPoint.Str(), entryPoint.Str(), entryPoint.Length()) == 0;
    }
}
}

#endif
