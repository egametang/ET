#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT

#include "os/Win32/WindowsHeaders.h"

#include <string>
#include <windows.system.profile.h>
#include <windows.system.userprofile.h>

#include "os/Mutex.h"
#include "SynchronousOperation.h"
#include "utils/Il2CppHashMap.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "Win32ApiSharedEmulation.h"
#include "Win32ApiWinRTEmulation.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

using namespace ABI::Windows::Foundation;
using namespace ABI::Windows::System::Profile;
using namespace ABI::Windows::System::UserProfile;
using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace il2cpp::winrt;

#if WINDOWS_SDK_BUILD_VERSION < 16299

struct WideStringHash
{
public:
    size_t operator()(const std::wstring& str) const
    {
        return Hash(str);
    }

    static size_t Hash(const std::wstring& str)
    {
        return il2cpp::utils::StringUtils::Hash((const char*)str.c_str(), sizeof(std::wstring::value_type) * str.length());
    }
};

typedef Il2CppHashMap<std::wstring, std::wstring, WideStringHash> EnvironmentVariableMap;
static EnvironmentVariableMap s_EnvironmentVariables;
static baselib::ReentrantLock s_EnvironmentVariablesMutex;

#endif

extern "C"
{
#if WINDOWS_SDK_BUILD_VERSION < 16299

    BOOL WINAPI FreeEnvironmentStringsW(LPWCH strings)
    {
        IL2CPP_FREE(strings);
        return TRUE;
    }

    LPWCH WINAPI GetEnvironmentStringsW()
    {
        il2cpp::os::FastAutoLock lock(&s_EnvironmentVariablesMutex);

        // Two iterations
        // 1) Figure out length
        // 2) Make result string
        size_t length = 0;

        for (EnvironmentVariableMap::const_iterator it = s_EnvironmentVariables.begin(); it != s_EnvironmentVariables.end(); it++)
        {
            // Key + value + '=' + '\0'
            length += it->first.key.length() + it->second.length() + 2;
        }

        // Terminating '\0'
        length++;

        LPWCH result = static_cast<LPWCH>(IL2CPP_MALLOC(length * sizeof(WCHAR)));
        size_t index = 0;

        for (EnvironmentVariableMap::const_iterator it = s_EnvironmentVariables.begin(); it != s_EnvironmentVariables.end(); it++)
        {
            const size_t keyLength = it->first.key.length();
            const size_t valueLength = it->second.length();

            memcpy(result + index, it->first.key.c_str(), keyLength * sizeof(WCHAR));
            index += keyLength;

            result[index++] = L'=';

            memcpy(result + index, it->second.c_str(), valueLength * sizeof(WCHAR));
            index += valueLength;

            result[index++] = '\0';
        }

        result[index++] = '\0';
        IL2CPP_ASSERT(index == length);
        return result;
    }

    DWORD WINAPI GetEnvironmentVariableW(LPCWSTR lpName, LPWSTR lpBuffer, DWORD nSize)
    {
        il2cpp::os::FastAutoLock lock(&s_EnvironmentVariablesMutex);

        std::wstring key(lpName);
        auto it = s_EnvironmentVariables.find(key);

        if (it == s_EnvironmentVariables.end())
            return 0;

        DWORD sizeNeeded = static_cast<DWORD>(it->second.length());

        if (nSize < sizeNeeded)
        {
            SetLastError(ERROR_BUFFER_OVERFLOW);
        }
        else
        {
            memcpy(lpBuffer, it->second.data(), (sizeNeeded + 1) * sizeof(wchar_t));
        }

        return sizeNeeded;
    }

    BOOL WINAPI GetVersionExW(LPOSVERSIONINFOW lpVersionInformation)
    {
        Assert(lpVersionInformation->dwOSVersionInfoSize == sizeof(OSVERSIONINFOW));

#define ERROR_CHECK(hr) do { if (FAILED(hr)) { SetLastError(WIN32_FROM_HRESULT(hr)); return FALSE; } } while (false)

        ComPtr<IAnalyticsInfoStatics> analytics;
        auto hr = RoGetActivationFactory(HStringReference(RuntimeClass_Windows_System_Profile_AnalyticsInfo).Get(), __uuidof(IAnalyticsInfoStatics), &analytics);
        ERROR_CHECK(hr);

        ComPtr<IAnalyticsVersionInfo> versionInfo;
        hr = analytics->get_VersionInfo(&versionInfo);
        ERROR_CHECK(hr);

        HString versionString;
        hr = versionInfo->get_DeviceFamilyVersion(versionString.GetAddressOf());
        ERROR_CHECK(hr);

#undef ERROR_CHECK

        unsigned int dummy;
        int64_t versionNumber = _wtoi64(versionString.GetRawBuffer(&dummy));

        if (versionNumber == 0)
        {
            SetLastError(ERROR_VERSION_PARSE_ERROR);
            return FALSE;
        }

        lpVersionInformation->dwMajorVersion = versionNumber >> 48;
        lpVersionInformation->dwMinorVersion = (versionNumber >> 32) & 0xFFFF;
        lpVersionInformation->dwBuildNumber = (versionNumber >> 16) & 0xFFFF;
        lpVersionInformation->dwPlatformId = VER_PLATFORM_WIN32_NT;
        ZeroMemory(lpVersionInformation->szCSDVersion, sizeof(lpVersionInformation->szCSDVersion));

        return TRUE;
    }

    BOOL WINAPI SetEnvironmentVariableW(LPCWSTR lpName, LPCWSTR lpValue)
    {
        il2cpp::os::FastAutoLock lock(&s_EnvironmentVariablesMutex);

        if (lpValue != NULL)
        {
            s_EnvironmentVariables[std::wstring(lpName)] = lpValue;
        }
        else
        {
            s_EnvironmentVariables.erase(std::wstring(lpName));
        }

        return TRUE;
    }

#endif

    BOOL WINAPI GetUserNameW(LPWSTR lpBuffer, LPDWORD pcbBuffer)
    {
#define ERROR_CHECK(hr) do { if (FAILED(hr)) { SetLastError(WIN32_FROM_HRESULT(hr)); return FALSE; } } while (false)

        ComPtr<IUserInformationStatics> info;
        auto hr = RoGetActivationFactory(HStringReference(RuntimeClass_Windows_System_UserProfile_UserInformation).Get(), __uuidof(info), &info);
        ERROR_CHECK(hr);

        boolean isAccessAllowed;
        hr = info->get_NameAccessAllowed(&isAccessAllowed);
        ERROR_CHECK(hr);

        if (!isAccessAllowed)
        {
            SetLastError(ERROR_ACCESS_DENIED);
            return FALSE;
        }

        ComPtr<IAsyncOperation<HSTRING> > op;
        hr = info->GetDisplayNameAsync(&op);
        ERROR_CHECK(hr);

        HString name;
        hr = MakeSynchronousOperation(op.Get())->GetResults(name.GetAddressOf());
        ERROR_CHECK(hr);

#undef ERROR_CHECK

        return CopyHStringToBuffer(name, lpBuffer, pcbBuffer);
    }
} // extern "C"

#endif
