#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

#include <dlfcn.h>
#include <string>
#include <set>
#if IL2CPP_TARGET_LINUX
#include <unistd.h>
#include <gnu/lib-names.h>
#endif

#include "il2cpp-runtime-metadata.h"
#include "os/LibraryLoader.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"
#include "utils/Environment.h"

namespace il2cpp
{
namespace os
{
    struct LibraryNamePrefixAndSuffix
    {
        LibraryNamePrefixAndSuffix(const char* prefix_, const char* suffix_)
        {
            prefix = std::string(prefix_);
            suffix = std::string(suffix_);
        }

        std::string prefix;
        std::string suffix;
    };

    static LibraryNamePrefixAndSuffix LibraryNamePrefixAndSuffixVariations[8] =
    {
        LibraryNamePrefixAndSuffix("", ".so"),
        LibraryNamePrefixAndSuffix("", ".dll"),
        LibraryNamePrefixAndSuffix("", ".dylib"),
        LibraryNamePrefixAndSuffix("", ".bundle"),
        LibraryNamePrefixAndSuffix("lib", ".so"),
        LibraryNamePrefixAndSuffix("lib", ".dll"),
        LibraryNamePrefixAndSuffix("lib", ".dylib"),
        LibraryNamePrefixAndSuffix("lib", ".bundle")
    };

    const HardcodedPInvokeDependencyLibrary* LibraryLoader::HardcodedPInvokeDependencies = NULL;
    const size_t LibraryLoader::HardcodedPInvokeDependenciesCount = 0;

    static Baselib_DynamicLibrary_Handle LoadLibraryWithName(const char* name, std::string& detailedError)
    {
#if IL2CPP_TARGET_IOS
        std::string dirName;
        if (utils::Environment::GetNumMainArgs() > 0)
        {
            std::string main = utils::StringUtils::Utf16ToUtf8(utils::Environment::GetMainArgs()[0]);
            dirName = utils::PathUtils::DirectoryName(main);
        }

        std::string libPath = utils::StringUtils::Printf("%s/%s", dirName.c_str(), name);
        auto errorState = Baselib_ErrorState_Create();
        auto handle = LibraryLoader::TryOpeningLibrary(libPath.c_str(), detailedError);
        if (handle != Baselib_DynamicLibrary_Handle_Invalid)
            return handle;

        // Fallback to just using the name. This might be a system dylib.
        return LibraryLoader::TryOpeningLibrary(name, detailedError);
#else
        return LibraryLoader::TryOpeningLibrary(name, detailedError);
#endif
    }

#if IL2CPP_TARGET_LINUX
    static Baselib_DynamicLibrary_Handle LoadLibraryRelativeToExecutableDirectory(const char* name, std::string& detailedError)
    {
        if (name == NULL || name[0] == '/')
            return Baselib_DynamicLibrary_Handle_Invalid;

        char exePath[PATH_MAX + 1];
        int len;

        if ((len = readlink("/proc/self/exe", exePath, sizeof(exePath))) == -1)
        {
            return Baselib_DynamicLibrary_Handle_Invalid;
        }
        exePath[len] = '\0'; // readlink does not terminate buffer
        while (len > 0 && exePath[len] != '/')
            len--;
        exePath[len] = '\0';
        std::string libPath = utils::StringUtils::Printf("%s/%s", exePath, name);

        return LibraryLoader::TryOpeningLibrary(libPath.c_str(), detailedError);
    }

#endif

    static Baselib_DynamicLibrary_Handle CheckLibraryVariations(const char* name, std::string& detailedError)
    {
        const int numberOfVariations = sizeof(LibraryNamePrefixAndSuffixVariations) / sizeof(LibraryNamePrefixAndSuffixVariations[0]);
        for (int i = 0; i < numberOfVariations; ++i)
        {
            std::string libraryName = LibraryNamePrefixAndSuffixVariations[i].prefix + name + LibraryNamePrefixAndSuffixVariations[i].suffix;
            auto handle = LoadLibraryWithName(libraryName.c_str(), detailedError);
            if (handle != Baselib_DynamicLibrary_Handle_Invalid)
                return handle;
#if IL2CPP_TARGET_LINUX
            // Linux does not search current directory by default
            handle = LoadLibraryRelativeToExecutableDirectory(libraryName.c_str(), detailedError);
            if (handle != Baselib_DynamicLibrary_Handle_Invalid)
                return handle;
#endif
        }

        return Baselib_DynamicLibrary_Handle_Invalid;
    }

    Baselib_DynamicLibrary_Handle LibraryLoader::ProbeForLibrary(const Il2CppNativeChar* libraryName, const size_t libraryNameLength, std::string& detailedError)
    {
        auto handle = Baselib_DynamicLibrary_Handle_Invalid;

#if IL2CPP_TARGET_LINUX
        // Workaround the fact that on Linux, libc is actually named libc.so.6 instead of libc.so.
        // mscorlib P/Invokes into plain libc, so we need this for those P/Invokes to succeed
        if (strncasecmp(libraryName, "libc", libraryNameLength) == 0)
            handle = LoadLibraryWithName(LIBC_SO, detailedError);
#endif

        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
            handle = LoadLibraryWithName(libraryName, detailedError);

        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
            handle = CheckLibraryVariations(libraryName, detailedError);

        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
        {
            const size_t lengthWithoutDotDll = libraryNameLength - 4;
            if (strncmp(libraryName + lengthWithoutDotDll, ".dll", 4) == 0)
            {
                char* nativeDynamicLibraryWithoutExtension = static_cast<char*>(alloca((lengthWithoutDotDll + 1) * sizeof(char)));
                memcpy(nativeDynamicLibraryWithoutExtension, libraryName, lengthWithoutDotDll);
                nativeDynamicLibraryWithoutExtension[lengthWithoutDotDll] = 0;

                handle = CheckLibraryVariations(nativeDynamicLibraryWithoutExtension, detailedError);
            }
        }

        return handle;
    }

    Baselib_DynamicLibrary_Handle LibraryLoader::OpenProgramHandle(Baselib_ErrorState& errorState, bool& /*needsClosing*/)
    {
        return Baselib_DynamicLibrary_OpenProgramHandle(&errorState);
    }
}
}

#endif
