#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "il2cpp-runtime-metadata.h"
#include "os/Mutex.h"
#include "os/LibraryLoader.h"
#include "os/Image.h"
#include "utils/StringUtils.h"

#include "WindowsHelpers.h"
#include "Evntprov.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

#define WINNT // All functions in Evntrace.h are under this define.. Why? I have no idea!
#include "Evntrace.h"

namespace il2cpp
{
namespace os
{
    static std::vector<std::pair<std::wstring, HMODULE> > s_NativeDllCache;
    typedef std::vector<std::pair<std::wstring, HMODULE> >::const_iterator DllCacheIterator;
    baselib::ReentrantLock s_NativeDllCacheMutex;

#define HARDCODED_DEPENDENCY_LIBRARY(libraryName, libraryFunctions) { libraryName, ARRAYSIZE(libraryFunctions), libraryFunctions }
#define HARDCODED_DEPENDENCY_FUNCTION(function) { #function, reinterpret_cast<Il2CppMethodPointer>(function), IL2CPP_ARRAY_SIZE(#function)-1  }

    struct HardcodedPInvokeDependencyFunction
    {
        const char* functionName;
        Il2CppMethodPointer functionPointer;
        size_t functionNameLen;
    };

    struct HardcodedPInvokeDependencyLibrary
    {
        const wchar_t* libraryName;
        size_t functionCount;
        const HardcodedPInvokeDependencyFunction* functions;
    };

#if !IL2CPP_TARGET_WINDOWS_DESKTOP
    const HardcodedPInvokeDependencyFunction kAdvapiFunctions[] =
    {
#if !IL2CPP_TARGET_XBOXONE
#if WINDOWS_SDK_BUILD_VERSION >= 16299
        HARDCODED_DEPENDENCY_FUNCTION(EnumerateTraceGuidsEx),
#endif
        HARDCODED_DEPENDENCY_FUNCTION(EventActivityIdControl),
#endif
        HARDCODED_DEPENDENCY_FUNCTION(EventRegister),
        HARDCODED_DEPENDENCY_FUNCTION(EventSetInformation),
        HARDCODED_DEPENDENCY_FUNCTION(EventUnregister),
        HARDCODED_DEPENDENCY_FUNCTION(EventWrite),
#if !IL2CPP_TARGET_XBOXONE
        HARDCODED_DEPENDENCY_FUNCTION(EventWriteEx),
        HARDCODED_DEPENDENCY_FUNCTION(EventWriteString),
        HARDCODED_DEPENDENCY_FUNCTION(EventWriteTransfer),
#endif
    };
#endif

    const HardcodedPInvokeDependencyFunction kKernel32Functions[] =
    {
        HARDCODED_DEPENDENCY_FUNCTION(FormatMessageW),
        HARDCODED_DEPENDENCY_FUNCTION(GetCurrentProcessId),
        HARDCODED_DEPENDENCY_FUNCTION(GetDynamicTimeZoneInformation),
        HARDCODED_DEPENDENCY_FUNCTION(GetNativeSystemInfo),
        HARDCODED_DEPENDENCY_FUNCTION(GetTimeZoneInformation),
        HARDCODED_DEPENDENCY_FUNCTION(GetFullPathNameW),
    };

    const HardcodedPInvokeDependencyFunction kiphlpapiFunctions[] =
    {
        HARDCODED_DEPENDENCY_FUNCTION(GetNetworkParams),
#if !IL2CPP_TARGET_XBOXONE
        HARDCODED_DEPENDENCY_FUNCTION(GetAdaptersAddresses),
        HARDCODED_DEPENDENCY_FUNCTION(GetIfEntry),
#endif
    };

#if !IL2CPP_TARGET_WINDOWS_DESKTOP && !IL2CPP_TARGET_WINDOWS_GAMES
    const HardcodedPInvokeDependencyFunction kTimezoneFunctions[] =
    {
#if !IL2CPP_TARGET_XBOXONE
        HARDCODED_DEPENDENCY_FUNCTION(EnumDynamicTimeZoneInformation),
#endif
        HARDCODED_DEPENDENCY_FUNCTION(GetDynamicTimeZoneInformation),
#if !IL2CPP_TARGET_XBOXONE
        HARDCODED_DEPENDENCY_FUNCTION(GetDynamicTimeZoneInformationEffectiveYears),
#endif
        HARDCODED_DEPENDENCY_FUNCTION(GetTimeZoneInformationForYear),
    };
#endif

#if IL2CPP_TARGET_WINRT
    const HardcodedPInvokeDependencyFunction kWinTypesFunctions[] =
    {
        HARDCODED_DEPENDENCY_FUNCTION(RoGetBufferMarshaler)
    };
#endif

// All these come without ".dll" extension!
    const HardcodedPInvokeDependencyLibrary kHardcodedPInvokeDependencies[] =
    {
#if !IL2CPP_TARGET_WINDOWS_DESKTOP && !IL2CPP_TARGET_WINDOWS_GAMES // Some of these functions are win8+
        HARDCODED_DEPENDENCY_LIBRARY(L"advapi32", kAdvapiFunctions),
        HARDCODED_DEPENDENCY_LIBRARY(L"api-ms-win-core-timezone-l1-1-0", kTimezoneFunctions),
#endif
        HARDCODED_DEPENDENCY_LIBRARY(L"kernel32", kKernel32Functions),
        HARDCODED_DEPENDENCY_LIBRARY(L"iphlpapi", kiphlpapiFunctions),
#if IL2CPP_TARGET_WINRT // Win8+, plus needs to be looked up dynamically on Xbox One
        HARDCODED_DEPENDENCY_LIBRARY(L"wintypes", kWinTypesFunctions),
#endif
    };

    inline static wchar_t AsciiToLower(wchar_t c)
    {
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 'a';

        return c;
    }

    static bool DoesNativeDynamicLibraryNameMatch(const il2cpp::utils::StringView<Il2CppNativeChar>& desiredLibraryName, const wchar_t* hardcodedLibraryName)
    {
        size_t desiredLibraryNameLength = desiredLibraryName.Length();
        for (size_t i = 0; i < desiredLibraryNameLength; i++)
        {
            wchar_t desiredCharacter = AsciiToLower(desiredLibraryName[i]);
            wchar_t hardcodedCharacter = hardcodedLibraryName[i];

            // Assume hardcodedLibraryName consists of only lower case ascii characters
            IL2CPP_ASSERT(hardcodedCharacter < 128 && (hardcodedCharacter<'A' || hardcodedCharacter> 'Z'));

            if (desiredCharacter != hardcodedCharacter)
            {
                // If we've reached end of our hardcoded dll name, it can still match if we've
                // reached end of desiredLibraryName file name and only the extension is left
                return hardcodedCharacter == 0 &&
                    i + 4 == desiredLibraryNameLength &&
                    desiredLibraryName[i] == '.' &&
                    AsciiToLower(desiredLibraryName[i + 1]) == 'd' &&
                    AsciiToLower(desiredLibraryName[i + 2]) == 'l' &&
                    AsciiToLower(desiredLibraryName[i + 3]) == 'l';
            }
            else if (hardcodedCharacter == 0)
            {
                // We've reached the end of hardcoded library name
                // It's a match if we're at the end of desired library name too
                return i + 1 == desiredLibraryNameLength;
            }
            else if (i == desiredLibraryNameLength - 1)
            {
                // We've reached the end of desired library name
                // It's a match if we're at the end of hardcoded library name too
                return hardcodedLibraryName[i + 1] == 0;
            }
        }

        // We've reached the end of desired library name,
        // but not the end of hardcoded library name.
        // It is not a match.
        return false;
    }

    Il2CppMethodPointer LibraryLoader::GetHardcodedPInvokeDependencyFunctionPointer(const il2cpp::utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, const il2cpp::utils::StringView<char>& entryPoint, Il2CppCharSet charSet)
    {
        // We don't support, nor do we need to Ansi functions.  That would break forwarding method names to Unicode MoveFileEx -> MoveFileExW
        if (charSet == CHARSET_ANSI)
            return NULL;

        for (int i = 0; i < ARRAYSIZE(kHardcodedPInvokeDependencies); i++)
        {
            const HardcodedPInvokeDependencyLibrary& library = kHardcodedPInvokeDependencies[i];
            if (DoesNativeDynamicLibraryNameMatch(nativeDynamicLibrary, library.libraryName))
            {
                size_t functionCount = library.functionCount;
                for (size_t j = 0; j < functionCount; j++)
                {
                    const HardcodedPInvokeDependencyFunction function = library.functions[j];

                    if (EntryNameMatches(il2cpp::utils::StringView<char>(function.functionName, function.functionNameLen), entryPoint))
                        return function.functionPointer;
                }

                // We assume that kHardcodedPInvokeDependencies will not contain duplicates
                return NULL;
            }
        }

        return NULL;
    }

    void* LibraryLoader::LoadDynamicLibraryImpl(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary)
    {
        return LoadDynamicLibraryImpl(nativeDynamicLibrary, 0);
    }

    void* LibraryLoader::LoadDynamicLibraryImpl(const utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, int flags)
    {
        if (nativeDynamicLibrary.IsEmpty())
            return (HMODULE)Image::GetImageBase();

        {
            os::FastAutoLock lock(&s_NativeDllCacheMutex);

            for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
            {
                if (it->first.compare(0, std::string::npos, nativeDynamicLibrary.Str(), nativeDynamicLibrary.Length()) == 0)
                    return it->second;
            }
        }

        std::wstring dllName(nativeDynamicLibrary.Str(), nativeDynamicLibrary.Length());
        HMODULE module = ::LoadLibraryW(dllName.c_str());

        if (module != NULL)
        {
            os::FastAutoLock lock(&s_NativeDllCacheMutex);
            s_NativeDllCache.push_back(std::make_pair(std::move(dllName), module));
        }

        return module;
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const PInvokeArguments& pinvokeArgs)
    {
        if (dynamicLibrary == NULL)
            return NULL;

        HMODULE module = (HMODULE)dynamicLibrary;
        StringViewAsNullTerminatedStringOf(char, pinvokeArgs.entryPoint, entryPoint);

        // If there's 'no mangle' flag set, just return directly what GetProcAddress returns
        if (pinvokeArgs.isNoMangle)
            return reinterpret_cast<Il2CppMethodPointer>(GetProcAddress(module, entryPoint));

        const size_t kBufferOverhead = 10;
        FARPROC functionPtr = NULL;
        size_t originalFuncNameLength = strlen(entryPoint) + 1;
        std::string functionName;

        functionName.resize(originalFuncNameLength + kBufferOverhead + 1); // Let's index the string from '1', because we might have to prepend an underscore in case of stdcall mangling
        memcpy(&functionName[1], entryPoint, originalFuncNameLength);
        ZeroMemory(&functionName[1] + originalFuncNameLength, kBufferOverhead);

        // If there's no 'dont mangle' flag set, 'W' function takes priority over original name, but 'A' function does not (yes, really)
        if (pinvokeArgs.charSet == CHARSET_UNICODE)
        {
            functionName[originalFuncNameLength] = 'W';
            functionPtr = GetProcAddress(module, functionName.c_str() + 1);
            if (functionPtr != NULL)
                return reinterpret_cast<Il2CppMethodPointer>(functionPtr);

            // If charset specific function lookup failed, try with original name
            functionPtr = GetProcAddress(module, entryPoint);
        }
        else
        {
            functionPtr = GetProcAddress(module, entryPoint);
            if (functionPtr != NULL)
                return reinterpret_cast<Il2CppMethodPointer>(functionPtr);

            // If original name function lookup failed, try with mangled name
            functionName[originalFuncNameLength] = 'A';
            functionPtr = GetProcAddress(module, functionName.c_str() + 1);
        }

        if (functionPtr != NULL)
            return reinterpret_cast<Il2CppMethodPointer>(functionPtr);

        // If it's not cdecl, try mangling the name
        // THIS ONLY APPLIES TO 32-bit x86!
#if !defined(__arm__)
        if (sizeof(void*) == 4 && pinvokeArgs.callingConvention != IL2CPP_CALL_C)
        {
            functionName[0] = '_';
            sprintf(&functionName[0] + originalFuncNameLength, "@%ld", pinvokeArgs.parameterSize);

            functionPtr = GetProcAddress(module, functionName.c_str());
        }
#endif

        return reinterpret_cast<Il2CppMethodPointer>(functionPtr);
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(void* dynamicLibrary, const char* functionName)
    {
        return reinterpret_cast<Il2CppMethodPointer>(GetProcAddress(static_cast<HMODULE>(dynamicLibrary), functionName));
    }

    void LibraryLoader::CleanupLoadedLibraries()
    {
        for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
        {
            FreeLibrary(it->second);
        }
    }

    bool LibraryLoader::CloseLoadedLibrary(void*& dynamicLibrary)
    {
        if (dynamicLibrary == NULL)
            return false;

        os::FastAutoLock lock(&s_NativeDllCacheMutex);

        for (DllCacheIterator it = s_NativeDllCache.begin(); it != s_NativeDllCache.end(); it++)
        {
            if (it->second == dynamicLibrary)
            {
                FreeLibrary(it->second);
                s_NativeDllCache.erase(it);
                return true;
            }
        }
        return false;
    }

    bool LibraryLoader::EntryNameMatches(const il2cpp::utils::StringView<char>& hardcodedEntryPoint, const il2cpp::utils::StringView<char>& entryPoint)
    {
        // Handle windows mapping generic to unicode methods. e.g. MoveFileEx -> MoveFileExW
        if (hardcodedEntryPoint.Length() == entryPoint.Length() || (hardcodedEntryPoint.Length() - 1 == entryPoint.Length() && hardcodedEntryPoint[hardcodedEntryPoint.Length() - 1] == 'W'))
        {
            return strncmp(hardcodedEntryPoint.Str(), entryPoint.Str(), entryPoint.Length()) == 0;
        }

        return false;
    }
}
}

#endif
