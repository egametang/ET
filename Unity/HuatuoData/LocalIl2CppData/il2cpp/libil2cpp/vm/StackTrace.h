#pragma once

#include <stdint.h>
#include <vector>
#include "il2cpp-config.h"
#include "il2cpp-metadata.h"

#if IL2CPP_TINY_DEBUGGER
#include <string>
#endif

namespace il2cpp
{
namespace vm
{
    typedef std::vector<Il2CppStackFrameInfo> StackFrames;

    class LIBIL2CPP_CODEGEN_API StackTrace
    {
    public:
        static void InitializeStackTracesForCurrentThread();
        static void CleanupStackTracesForCurrentThread();

#if IL2CPP_TINY_DEBUGGER
        static const char* GetStackTrace();
#endif

        // Current thread functions
        static const StackFrames* GetStackFrames();
        static const StackFrames* GetCachedStackFrames(int32_t depth);
        static bool GetStackFrameAt(int32_t depth, Il2CppStackFrameInfo& frame);
        static void WalkFrameStack(Il2CppFrameWalkFunc callback, void* context);

        inline static size_t GetStackDepth() { return GetStackFrames()->size(); }
        inline static bool GetTopStackFrame(Il2CppStackFrameInfo& frame) { return GetStackFrameAt(0, frame); }

        static void PushFrame(Il2CppStackFrameInfo& frame);
        static void PopFrame();

        static const void* GetStackPointer();

        // Remote thread functions
        static bool GetThreadStackFrameAt(Il2CppThread* thread, int32_t depth, Il2CppStackFrameInfo& frame);
        static void WalkThreadFrameStack(Il2CppThread* thread, Il2CppFrameWalkFunc callback, void* context);
        static int32_t GetThreadStackDepth(Il2CppThread* thread);
        static bool GetThreadTopStackFrame(Il2CppThread* thread, Il2CppStackFrameInfo& frame);
    };
}
}
