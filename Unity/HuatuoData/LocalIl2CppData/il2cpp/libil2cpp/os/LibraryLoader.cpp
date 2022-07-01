#include "LibraryLoader.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    static Il2CppSetFindPlugInCallback s_FindPluginCallback = NULL;

    void* LibraryLoader::LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar> nativeDynamicLibrary)
    {
        return LoadDynamicLibrary(nativeDynamicLibrary, 0);
    }

    void* LibraryLoader::LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar> nativeDynamicLibrary, int flags)
    {
        if (s_FindPluginCallback)
        {
            StringViewAsNullTerminatedStringOf(Il2CppNativeChar, nativeDynamicLibrary, libraryName);
            const Il2CppNativeChar* modifiedLibraryName = s_FindPluginCallback(libraryName);

            if (modifiedLibraryName != libraryName)
            {
                utils::StringView<Il2CppNativeChar> modifiedDynamicLibrary(modifiedLibraryName, utils::StringUtils::StrLen(modifiedLibraryName));
                return os::LibraryLoader::LoadDynamicLibraryImpl(modifiedDynamicLibrary);
            }
        }

        return os::LibraryLoader::LoadDynamicLibraryImpl(nativeDynamicLibrary);
    }

    void LibraryLoader::SetFindPluginCallback(Il2CppSetFindPlugInCallback method)
    {
        s_FindPluginCallback = method;
    }
} /* namespace vm */
} /* namespace il2cpp */
