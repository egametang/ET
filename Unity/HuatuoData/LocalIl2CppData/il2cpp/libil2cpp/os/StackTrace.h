#pragma once
#include "il2cpp-config.h"

#include <string>
#include <vector>

namespace il2cpp
{
namespace os
{
    typedef bool(*WalkStackCallback)(Il2CppMethodPointer frame, void* context);

    class StackTrace
    {
    public:
        enum WalkOrder
        {
            kFirstCalledToLastCalled,
            kLastCalledToFirstCalled
        };

        // Walks the stack calling callback for each frame in the stack
        // Stops when callback returns false
        static void WalkStack(WalkStackCallback callback, void* context, WalkOrder walkOrder);

#if IL2CPP_ENABLE_NATIVE_STACKTRACES
        static std::string NativeStackTrace();
#endif

        // Returns SP value or nullptr if not implemented
        static const void* GetStackPointer();

        static void OverrideStackBacktrace(Il2CppBacktraceFunc stackBacktraceFunc);
    private:
        static void WalkStackNative(WalkStackCallback callback, void* context, WalkOrder walkOrder);
    };
}
}
