#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "il2cpp-runtime-metadata.h"
#include "os/LibraryLoader.h"
#include "os/Image.h"
#include "utils/StringUtils.h"

#include "WindowsHelpers.h"
#include "Evntprov.h"

#define WINNT // All functions in Evntrace.h are under this define.. Why? I have no idea!
#include "Evntrace.h"

#if IL2CPP_TARGET_XBOXONE
#include "os/XboxOne/Win32ApiEmulationForXboxClassLibraries.h"
#elif IL2CPP_TARGET_WINDOWS_GAMES
#include "os/WindowsGames/Win32ApiWindowsGamesEmulation.h"
#endif

namespace il2cpp
{
namespace os
{
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
        HARDCODED_DEPENDENCY_FUNCTION(GetFileAttributesExW),
        HARDCODED_DEPENDENCY_FUNCTION(CreateDirectoryW),
        HARDCODED_DEPENDENCY_FUNCTION(CloseHandle),
        HARDCODED_DEPENDENCY_FUNCTION(CreateFileW),
        HARDCODED_DEPENDENCY_FUNCTION(DeleteFileW),
        HARDCODED_DEPENDENCY_FUNCTION(FindFirstFileExW),
        HARDCODED_DEPENDENCY_FUNCTION(FindNextFileW),
        HARDCODED_DEPENDENCY_FUNCTION(MoveFileExW),
        HARDCODED_DEPENDENCY_FUNCTION(RemoveDirectoryW),
        HARDCODED_DEPENDENCY_FUNCTION(ReplaceFileW),
        HARDCODED_DEPENDENCY_FUNCTION(SetFileAttributesW),
        HARDCODED_DEPENDENCY_FUNCTION(SetFileInformationByHandle),
        HARDCODED_DEPENDENCY_FUNCTION(GetFileInformationByHandleEx),
        // The CopyFile2 method is only required by the class library code for UWP builds.
        // It does not exist in Windows 7, so we don't want to use it for Windows Desktop
        // builds, since they still support Windows 7.
#if !IL2CPP_TARGET_WINDOWS_DESKTOP
        HARDCODED_DEPENDENCY_FUNCTION(CopyFile2),
#endif
#if WINDOWS_SDK_BUILD_VERSION >= 16299
        HARDCODED_DEPENDENCY_FUNCTION(SetThreadErrorMode),
        HARDCODED_DEPENDENCY_FUNCTION(CopyFileExW),
        HARDCODED_DEPENDENCY_FUNCTION(DeleteVolumeMountPointW),
        HARDCODED_DEPENDENCY_FUNCTION(GetLogicalDrives),
#endif
    };

#if IL2CPP_TARGET_WINRT
    const HardcodedPInvokeDependencyFunction kBCryptFunctions[] =
    {
        HARDCODED_DEPENDENCY_FUNCTION(BCryptGenRandom),
    };
#endif

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
        HARDCODED_DEPENDENCY_LIBRARY(L"bcrypt", kBCryptFunctions),
#endif
    };

    const HardcodedPInvokeDependencyLibrary* LibraryLoader::HardcodedPInvokeDependencies = kHardcodedPInvokeDependencies;
    const size_t LibraryLoader::HardcodedPInvokeDependenciesCount = ARRAYSIZE(kHardcodedPInvokeDependencies);

    Baselib_DynamicLibrary_Handle LibraryLoader::ProbeForLibrary(const Il2CppNativeChar* libraryName, const size_t /*libraryNameLength*/, std::string& detailedError)
    {
        return TryOpeningLibrary(libraryName, detailedError);
    }

    Baselib_DynamicLibrary_Handle LibraryLoader::OpenProgramHandle(Baselib_ErrorState& errorState, bool& needsClosing)
    {
        needsClosing = false;
        return Baselib_DynamicLibrary_FromNativeHandle(reinterpret_cast<uint64_t>(Image::GetImageBase()), Baselib_DynamicLibrary_WinApiHMODULE, &errorState);
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
