#include "il2cpp-config.h"
#include "icalls/mscorlib/Mono/Runtime.h"
#include "vm/String.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    Il2CppString* Runtime::GetDisplayName()
    {
        return il2cpp::vm::String::New("Unity IL2CPP (" __DATE__ " " __TIME__ ")");
    }

    void Runtime::mono_runtime_install_handlers()
    {
        NOT_SUPPORTED_IL2CPP(Runtime::mono_runtime_install_handlers, "This method appears to never be called.");
    }

    Il2CppString* Runtime::GetNativeStackTrace(Il2CppException* exception)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::GetNativeStackTrace);
        IL2CPP_UNREACHABLE;
        return NULL;
    }
} /* namespace Mono */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
