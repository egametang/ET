#pragma once

#include "il2cpp-config.h"

#if IL2CPP_THREADS_WIN32

#include "os/c-api/il2cpp-config-platforms.h"
#include "os/ErrorCodes.h"
#include "utils/NonCopyable.h"

// We declare windows APIs here instead of including windows system headers
// to avoid leaking windows system headers to all of il2cpp and il2cpp-generated
// code.
// On UWP, old Windows SDKs (10586 and older) did not have support for Tls* functions
// so instead we used forwarded them to Fls* equivalents. Using Tls* functions directly
// with these old SDKs causes linker errors.

#define IL2CPP_USE_WINRT_FLS_WORKAROUND (IL2CPP_TARGET_WINRT && WINDOWS_SDK_BUILD_VERSION <= 10586)

#if !IL2CPP_USE_WINRT_FLS_WORKAROUND
extern "C" {
    __declspec(dllimport) unsigned long __stdcall TlsAlloc(void);
    __declspec(dllimport) void* __stdcall TlsGetValue(unsigned long dwTlsIndex);
    __declspec(dllimport) int __stdcall TlsSetValue(unsigned long dwTlsIndex, void* lpTlsValue);
    __declspec(dllimport) int __stdcall TlsFree(unsigned long dwTlsIndex);
    __declspec(dllimport) unsigned long __stdcall GetLastError(void);
}
#else
extern "C" {
    typedef void (__stdcall* PFLS_CALLBACK_FUNCTION)(void* lpFlsData);
    __declspec(dllimport) unsigned long __stdcall FlsAlloc(PFLS_CALLBACK_FUNCTION callback);
    __declspec(dllimport) void* __stdcall FlsGetValue(unsigned long dwTlsIndex);
    __declspec(dllimport) int __stdcall FlsSetValue(unsigned long dwTlsIndex, void* lpTlsValue);
    __declspec(dllimport) int __stdcall FlsFree(unsigned long dwTlsIndex);
    __declspec(dllimport) unsigned long __stdcall GetLastError(void);
}

#define TlsAlloc() FlsAlloc(NULL)
#define TlsGetValue FlsGetValue
#define TlsSetValue FlsSetValue
#define TlsFree FlsFree
#endif

#define IL2CPP_TLS_OUT_OF_INDEXES ((unsigned long)0xFFFFFFFF)

namespace il2cpp
{
namespace os
{
    class ThreadLocalValueImpl : public il2cpp::utils::NonCopyable
    {
    public:
        inline ThreadLocalValueImpl()
        {
            m_Index = TlsAlloc();
            IL2CPP_ASSERT(m_Index != IL2CPP_TLS_OUT_OF_INDEXES);
        }

        inline ~ThreadLocalValueImpl()
        {
            bool success = TlsFree(m_Index);
            NO_UNUSED_WARNING(success);
            IL2CPP_ASSERT(success);
        }

        inline ErrorCode SetValue(void* value)
        {
            if (TlsSetValue(m_Index, value) == false)
                return static_cast<ErrorCode>(GetLastError());

            return kErrorCodeSuccess;
        }

        inline ErrorCode GetValue(void** value)
        {
            *value = TlsGetValue(m_Index);
            if (*value)
                return kErrorCodeSuccess;

            unsigned long lastError = GetLastError();
            if (lastError == 0)
                return kErrorCodeSuccess;

            return static_cast<ErrorCode>(lastError);
        }

    private:
        unsigned long m_Index;
    };
}
}

#if IL2CPP_USE_WINRT_FLS_WORKAROUND
#undef TlsAlloc
#undef TlsGetValue
#undef TlsSetValue
#undef TlsFree
#endif

#endif // IL2CPP_THREADS_WIN32
