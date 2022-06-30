#include "LibraryLoader.h"
#include "utils/StringUtils.h"
#include "utils/Exception.h"

#if !RUNTIME_TINY
#include "os/Mutex.h"
#endif

namespace il2cpp
{
namespace os
{
    static Il2CppSetFindPlugInCallback s_FindPluginCallback = NULL;

#if !RUNTIME_TINY
    typedef std::vector<std::pair<std::basic_string<Il2CppNativeChar>, Baselib_DynamicLibrary_Handle> > DllCacheContainer;
    typedef DllCacheContainer::const_iterator DllCacheIterator;
    static DllCacheContainer s_DllCache; // If a library does not need to be closed - do not add it to the cache.
    static baselib::ReentrantLock s_DllCacheMutex;
#endif

    static inline Il2CppNativeChar AsciiToLower(Il2CppNativeChar c)
    {
        if (c >= 'A' && c <= 'Z')
            return c - 'A' + 'a';

        return c;
    }

    static bool DoesNativeDynamicLibraryNameMatch(const il2cpp::utils::StringView<Il2CppNativeChar>& desiredLibraryName, const Il2CppNativeChar* hardcodedLibraryName)
    {
        size_t desiredLibraryNameLength = desiredLibraryName.Length();
        for (size_t i = 0; i < desiredLibraryNameLength; i++)
        {
            Il2CppNativeChar desiredCharacter = AsciiToLower(desiredLibraryName[i]);
            Il2CppNativeChar hardcodedCharacter = hardcodedLibraryName[i];

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
        if (HardcodedPInvokeDependencies == NULL || charSet == CHARSET_ANSI)
            return NULL;

        for (size_t i = 0; i < HardcodedPInvokeDependenciesCount; i++)
        {
            const HardcodedPInvokeDependencyLibrary& library = HardcodedPInvokeDependencies[i];
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

    Baselib_DynamicLibrary_Handle LibraryLoader::LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar> nativeDynamicLibrary, std::string& detailedError)
    {
        StringViewAsNullTerminatedStringOf(Il2CppNativeChar, nativeDynamicLibrary, libraryName);
        if (s_FindPluginCallback)
            libraryName = s_FindPluginCallback(libraryName);
        auto libraryNameLength = utils::StringUtils::StrLen(libraryName);

#if !RUNTIME_TINY
        {
            os::FastAutoLock lock(&s_DllCacheMutex);
            for (DllCacheIterator it = s_DllCache.begin(); it != s_DllCache.end(); it++)
                if (it->first.compare(0, std::string::npos, libraryName, libraryNameLength) == 0)
                    return it->second;
        }
#endif

        bool needsClosing = true;

        auto handle = Baselib_DynamicLibrary_Handle_Invalid;
        if (libraryName == nullptr || libraryNameLength == 0)
        {
            auto errorState = Baselib_ErrorState_Create();
            handle = OpenProgramHandle(errorState, needsClosing);
            // Disabling it for emscripten and tiny builds as they seem to be quite code sensitive
#if (!RUNTIME_TINY) && (!defined(__EMSCRIPTEN__))
            if (Baselib_ErrorState_ErrorRaised(&errorState))
            {
                if (!detailedError.empty())
                    detailedError += " ";
                detailedError += "Unable to open program handle because of '";
                detailedError += utils::Exception::FormatBaselibErrorState(errorState);
                detailedError += "'.";
            }
#endif
        }
        else
            handle = ProbeForLibrary(libraryName, libraryNameLength, detailedError);

#if !RUNTIME_TINY
        if ((handle != Baselib_DynamicLibrary_Handle_Invalid) && needsClosing)
        {
            os::FastAutoLock lock(&s_DllCacheMutex);
            s_DllCache.push_back(std::make_pair(libraryName, handle));
        }
#endif

        return handle;
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(Baselib_DynamicLibrary_Handle handle, const PInvokeArguments& pinvokeArgs, std::string& detailedError)
    {
        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
            return NULL;

        StringViewAsNullTerminatedStringOf(char, pinvokeArgs.entryPoint, entryPoint);

        // If there's 'no mangle' flag set, just return directly what GetProcAddress returns
        if (pinvokeArgs.isNoMangle)
            return GetFunctionPointer(handle, entryPoint, detailedError);

        const size_t kBufferOverhead = 10;
        Il2CppMethodPointer func = nullptr;
        size_t originalFuncNameLength = strlen(entryPoint) + 1;
        std::string functionName;

        functionName.resize(originalFuncNameLength + kBufferOverhead + 1); // Let's index the string from '1', because we might have to prepend an underscore in case of stdcall mangling
        memcpy(&functionName[1], entryPoint, originalFuncNameLength);
        memset(&functionName[1] + originalFuncNameLength, 0, kBufferOverhead);

        // If there's no 'dont mangle' flag set, 'W' function takes priority over original name, but 'A' function does not (yes, really)
        if (pinvokeArgs.charSet == CHARSET_UNICODE)
        {
            functionName[originalFuncNameLength] = 'W';
            if ((func = GetFunctionPointer(handle, functionName.c_str() + 1, detailedError)))
                return func;

            // If charset specific function lookup failed, try with original name
            if ((func = GetFunctionPointer(handle, entryPoint, detailedError)))
                return func;
        }
        else
        {
            if ((func = GetFunctionPointer(handle, entryPoint, detailedError)))
                return func;

            // If original name function lookup failed, try with mangled name
            functionName[originalFuncNameLength] = 'A';
            if ((func = GetFunctionPointer(handle, functionName.c_str() + 1, detailedError)))
                return func;
        }

        // TODO is this Win only?
        // If it's not cdecl, try mangling the name
        // THIS ONLY APPLIES TO 32-bit x86!
#if defined(_X86_) && PLATFORM_ARCH_32
        if (sizeof(void*) == 4 && pinvokeArgs.callingConvention != IL2CPP_CALL_C)
        {
            functionName[0] = '_';
            sprintf(&functionName[0] + originalFuncNameLength, "@%i", pinvokeArgs.parameterSize);
            if ((func = GetFunctionPointer(handle, functionName.c_str(), detailedError)))
                return func;
        }
#endif

        return NULL;
    }

    Il2CppMethodPointer LibraryLoader::GetFunctionPointer(Baselib_DynamicLibrary_Handle handle, const char* functionName, std::string& detailedError)
    {
        auto errorState = Baselib_ErrorState_Create();
        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
            return NULL;
        auto func = reinterpret_cast<Il2CppMethodPointer>(Baselib_DynamicLibrary_GetFunction(handle, functionName, &errorState));
#if (!RUNTIME_TINY) && (!defined(__EMSCRIPTEN__))
        if (Baselib_ErrorState_ErrorRaised(&errorState))
        {
            if (!detailedError.empty())
                detailedError += " ";
            detailedError += "Unable to get function '";
            detailedError += functionName;
            detailedError += "' because of '";
            detailedError += utils::Exception::FormatBaselibErrorState(errorState);
            detailedError += "'.";
        }
#else
        NO_UNUSED_WARNING(detailedError);
#endif
        return func;
    }

    void LibraryLoader::CleanupLoadedLibraries()
    {
#if !RUNTIME_TINY
        // We assume that presence of the library in s_DllCache is a valid reason to be able to close it
        for (DllCacheIterator it = s_DllCache.begin(); it != s_DllCache.end(); it++)
        {
            // If libc is a "loaded library", it is a special case, and closing it will cause dlclose
            // on some Posix platforms to return an error (I'm looking at you, iOS 11). This really is
            // not an error, but Baselib_DynamicLibrary_Close will correctly assert when dlclose
            // returns an error. To avoid this assert, let's skip closing libc.
            if (utils::StringUtils::NativeStringToUtf8(it->first.c_str()) != "libc")
                Baselib_DynamicLibrary_Close(it->second);
        }
        s_DllCache.clear();
#endif
    }

    bool LibraryLoader::CloseLoadedLibrary(Baselib_DynamicLibrary_Handle handle)
    {
        if (handle == Baselib_DynamicLibrary_Handle_Invalid)
            return false;

#if !RUNTIME_TINY
        os::FastAutoLock lock(&s_DllCacheMutex);
        // We assume that presence of the library in s_DllCache is a valid reason to be able to close it
        for (DllCacheIterator it = s_DllCache.begin(); it != s_DllCache.end(); it++)
        {
            if (it->second == handle)
            {
                Baselib_DynamicLibrary_Close(it->second);
                s_DllCache.erase(it);
                return true;
            }
        }
#endif
        return false;
    }

    void LibraryLoader::SetFindPluginCallback(Il2CppSetFindPlugInCallback method)
    {
        s_FindPluginCallback = method;
    }

    Baselib_DynamicLibrary_Handle LibraryLoader::TryOpeningLibrary(const Il2CppNativeChar* libraryName, std::string& detailedError)
    {
        auto errorState = Baselib_ErrorState_Create();
        auto handle = Baselib_DynamicLibrary_Open(utils::StringUtils::NativeStringToBaselib(libraryName), &errorState);

#if (!RUNTIME_TINY) && (!defined(__EMSCRIPTEN__))
        if (Baselib_ErrorState_ErrorRaised(&errorState))
        {
            if (!detailedError.empty())
                detailedError += " ";
            detailedError += "Unable to load dynamic library '";
            detailedError += utils::StringUtils::NativeStringToUtf8(libraryName);
            detailedError += "' because of '";
            detailedError += utils::Exception::FormatBaselibErrorState(errorState);
            detailedError += "'.";
        }
#else
        NO_UNUSED_WARNING(detailedError);
#endif
        return handle;
    }
} /* namespace vm */
} /* namespace il2cpp */
