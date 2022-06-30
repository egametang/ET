#pragma once

#if IL2CPP_TARGET_WINRT

extern "C"
{
#if WINDOWS_SDK_BUILD_VERSION < 16299 // These APIs got readded on Windows 10 Fall Creators Update

#define CreateEvent CreateEventW
#define FreeEnvironmentStrings FreeEnvironmentStringsW
#define GetEnvironmentStrings GetEnvironmentStringsW
#define GetEnvironmentVariable GetEnvironmentVariableW
#define GetVersionEx GetVersionExW
#define SetEnvironmentVariable SetEnvironmentVariableW

#endif

#define GetUserName GetUserNameW

#if WINDOWS_SDK_BUILD_VERSION < 16299

    inline HANDLE WINAPI CreateEventW(LPSECURITY_ATTRIBUTES lpEventAttributes, BOOL bManualReset, BOOL bInitialState, LPCWSTR lpName)
    {
        DWORD flags = 0;
        if (bManualReset)
            flags |= CREATE_EVENT_MANUAL_RESET;
        if (bInitialState)
            flags |= CREATE_EVENT_INITIAL_SET;
        return CreateEventExW(lpEventAttributes, lpName, flags, EVENT_ALL_ACCESS);
    }

#endif

    inline HANDLE WINAPI CreateFileW(LPCWSTR lpFileName, DWORD dwDesiredAccess, DWORD dwShareMode, LPSECURITY_ATTRIBUTES lpSecurityAttributes, DWORD dwCreationDisposition, DWORD dwFlagsAndAttributes, HANDLE hTemplateFile)
    {
        const DWORD kFileAttributeMask = 0x0000FFFF;
        const DWORD kFileFlagMask = 0xFFFF0000;

        CREATEFILE2_EXTENDED_PARAMETERS extendedParameters;
        extendedParameters.dwSize = sizeof(CREATEFILE2_EXTENDED_PARAMETERS);
        extendedParameters.dwFileAttributes = dwFlagsAndAttributes & kFileAttributeMask;
        extendedParameters.dwFileFlags = dwFlagsAndAttributes & kFileFlagMask;
        extendedParameters.dwSecurityQosFlags = SECURITY_ANONYMOUS;
        extendedParameters.lpSecurityAttributes = lpSecurityAttributes;
        extendedParameters.hTemplateFile = hTemplateFile;

        return CreateFile2(lpFileName, dwDesiredAccess, dwShareMode, dwCreationDisposition, &extendedParameters);
    }

#if WINDOWS_SDK_BUILD_VERSION < 16299

    BOOL WINAPI FreeEnvironmentStringsW(LPWCH strings);

    LPWCH WINAPI GetEnvironmentStringsW();

    DWORD WINAPI GetEnvironmentVariableW(LPCWSTR lpName, LPWSTR lpBuffer, DWORD nSize);

    BOOL WINAPI GetVersionExW(LPOSVERSIONINFOW lpVersionInformation);

#endif

    BOOL WINAPI GetUserNameW(LPWSTR lpBuffer, LPDWORD pcbBuffer);

    inline HMODULE WINAPI LoadLibraryW(LPCWSTR lpLibFileName)
    {
        return LoadPackagedLibrary(lpLibFileName, 0);
    }

#if WINDOWS_SDK_BUILD_VERSION < 16299

    BOOL WINAPI SetEnvironmentVariableW(LPCWSTR lpName, LPCWSTR lpValue);

#endif

#define CreateFileMappingW(hFile, lpFileMappingAttributes, flProtect, dwMaximumSizeHigh, dwMaximumSizeLow, lpName) \
    CreateFileMappingFromApp(hFile, lpFileMappingAttributes, flProtect, (static_cast<ULONG64>(dwMaximumSizeHigh) << 32) | dwMaximumSizeLow, lpName);

#define MapViewOfFile(hFileMappingObject, dwDesiredAccess, dwFileOffsetHigh, dwFileOffsetLow, dwNumberOfBytesToMap) \
    MapViewOfFileFromApp(hFileMappingObject, dwDesiredAccess, (static_cast<ULONG64>(dwFileOffsetHigh) << 32) | dwFileOffsetLow, dwNumberOfBytesToMap);

#if WINDOWS_SDK_BUILD_VERSION < 14393
#define TlsAlloc() FlsAlloc(NULL)
#define TlsGetValue FlsGetValue
#define TlsSetValue FlsSetValue
#define TlsFree FlsFree
#endif
} // extern "C"

#endif
