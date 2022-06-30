#pragma once

struct Il2CppWaitHandle;
namespace il2cpp { namespace os { class Handle; } }

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API WaitHandle
    {
    public:
        static Il2CppWaitHandle* NewManualResetEvent(bool initialState);
        static os::Handle* GetPlatformHandle(Il2CppWaitHandle* waitHandle);
    };
} /* namespace vm */
} /* namespace il2cpp */
