#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHeaders.h"
#include "dbghelp.h"

#include "os/Mutex.h"
#include "os/StackTrace.h"

namespace il2cpp
{
namespace os
{
    void StackTrace::WalkStackNative(WalkStackCallback callback, void* context, WalkOrder walkOrder)
    {
        const uint32_t kMaxFrames = 128;
        void* stack[kMaxFrames];

        size_t frames = CaptureStackBackTrace(0, kMaxFrames, stack, NULL);

        if (walkOrder == kFirstCalledToLastCalled)
        {
            for (size_t i = frames; i--;)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(stack[i]), context))
                    break;
            }
        }
        else
        {
            for (size_t i = 0; i < frames; i++)
            {
                if (!callback(reinterpret_cast<Il2CppMethodPointer>(stack[i]), context))
                    break;
            }
        }
    }

    std::string StackTrace::NativeStackTrace()
    {
        std::string stackTrace;

#if !IL2CPP_TARGET_WINRT && !IL2CPP_TARGET_XBOXONE && !IL2CPP_TARGET_WINDOWS_GAMES
        HANDLE hProcess = GetCurrentProcess();
        BOOL result = SymInitialize(hProcess, NULL, TRUE);
        if (!result)
            return std::string();

        const uint32_t kMaxFrames = 128;
        void* stack[kMaxFrames];

        size_t frames = CaptureStackBackTrace(0, kMaxFrames, stack, NULL);

        for (size_t i = 0; i < frames; i++)
        {
            DWORD64  dwDisplacement = 0;

            char buffer[sizeof(SYMBOL_INFO) + MAX_SYM_NAME * sizeof(TCHAR)];
            PSYMBOL_INFO pSymbol = (PSYMBOL_INFO)buffer;

            pSymbol->SizeOfStruct = sizeof(SYMBOL_INFO);
            pSymbol->MaxNameLen = MAX_SYM_NAME;

            if (SymFromAddr(hProcess, (DWORD64)stack[i], &dwDisplacement, pSymbol))
            {
                stackTrace += "at ";
                stackTrace += pSymbol->Name;
                stackTrace += "\n  ";
            }
        }
#endif // !IL2CPP_TARGET_WINRT && !IL2CPP_TARGET_XBOXONE && !IL2CPP_TARGET_WINDOWS_GAMES

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
