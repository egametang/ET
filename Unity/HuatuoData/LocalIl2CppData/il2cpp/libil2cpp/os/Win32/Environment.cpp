#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_ENVIRONMENT && IL2CPP_TARGET_WINDOWS
#include "WindowsHelpers.h"
#if !IL2CPP_TARGET_XBOXONE && !IL2CPP_TARGET_WINDOWS_GAMES
#include <Shlobj.h>
#endif
// Windows.h defines GetEnvironmentVariable as GetEnvironmentVariableW for unicode and this will
// change the string "Environment::GetEnvironmentVariable" below to "Environment::GetEnvironmentVariableW"
// in the preprocessor. So we undef to avoid this issue and use GetEnvironmentVariableW directly.
// Same for SetEnvironmentVariable
#undef GetEnvironmentVariable
#undef SetEnvironmentVariable
#include "os/Environment.h"
#include "utils/StringUtils.h"
#include <string>

#define BUFFER_SIZE 1024

namespace il2cpp
{
namespace os
{
    std::string Environment::GetMachineName()
    {
        Il2CppChar computerName[MAX_COMPUTERNAME_LENGTH + 1];
        DWORD size = sizeof(computerName) / sizeof(computerName[0]);
        if (!GetComputerNameW(computerName, &size))
            return NULL;

        return utils::StringUtils::Utf16ToUtf8(computerName);
    }

    int32_t Environment::GetProcessorCount()
    {
        SYSTEM_INFO info;
        GetSystemInfo(&info);
        return info.dwNumberOfProcessors;
    }

// GetVersionEx is deprecated on desktop in Windows SDK, and we shim it for WinRT
#pragma warning( push )
#pragma warning( disable : 4996 )

    std::string Environment::GetOsVersionString()
    {
        OSVERSIONINFO verinfo;

        verinfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);
        if (GetVersionEx(&verinfo))
        {
            char version[64];
            /* maximum string length is 35 bytes
               3 x 10 bytes per number, 1 byte for 0, 3 x 1 byte for dots, 1 for NULL */
            sprintf(version, "%ld.%ld.%ld.0",
                verinfo.dwMajorVersion,
                verinfo.dwMinorVersion,
                verinfo.dwBuildNumber);
            return version;
        }

        return "0.0.0.0";
    }

#pragma warning( pop )

    std::string Environment::GetOsUserName()
    {
#if !IL2CPP_TARGET_WINDOWS_GAMES
        Il2CppChar user_name[256 + 1];
        DWORD user_name_size = ARRAYSIZE(user_name);
        if (GetUserNameW(user_name, &user_name_size))
            return utils::StringUtils::Utf16ToUtf8(user_name);
#endif // !IL2CPP_TARGET_WINDOWS_GAMES

        return "Unknown";
    }

    std::string Environment::GetEnvironmentVariable(const std::string& name)
    {
        std::vector<Il2CppChar> buffer(BUFFER_SIZE);

        const UTF16String varName = utils::StringUtils::Utf8ToUtf16(name.c_str());

        DWORD ret = GetEnvironmentVariableW(varName.c_str(), &buffer[0], BUFFER_SIZE);

        if (ret == 0) // Not found
            return std::string();

        if (ret < BUFFER_SIZE) // Found and fits into buffer
            return utils::StringUtils::Utf16ToUtf8(&buffer[0]);

        // Requires bigger buffer
        IL2CPP_ASSERT(ret >= BUFFER_SIZE);

        buffer.resize(ret + 1);

        ret = GetEnvironmentVariableW(varName.c_str(), &buffer[0], ret + 1);
        IL2CPP_ASSERT(ret != 0);

        return utils::StringUtils::Utf16ToUtf8(&buffer[0]);
    }

    void Environment::SetEnvironmentVariable(const std::string& name, const std::string& value)
    {
        const UTF16String varName = utils::StringUtils::Utf8ToUtf16(name.c_str());

        if (value.empty())
            SetEnvironmentVariableW((LPWSTR)varName.c_str(), NULL);
        else
        {
            const UTF16String varValue = utils::StringUtils::Utf8ToUtf16(value.c_str());
            SetEnvironmentVariableW((LPWSTR)varName.c_str(), (LPWSTR)varValue.c_str());
        }
    }

    std::vector<std::string> Environment::GetEnvironmentVariableNames()
    {
        WCHAR* env_strings;
        WCHAR* env_string;
        WCHAR* equal_str;

        std::vector<std::string> result;

        env_strings = GetEnvironmentStringsW();

        if (env_strings)
        {
            env_string = env_strings;
            while (*env_string != '\0')
            {
                // Skip over environment variables starting with '='
                if (*env_string != '=')
                {
                    equal_str = wcschr(env_string, '=');
                    result.push_back(utils::StringUtils::Utf16ToUtf8(env_string, (int)(equal_str - env_string)));
                }
                while (*env_string != '\0')
                    env_string++;
                env_string++;
            }

            FreeEnvironmentStringsW(env_strings);
        }

        return result;
    }

    std::string Environment::GetHomeDirectory()
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        std::string home_directory;

        PWSTR profile_path = NULL;
        HRESULT hr = SHGetKnownFolderPath(FOLDERID_Profile, KF_FLAG_DEFAULT, NULL, &profile_path);
        if (SUCCEEDED(hr))
        {
            home_directory = utils::StringUtils::Utf16ToUtf8(profile_path);
            CoTaskMemFree(profile_path);
        }

        if (home_directory.empty())
        {
            home_directory = GetEnvironmentVariable("USERPROFILE");
        }

        if (home_directory.empty())
        {
            std::string drive = GetEnvironmentVariable("HOMEDRIVE");
            std::string path = GetEnvironmentVariable("HOMEPATH");

            if (!drive.empty() && !path.empty())
                home_directory = drive + path;
        }

        return home_directory;
#else
        IL2CPP_NOT_IMPLEMENTED_ICALL(Environment::GetHomeDirectory);
        return std::string();
#endif
    }

    std::vector<std::string> SplitLogicalDriveString(Il2CppChar *buffer, DWORD size)
    {
        std::vector<std::string> retVal;
        Il2CppChar *ptr = buffer;

        for (DWORD i = 0; i < size; ++i)
        {
            Il2CppChar c = buffer[i];
            if (c == 0)
            {
                retVal.push_back(utils::StringUtils::Utf16ToUtf8(ptr));
                ptr = &buffer[i + 1];
            }
        }

        return retVal;
    }

    std::vector<std::string> Environment::GetLogicalDrives()
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        std::vector<Il2CppChar> buffer(BUFFER_SIZE);

        DWORD size = GetLogicalDriveStringsW(BUFFER_SIZE, &buffer[0]);

        if (size == 0)
            return std::vector<std::string>();

        if (size > BUFFER_SIZE)
        {
            buffer.resize(size + 1);
            size = GetLogicalDriveStringsW(size + 1, &buffer[0]);
            IL2CPP_ASSERT(size != 0);
        }

        return SplitLogicalDriveString(&buffer[0], size);
#else
        return std::vector<std::string>();
#endif
    }

    void Environment::Exit(int result)
    {
        ::exit(result);
    }

    NORETURN void Environment::Abort()
    {
        // __fastfail() is available since VS2012
#if _MSC_VER >= 1700
        __fastfail(FAST_FAIL_FATAL_APP_EXIT);
#else
        abort();
#endif
    }

#if IL2CPP_TARGET_WINDOWS_DESKTOP

    utils::Expected<std::string> Environment::GetWindowsFolderPath(int32_t folder)
    {
        Il2CppChar path[MAX_PATH];
        if (SUCCEEDED(SHGetFolderPathW(NULL, folder | CSIDL_FLAG_CREATE, NULL, 0, path)))
            return utils::StringUtils::Utf16ToUtf8(path);

        return std::string();
    }

    typedef BOOL(WINAPI *LPFN_ISWOW64PROCESS) (HANDLE, PBOOL);

    utils::Expected<bool> Environment::Is64BitOs()
    {
        BOOL isWow64Process = false;

        // Supported on XP SP2 and higher

        //IsWow64Process is not available on all supported versions of Windows.
        //Use GetModuleHandle to get a handle to the DLL that contains the function
        //and GetProcAddress to get a pointer to the function if available.

        LPFN_ISWOW64PROCESS fnIsWow64Process = (LPFN_ISWOW64PROCESS)GetProcAddress(
            GetModuleHandle(TEXT("kernel32")), "IsWow64Process");

        if (NULL != fnIsWow64Process)
        {
            if (fnIsWow64Process(GetCurrentProcess(), &isWow64Process))
            {
                return isWow64Process == TRUE;
            }
        }

        return false;
    }

#elif IL2CPP_TARGET_WINDOWS_GAMES
    utils::Expected<std::string> Environment::GetWindowsFolderPath(int32_t folder)
    {
        return std::string();
    }

    utils::Expected<bool> Environment::Is64BitOs()
    {
#if _WIN64 // the IsWow64Process(used above) function is not available on Windows Games,this is the best available.
        return true;
#else
        return false;
#endif
    }

#endif
}
}
#endif
