#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
#if WINDOWS_SDK_BUILD_VERSION < 16299

#include "os/Win32/WindowsHeaders.h"
#include "Win32ApiSharedEmulation.h"

#include <io.h>
#include <windows.networking.connectivity.h>
#include <windows.storage.h>

using namespace il2cpp::winrt;
using namespace Microsoft::WRL;
using namespace Microsoft::WRL::Wrappers;
using namespace ABI::Windows::Foundation;
using namespace ABI::Windows::Foundation::Collections;
using namespace ABI::Windows::Networking;
using namespace ABI::Windows::Networking::Connectivity;
using namespace ABI::Windows::Storage;

extern "C"
{
    BOOL WINAPI GetComputerNameW(LPWSTR lpBuffer, LPDWORD nSize)
    {
#define ERROR_CHECK(hr) do { if (FAILED(hr)) { SetLastError(WIN32_FROM_HRESULT(hr)); return FALSE; } } while (false)

        ComPtr<INetworkInformationStatics> info;
        auto hr = RoGetActivationFactory(HStringReference(RuntimeClass_Windows_Networking_Connectivity_NetworkInformation).Get(), __uuidof(info), &info);
        ERROR_CHECK(hr);

        ComPtr<IVectorView<HostName*> > names;
        hr = info->GetHostNames(&names);
        ERROR_CHECK(hr);

        unsigned int size;
        hr = names->get_Size(&size);
        if (FAILED(hr) || !size)
        {
            SetLastError(WIN32_FROM_HRESULT(hr));
            return FALSE;
        }

        ComPtr<IHostName> name;
        hr = names->GetAt(0, &name);
        ERROR_CHECK(hr);

        HString displayName;
        hr = name->get_DisplayName(displayName.GetAddressOf());
        ERROR_CHECK(hr);

#undef ERROR_CHECK

        unsigned int sourceLength;
        auto sourceBuffer = displayName.GetRawBuffer(&sourceLength);

        // NetBIOS caps at 15 characters, not including the null terminator
        DWORD finalSize = sourceLength > 15 ? 15 : sourceLength;

        // Cap at the first period
        for (DWORD i = 0; i < finalSize; ++i)
            if (sourceBuffer[i] == '.')
                finalSize = i;

        // Error and return the size if the buffer is not large enough
        if (finalSize + 1 > *nSize)
        {
            SetLastError(ERROR_BUFFER_OVERFLOW);
            *nSize = finalSize + 1;
            return FALSE;
        }

        if (lpBuffer != nullptr)
        {
            memset(lpBuffer, 0, *nSize);

            // Copy the characters and make them uppercase
            for (DWORD i = 0; i < finalSize; ++i)
                lpBuffer[i] = toupper(sourceBuffer[i]);

            *nSize = finalSize;

            return TRUE;
        }

        *nSize = finalSize;
        return FALSE;
    }
} // extern "C"

#endif // WINDOWS_SDK_BUILD_VERSION < 16299

#if WINDOWS_SDK_BUILD_VERSION < 15063

#include "os/Win32/WindowsHeaders.h"
#include "Win32ApiSharedEmulation.h"

extern "C"
{
    DWORD WINAPI GetNetworkParams(PFIXED_INFO pFixedInfo, PULONG pOutBufLen)
    {
        if (*pOutBufLen < sizeof(FIXED_INFO))
        {
            *pOutBufLen = sizeof(FIXED_INFO);
            return ERROR_BUFFER_OVERFLOW;
        }
        memset(pFixedInfo, 0, sizeof(FIXED_INFO));
        return ERROR_NOT_SUPPORTED;
    }
} // extern "C"

#endif // WINDOWS_SDK_BUILD_VERSION < 15063

#if IL2CPP_TARGET_WINRT

#include "Win32ApiSharedEmulation.h"

extern "C"
{
    // Provide a dummy GetIfEntry for WinRT. This is used by the class library
    // code to implement GetAllNetworkInterfaces(). It looks like the values
    // returned though are never actually used. So this dummy implementation seems
    // to be enough for the class library code to work in WinRT.
    DWORD WINAPI GetIfEntry(PMIB_IFROW pIfRow)
    {
        memset(pIfRow, 0, sizeof(MIB_IFROW));
        return NO_ERROR;
    }
} // extern "C"

#endif // IL2CPP_TARGET_WINRT

#endif // IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
