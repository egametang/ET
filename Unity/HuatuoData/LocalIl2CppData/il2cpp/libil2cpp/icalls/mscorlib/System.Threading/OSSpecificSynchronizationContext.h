#pragma once

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
    class LIBIL2CPP_CODEGEN_API OSSpecificSynchronizationContext
    {
    public:
        static Il2CppObject* GetOSContext();
        static void PostInternal(Il2CppObject* context, SynchronizationContextCallback callback, intptr_t arg);
    };
}
}
}
}
}
