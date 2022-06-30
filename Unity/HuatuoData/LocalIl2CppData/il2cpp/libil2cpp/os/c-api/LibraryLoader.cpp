#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/LibraryLoader-c-api.h"
#include "os/LibraryLoader.h"
#include "utils/BaselibHandleUtils.h"
#include "utils/StringUtils.h"
#include "utils/StringViewUtils.h"

extern "C"
{
    void* UnityPalLibraryLoaderLoadDynamicLibrary(const char* nativeDynamicLibrary, int /*flags*/)
    {
        Il2CppNativeString libName(il2cpp::utils::StringUtils::Utf8ToNativeString(nativeDynamicLibrary));
        std::string detailedError;
        auto handle = il2cpp::os::LibraryLoader::LoadDynamicLibrary(STRING_TO_STRINGVIEW(libName), detailedError);
        return il2cpp::utils::BaselibHandleUtils::HandleToVoidPtr(handle);
    }

    void UnityPalLibraryLoaderCleanupLoadedLibraries()
    {
        il2cpp::os::LibraryLoader::CleanupLoadedLibraries();
    }

    UnityPalMethodPointer UnityPalLibraryLoaderGetFunctionPointer(void* dynamicLibrary, const char* functionName)
    {
        std::string detailedError;
        auto handle = il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(dynamicLibrary);
        return il2cpp::os::LibraryLoader::GetFunctionPointer(handle, functionName, detailedError);
    }

    int32_t UnityPalLibraryLoaderCloseLoadedLibrary(void** dynamicLibrary)
    {
        IL2CPP_ASSERT(*dynamicLibrary);
        auto handle = il2cpp::utils::BaselibHandleUtils::VoidPtrToHandle<Baselib_DynamicLibrary_Handle>(*dynamicLibrary);
        return il2cpp::os::LibraryLoader::CloseLoadedLibrary(handle);
    }
}

#endif
