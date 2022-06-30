#include "il2cpp-config.h"

#include "StackTrace.h"

namespace il2cpp
{
namespace os
{
    static Il2CppBacktraceFunc s_StackBacktraceFunc = 0;

    void StackTrace::WalkStack(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        if (s_StackBacktraceFunc == 0)
        {
            StackTrace::WalkStackNative(callback, context, walkOrder);
            return;
        }
        const int kMaxStackFrames = 128;
        Il2CppMethodPointer addrs[kMaxStackFrames];
        size_t size = s_StackBacktraceFunc(addrs, kMaxStackFrames);
        for (size_t i = 0; i < size; ++i)
        {
            const size_t index = (walkOrder == os::StackTrace::kFirstCalledToLastCalled) ? (size - i - 1) : i;
            if (!callback(addrs[index], context))
                break;
        }
    }

    void StackTrace::OverrideStackBacktrace(Il2CppBacktraceFunc stackBacktraceFunc)
    {
        s_StackBacktraceFunc = stackBacktraceFunc;
    }
}
}
