#pragma once

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE

#include "os/Win32/WindowsHeaders.h"
#include <wrl.h>

#if WINDOWS_SDK_BUILD_VERSION < 16299 // This got readded on Windows 10 Fall Creators Update
#define MAX_COMPUTERNAME_LENGTH 31
#define GetComputerName GetComputerNameW
#endif

namespace il2cpp
{
namespace winrt
{
    inline DWORD WIN32_FROM_HRESULT(HRESULT hr)
    {
        if ((hr & 0xFFFF0000) == MAKE_HRESULT(SEVERITY_ERROR, FACILITY_WIN32, 0))
            return HRESULT_CODE(hr);
        if (hr == S_OK)
            return HRESULT_CODE(hr);
        return ERROR_SUCCESS;
    }

    inline static BOOL CopyHStringToBuffer(Microsoft::WRL::Wrappers::HString& source, LPWSTR target, LPDWORD targetSize)
    {
        unsigned int sourceLength;
        auto sourceBuffer = source.GetRawBuffer(&sourceLength);

        if (sourceLength + 1 > *targetSize)
        {
            SetLastError(ERROR_BUFFER_OVERFLOW);
            *targetSize = sourceLength + 1;
            return FALSE;
        }

        *targetSize = sourceLength;

        if (target != nullptr)
        {
            memcpy(target, sourceBuffer, sourceLength * sizeof(wchar_t));
            target[sourceLength] = L'\0';
            return TRUE;
        }

        return FALSE;
    }
}
}

#if WINDOWS_SDK_BUILD_VERSION < 16299 // These APIs got readded on Windows 10 Fall Creators Update

extern "C"
{
    inline BOOL WINAPI CopyFileW(LPCWSTR lpExistingFileName, LPCWSTR lpNewFileName, BOOL bFailIfExists)
    {
        COPYFILE2_EXTENDED_PARAMETERS params;

        params.dwSize = sizeof(params);
        params.dwCopyFlags = bFailIfExists ? COPY_FILE_FAIL_IF_EXISTS : 0;
        params.pfCancel = FALSE;
        params.pProgressRoutine = nullptr;
        params.pvCallbackContext = nullptr;

        auto hr = CopyFile2(lpExistingFileName, lpNewFileName, &params);
        if (FAILED(hr))
        {
            SetLastError(il2cpp::winrt::WIN32_FROM_HRESULT(hr));
            return FALSE;
        }

        return TRUE;
    }

    inline UINT WINAPI GetACP()
    {
        return CP_ACP;
    }

    BOOL WINAPI GetComputerNameW(LPWSTR lpBuffer, LPDWORD nSize);
} // extern "C"

#endif

#if WINDOWS_SDK_BUILD_VERSION < 15063

extern "C"
{
    typedef struct
    {
        char String[4 * 4];
    } IP_ADDRESS_STRING, *PIP_ADDRESS_STRING, IP_MASK_STRING, *PIP_MASK_STRING;

    typedef struct _IP_ADDR_STRING
    {
        struct _IP_ADDR_STRING* Next;
        IP_ADDRESS_STRING IpAddress;
        IP_MASK_STRING IpMask;
        DWORD Context;
    } IP_ADDR_STRING, *PIP_ADDR_STRING;

#define MAX_HOSTNAME_LEN                128
#define MAX_DOMAIN_NAME_LEN             128
#define MAX_SCOPE_ID_LEN                256

    typedef struct
    {
        char            HostName[MAX_HOSTNAME_LEN + 4];
        char            DomainName[MAX_DOMAIN_NAME_LEN + 4];
        PIP_ADDR_STRING CurrentDnsServer;
        IP_ADDR_STRING  DnsServerList;
        UINT            NodeType;
        char            ScopeId[MAX_SCOPE_ID_LEN + 4];
        UINT            EnableRouting;
        UINT            EnableProxy;
        UINT            EnableDns;
    } FIXED_INFO, *PFIXED_INFO;

    DWORD WINAPI GetNetworkParams(PFIXED_INFO pFixedInfo, PULONG pOutBufLen);
} // extern "C"

#endif

#if IL2CPP_TARGET_WINRT

extern "C"
{
#if WINDOWS_SDK_BUILD_VERSION < 15063

    #define MAX_INTERFACE_NAME_LEN 256
    #define MAXLEN_PHYSADDR 8

    typedef enum
    {
        IF_OPER_STATUS_NON_OPERATIONAL = 0,
        IF_OPER_STATUS_UNREACHABLE     = 1,
        IF_OPER_STATUS_DISCONNECTED    = 2,
        IF_OPER_STATUS_CONNECTING      = 3,
        IF_OPER_STATUS_CONNECTED       = 4,
        IF_OPER_STATUS_OPERATIONAL     = 5,
    } INTERNAL_IF_OPER_STATUS;

    #define MAXLEN_IFDESCR 256

    typedef struct
    {
        WCHAR wszName[MAX_INTERFACE_NAME_LEN];
        IF_INDEX dwIndex;
        IFTYPE dwType;
        DWORD dwMtu;
        DWORD dwSpeed;
        DWORD dwPhysAddrLen;
        UCHAR bPhysAddr[MAXLEN_PHYSADDR];
        DWORD dwAdminStatus;
        INTERNAL_IF_OPER_STATUS dwOperStatus;
        DWORD dwLastChange;
        DWORD dwInOctets;
        DWORD dwInUcastPkts;
        DWORD dwInNUcastPkts;
        DWORD dwInDiscards;
        DWORD dwInErrors;
        DWORD dwInUnknownProtos;
        DWORD dwOutOctets;
        DWORD dwOutUcastPkts;
        DWORD dwOutNUcastPkts;
        DWORD dwOutDiscards;
        DWORD dwOutErrors;
        DWORD dwOutQLen;
        DWORD dwDescrLen;
        UCHAR bDescr[MAXLEN_IFDESCR];
    } MIB_IFROW, *PMIB_IFROW;

#endif

    DWORD WINAPI GetIfEntry(PMIB_IFROW pIfRow);
} // extern "C"

#endif // IL2CPP_TARGET_WINRT

#endif
