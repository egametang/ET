#include "il2cpp-config.h"

#if IL2CPP_TARGET_LUMIN

#include "os/StackTrace.h"
#include "os/Image.h"
#include "utils/Logging.h"

#include <dlfcn.h>
#include <pthread.h>
#include <string.h>
#include <unwind.h>

#include <set>

namespace il2cpp
{
namespace os
{
    const int kMaxStackFrames = 128;

namespace
{
    const char* kLibraryName = "libil2cpp.so";
    extern "C" char end;

    typedef std::set<uintptr_t> KnownAddressSet;

    uintptr_t s_BaseAddress;
    uintptr_t s_EndAddress;
    pthread_once_t s_InitKnownSymbolInfoOnceFlag = PTHREAD_ONCE_INIT;

    static void InitKnownSymbolInfo()
    {
        s_BaseAddress = reinterpret_cast<uintptr_t>(os::Image::GetImageBase());
        s_EndAddress = reinterpret_cast<uintptr_t>(&end);
    }

    static void DumpSymbolInfo(uintptr_t addr, const Dl_info* info)
    {
        uintptr_t base = reinterpret_cast<uintptr_t>(info->dli_fbase);
        if (info->dli_sname && info->dli_saddr)
        {
            uintptr_t start = reinterpret_cast<uintptr_t>(info->dli_saddr);
            uintptr_t offset = start - base;
            utils::Logging::Write("Found symbol \"%s\" for 0x%llx (base: 0x%llx, offset: 0x%llx)", info->dli_sname, addr, base, offset);
        }
        else
        {
            uintptr_t offset = addr - base;
            utils::Logging::Write("Found symbol for 0x%llx (base: 0x%llx, offset: 0x%llx)", addr, base, offset);
        }
    }

    static bool KnownSymbol(const uintptr_t addr)
    {
        // FIXME : This is a lot of hand-waving and maddness,
        // and really needs to be supported properly at the platform
        // level.
        pthread_once(&s_InitKnownSymbolInfoOnceFlag, &InitKnownSymbolInfo);
        if (!addr)
            return false;
        //if (addr < s_BaseAddress) return false;

        /*
        if (addr >= s_BaseAddress && addr <= s_EndAddress)
            return true;
        return false;
        */

        Dl_info info;
        /*
        if (!dladdr(reinterpret_cast<void*>(addr), &info))
            return false;

        const char* const slash = strrchr(info.dli_fname, '/');
        return slash && strcmp(slash + 1, kLibraryName) == 0;
        */
        int ret = dladdr(reinterpret_cast<void*>(addr), &info);
        /*
        if (ret)
        {
            DumpSymbolInfo(addr, &info);
        }
        */
        return ret;
    }

    struct LuminStackTrace
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
            uintptr_t addr = _Unwind_GetIP(context);
            // Workaround to avoid crash when generating stack trace in some third-party libraries
            if (!KnownSymbol(addr))
                return _URC_END_OF_STACK;

            LuminStackTrace* state = static_cast<LuminStackTrace*>(self);
            if (state->PushStackFrameAddress(addr))
            {
                return _URC_NO_REASON;
            }
            else
            {
                return _URC_END_OF_STACK;
            }
        }
    };
}

    void StackTrace::WalkStackNative(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        LuminStackTrace callstack = {};
        _Unwind_Backtrace(LuminStackTrace::Callback, &callstack);
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
        // TODO implement to avoid extra WalkStack calls
        return nullptr;
    }
}
}

#endif
