#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/LibraryLoader-c-api.h"
#include "os/LibraryLoader.h"
#include "utils/StringUtils.h"
#include "utils/StringViewUtils.h"

extern "C"
{
    void* UnityPalLibraryLoaderLoadDynamicLibrary(const char* nativeDynamicLibrary, int flags)
    {
        Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(nativeDynamicLibrary));
        return il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), flags);
    }

    void UnityPalLibraryLoaderCleanupLoadedLibraries()
    {
        il2cpp::os::LibraryLoader::CleanupLoadedLibraries();
    }

    UnityPalMethodPointer UnityPalLibraryLoaderGetFunctionPointer(void* dynamicLibrary, const char* functionName)
    {
        return il2cpp::os::LibraryLoader::GetFunctionPointer(dynamicLibrary, functionName);
    }

    int32_t UnityPalLibraryLoaderCloseLoadedLibrary(void** dynamicLibrary)
    {
        IL2CPP_ASSERT(*dynamicLibrary);
        return il2cpp::os::LibraryLoader::CloseLoadedLibrary(*dynamicLibrary);
    }
}

#endif
