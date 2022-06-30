#include "il2cpp-config.h"

#if IL2CPP_TARGET_ANDROID

#include "os/StackTrace.h"
#include "os/Image.h"

#include <unwind.h>
#include <dlfcn.h>
#include <pthread.h>
#include <string.h>

namespace il2cpp
{
namespace os
{
    const int kMaxStackFrames = 128;

namespace
{
    extern "C" char end;

    uintptr_t s_BaseAddress;
    uintptr_t s_EndAddress;
    pthread_once_t s_InitKnownSymbolInfoOnceFlag = PTHREAD_ONCE_INIT;

    static void InitKnownSymbolInfo()
    {
        s_BaseAddress = reinterpret_cast<uintptr_t>(os::Image::GetImageBase());
        s_EndAddress = reinterpret_cast<uintptr_t>(&end);
    }

    static bool KnownSymbol(const uintptr_t addr)
    {
        pthread_once(&s_InitKnownSymbolInfoOnceFlag, &InitKnownSymbolInfo);

        if (addr >= s_BaseAddress && addr <= s_EndAddress)
            return true;

        Dl_info info;
        if (!dladdr(reinterpret_cast<void*>(addr), &info))
            return false;

        // dli_name can have different values depending on Android OS:
        // Google Pixel 2 Android 10, dli_name will be "/data/app/com.unity.stopaskingforpackagename-uRHSDLXYA4cnHxyTNT30-g==/lib/arm/libunity.so"
        // Samsung GT-I9505 Android 5, dli_name will be "libunity.so"
        return info.dli_fname != NULL && strstr(info.dli_fname, "libunity.so") != NULL;
    }

    struct AndroidStackTrace
    {
        size_t size;
        Il2CppMethodPointer addrs[kMaxStackFrames];

        bool PushStackFrameAddress(const uintptr_t addr)
        {
            if (size >= kMaxStackFrames)
                return false;

            addrs[size++] = reinterpret_cast<Il2CppMethodPointer>(addr);
            return true;
        }

        static _Unwind_Reason_Code Callback(struct _Unwind_Context* context, void* self)
        {
            const uintptr_t addr = _Unwind_GetIP(context);

            // Workaround to avoid crash when generating stack trace in some third-party libraries
            if (!KnownSymbol(addr))
                return _URC_END_OF_STACK;

            if (static_cast<AndroidStackTrace*>(self)->PushStackFrameAddress(addr))
                return _URC_NO_REASON;
            else
                return _URC_END_OF_STACK;
        }
    };
}

    void StackTrace::WalkStackNative(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        AndroidStackTrace callstack = {};
        _Unwind_Backtrace(AndroidStackTrace::Callback, &callstack);
        for (size_t i = 0; i < callstack.size; ++i)
        {
            const size_t index = (walkOrder == kFirstCalledToLastCalled) ? (callstack.size - i - 1) : i;
            if (!callback(callstack.addrs[index], context))
                break;
        }
    }

    std::string StackTrace::NativeStackTrace()
    {
        return std::string();
    }

    const void* StackTrace::GetStackPointer()
    {
        return __builtin_frame_address(0);
    }
}
}

#endif
