#include "il2cpp-config.h"

#if IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_JAVASCRIPT && !IL2CPP_TARGET_ANDROID && !IL2CPP_TARGET_PS4  && !IL2CPP_TARGET_LUMIN

#include "os/StackTrace.h"
#include <execinfo.h>
#include <cxxabi.h>
#include <cstdlib>

namespace il2cpp
{
namespace os
{
    const int kMaxStackFrames = 128;

    void StackTrace::WalkStackNative(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        void* callstack[kMaxStackFrames];
        int frames = backtrace(callstack, kMaxStackFrames);

        if (walkOrder == kFirstCalledToLastCalled)
        {
            for (int i = frames - 1; i >= 0; i--)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(callstack[i]), context))
                    break;
            }
        }
        else
        {
            for (int i = 0; i < frames; i++)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(callstack[i]), context))
                    break;
            }
        }
    }

    std::string StackTrace::NativeStackTrace()
    {
        void* callstack[kMaxStackFrames];
        int frames = backtrace(callstack, kMaxStackFrames);
        char **symbols = backtrace_symbols(callstack, frames);

        std::string stackTrace;
        if (symbols != NULL)
        {
            for (int i = 0; i < frames; ++i)
            {
                stackTrace += symbols[i];
                stackTrace += "\n";
            }

            free(symbols);
        }

        return stackTrace;
    }

    const void* StackTrace::GetStackPointer()
    {
        // TODO implement to avoid extra WalkStack calls
        return nullptr;
    }
}
}

#endif
