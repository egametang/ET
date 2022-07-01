#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX

//#define VERBOSE_OUTPUT

#ifdef VERBOSE_OUTPUT
#include <stdio.h>
#endif

#include <dlfcn.h>
#include <string>
#include <set>
#if IL2CPP_TARGET_LINUX
#include <unistd.h>
#include <gnu/lib-names.h>
#endif

#include "il2cpp-runtime-metadata.h"
#include "os/LibraryLoader.h"
#include "os/Mutex.h"
#include "utils/PathUtils.h"
#include "utils/StringUtils.h"
#include "utils/Environment.h"

#if !RUNTIME_TINY
#include "Baselib.h"
#include "Cpp/ReentrantLock.h"
#endif

namespace il2cpp
{
namespace os
{
#if !RUNTIME_TINY
    static std::set<void*> s_NativeHandlesOpen;
    typedef std::set<void*>::const_iterator OpenHandleIterator;
    baselib::ReentrantLock s_NativeHandlesOpenMutex;
#endif

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

// Note that testing this code can be a bit difficult, since dlopen will cache
// the values it returns, and we don't call dlcose from the C# level. See the
// comments in the integration test code for more details.

    static void* LoadLibraryWithName(const char* name, int flags)
    {
#ifdef VERBOSE_OUTPUT
        printf("Trying name: %s\n", name);
#endif

        void* handle = NULL;
#if IL2CPP_TARGET_IOS
        std::string dirName;
        if (utils::Environment::GetNumMainArgs() > 0)
        {
            std::string main = utils::StringUtils::Utf16ToUtf8(utils::Environment::GetMainArgs()[0]);
            dirName = utils::PathUtils::DirectoryName(main);
        }

        std::string libPath = utils::StringUtils::Printf("%s/%s", dirName.c_str(), name);
        handle = dlopen(libPath.c_str(), flags);

        // Fallback to just using the name. This might be a system dylib.
        if (handle == NULL)
            handle = dlopen(name, flags);
#else
        handle = dlopen(name, flags);
#endif

        if (handle != NULL)
            return handle;

#ifdef VERBOSE_OUTPUT
        printf("Error: %s\n", dlerror());
#endif

        return NULL;
    }

#if IL2CPP_TARGET_LINUX
    static void* LoadLibraryRelativeToExecutableDirectory(const char* name, int flags)
    {
        if (name == NULL || name[0] == '/')
            return NULL;

        char exePath[PATH_MAX + 1];
        int len;

        if ((len = readlink("/proc/self/exe", exePath, sizeof(exePath))) == -1)
        {
#ifdef VERBOSE_OUTPUT
            printf("Error: %s\n", perror());
#endif
            return NULL;
        }
        exePath[len] = '\0'; // readlink does not terminate buffer
        while (len > 0 && exePath[len] != '/')
            len--;
        exePath[len] = '\0';
        std::string libPath = utils::StringUtils::Printf("%s/%s", exePath, name);
        return dlopen(libPath.c_str(), flags);
    }

#endif

    static void* CheckLibraryVariations(const char* name, int flags)
    {
        int numberOfVariations = sizeof(LibraryNamePrefixAndSuffixVariations) / sizeof(LibraryNamePrefixAndSuffixVariations[0]);
        for (int i = 0; i < numberOfVariations; ++i)
        {
            std::string libraryName = LibraryNamePrefixAndSuffixVariations[i].prefix + name + LibraryNamePrefixAndSuffixVariations[i].suffix;
            void* handle = LoadLibraryWithName(libraryName.c_str(), flags);
            if (handle != NULL)
                return handle;
#if IL2CPP_TARGET_LINUX
            // Linux does not search current directory by default
            handle = LoadLibraryRelativeToExecutableDirectory(libraryName.c_str(), flags);
            if (handle != NULL)
                return handle;
#endif
        }

        return NULL;
    }

    Il2CppMethodPointer LibraryLoader::GetHardcodedPInvokeDependencyFunctionPointer(const il2cpp::utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, const il2cpp::utils::StringView<char>& entryPoint, Il2CppCharSet charSet)
    {
        return NULL;
    }

    void* LibraryLoader::LoadDynamicLibraryImpl(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary)
    {
        return LoadDynamicLibraryImpl(nativeDynamicLibrary, RTLD_LAZY);
    }

    void* LibraryLoader::LoadDynamicLibraryImpl(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, int flags)
    {
#ifdef VERBOSE_OUTPUT
        printf("Attempting to load dynamic library: %s\n", nativeDynamicLibrary.Str());
#endif

        if (nativeDynamicLibrary.IsEmpty())
            return LoadLibraryWithName(NULL, flags);

#if IL2CPP_TARGET_LINUX
        // Workaround the fact that on Linux, libc is actually named libc.so.6 instead of libc.so.
        // mscorlib P/Invokes into plain libc, so we need this for those P/Invokes to succeed
        if (strncasecmp(nativeDynamicLibrary.Str(), "libc", nativeDynamicLibrary.Length()) == 0)
            return LoadLibraryWithName(LIBC_SO, flags);
#endif

        StringViewAsNullTerminatedStringOf(char, nativeDynamicLibrary, libraryName);
        void* handle = LoadLibraryWithName(libraryName, flags);

        if (handle == NULL)
            handle = CheckLibraryVariations(libraryName, flags);

        if (handle == NULL)
        {
            size_t lengthWithoutDotDll = nativeDynamicLibrary.Length() - 4;
            if (strncmp(libraryName + lengthWithoutDotDll, ".dll", 4) == 0)
            {
                char* nativeDynamicLibraryWithoutExtension = static_cast<char*>(alloca((lengthWithoutDotDll + 1) * sizeof(char)));
                memcpy(nativeDynamicLibraryWithoutExtension, libraryName, lengthWithoutDotDll);
                nativeDynamicLibraryWithoutExtension[lengthWithoutDotDll] = 0;

                handle = CheckLibraryVariations(nativeDynamicLibraryWithoutExtension, flags);
            }
        }

#if !RUNTIME_TINY
        os::FastAutoLock lock(&s_NativeHandlesOpenMutex);
        if (handle != NULL)
            s_NativeHandlesOpen.insert(handle);
#endif

        return handle;
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const PInvokeArguments& pinvokeArgs)
    {
        StringViewAsNullTerminatedStringOf(char, pinvokeArgs.entryPoint, entryPoint);
#if RUNTIME_TINY
        return reinterpret_cast<Il2CppMethodPointer>(dlsym(dynamicLibrary, entryPoint));
#else

        if (pinvokeArgs.isNoMangle)
            return reinterpret_cast<Il2CppMethodPointer>(dlsym(dynamicLibrary, entryPoint));

        const size_t kBufferOverhead = 10;
        void* functionPtr = NULL;
        size_t originalFuncNameLength = strlen(entryPoint) + 1;
        std::string functionName;

        functionName.resize(originalFuncNameLength + kBufferOverhead + 1); // Let's index the string from '1', because we might have to prepend an underscore in case of stdcall mangling
        memcpy(&functionName[1], entryPoint, originalFuncNameLength);
        memset(&functionName[1] + originalFuncNameLength, 0, kBufferOverhead);

        // If there's no 'dont mangle' flag set, 'W' function takes priority over original name, but 'A' function does not (yes, really)
        if (pinvokeArgs.charSet == CHARSET_UNICODE)
        {
            functionName[originalFuncNameLength] = 'W';
            functionPtr = dlsym(dynamicLibrary, functionName.c_str() + 1);
            if (functionPtr != NULL)
                return reinterpret_cast<Il2CppMethodPointer>(functionPtr);

            // If charset specific function lookup failed, try with original name
            functionPtr = dlsym(dynamicLibrary, entryPoint);
        }
        else
        {
            functionPtr = dlsym(dynamicLibrary, entryPoint);
            if (functionPtr != NULL)
                return reinterpret_cast<Il2CppMethodPointer>(functionPtr);

            // If original name function lookup failed, try with mangled name
            functionName[originalFuncNameLength] = 'A';
            functionPtr = dlsym(dynamicLibrary, functionName.c_str() + 1);
        }

        return reinterpret_cast<Il2CppMethodPointer>(functionPtr);
#endif
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const char* functionName)
    {
#ifdef VERBOSE_OUTPUT
        printf("Attempting to load method at entry point: %s\n", functionName);
#endif

        Il2CppMethodPointer method = reinterpret_cast<Il2CppMethodPointer>(dlsym(dynamicLibrary, functionName));

#ifdef VERBOSE_OUTPUT
        if (method == NULL)
            printf("Error: %s\n", dlerror());
#endif

        return method;
    }

    void LibraryLoader::CleanupLoadedLibraries()
    {
#if !RUNTIME_TINY
        os::FastAutoLock lock(&s_NativeHandlesOpenMutex);
        for (OpenHandleIterator it = s_NativeHandlesOpen.begin(); it != s_NativeHandlesOpen.end(); it++)
        {
            dlclose(*it);
        }
#endif
    }

    bool LibraryLoader::CloseLoadedLibrary(void*& dynamicLibrary)
    {
        if (dynamicLibrary == NULL)
            return false;

#if !RUNTIME_TINY
        os::FastAutoLock lock(&s_NativeHandlesOpenMutex);
        OpenHandleIterator it = s_NativeHandlesOpen.find(dynamicLibrary);
        if (it != s_NativeHandlesOpen.end())
        {
            dlclose(*it);
            s_NativeHandlesOpen.erase(it);
            return true;
        }
        return false;
#else
        return true;
#endif
    }
}
}

#endif
