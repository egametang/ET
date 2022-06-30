#pragma once

#include "il2cpp-config.h"
#include "il2cpp-pinvoke-support.h"
#include "utils/StringView.h"
#include <string>

#include "Baselib.h"
#include "Cpp/Baselib_DynamicLibrary.h"

#if !RUNTIME_TINY
#include "Cpp/ReentrantLock.h"
#include <vector>
#include <string>
#endif

namespace il2cpp
{
namespace os
{
    struct HardcodedPInvokeDependencyFunction
    {
        const char* functionName;
        Il2CppMethodPointer functionPointer;
        size_t functionNameLen;
    };

    struct HardcodedPInvokeDependencyLibrary
    {
        const Il2CppNativeChar* libraryName;
        size_t functionCount;
        const HardcodedPInvokeDependencyFunction* functions;
    };

#define HARDCODED_DEPENDENCY_LIBRARY(libraryName, libraryFunctions) { libraryName, sizeof(libraryFunctions) / sizeof(HardcodedPInvokeDependencyFunction), libraryFunctions }
#define HARDCODED_DEPENDENCY_FUNCTION(function) { #function, reinterpret_cast<Il2CppMethodPointer>(function), IL2CPP_ARRAY_SIZE(#function)-1  }

    class LibraryLoader
    {
    public:
        static Il2CppMethodPointer GetHardcodedPInvokeDependencyFunctionPointer(const il2cpp::utils::StringView<Il2CppNativeChar>& nativeDynamicLibrary, const il2cpp::utils::StringView<char>& entryPoint, Il2CppCharSet charSet);
        static Baselib_DynamicLibrary_Handle LoadDynamicLibrary(const utils::StringView<Il2CppNativeChar> nativeDynamicLibrary, std::string& detailedError);
        static Il2CppMethodPointer GetFunctionPointer(Baselib_DynamicLibrary_Handle handle, const PInvokeArguments& pinvokeArgs, std::string& detailedError);
        static Il2CppMethodPointer GetFunctionPointer(Baselib_DynamicLibrary_Handle handle, const char* functionName, std::string& detailedError);
        static void CleanupLoadedLibraries();
        static bool CloseLoadedLibrary(Baselib_DynamicLibrary_Handle handle);
        static void SetFindPluginCallback(Il2CppSetFindPlugInCallback method);
        static Baselib_DynamicLibrary_Handle TryOpeningLibrary(const Il2CppNativeChar* libraryName, std::string& detailedError);
    private:
        static Baselib_DynamicLibrary_Handle ProbeForLibrary(const Il2CppNativeChar* libraryName, const size_t libraryNameLength, std::string& detailedError);
        // needsClosing defaults to true, only set it to false when needed
        static Baselib_DynamicLibrary_Handle OpenProgramHandle(Baselib_ErrorState& errorState, bool& needsClosing);

        static const HardcodedPInvokeDependencyLibrary* HardcodedPInvokeDependencies;
        static const size_t HardcodedPInvokeDependenciesCount;
        static bool EntryNameMatches(const il2cpp::utils::StringView<char>& hardcodedEntryPoint, const il2cpp::utils::StringView<char>& entryPoint);
    };
} /* namespace os */
} /* namespace il2cpp*/
