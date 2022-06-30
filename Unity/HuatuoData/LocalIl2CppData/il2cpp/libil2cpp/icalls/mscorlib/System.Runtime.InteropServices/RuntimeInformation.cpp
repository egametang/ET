#include "il2cpp-config.h"
#include "RuntimeInformation.h"
#include "vm/String.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Runtime
{
namespace InteropServices
{
    Il2CppString* RuntimeInformation::GetOSName()
    {
#if IL2CPP_TARGET_WINDOWS_DESKTOP
        return vm::String::New("windows");
#elif IL2CPP_TARGET_OSX
        return vm::String::New("osx");
#elif IL2CPP_TARGET_LINUX
        return vm::String::New("linux");
#else
        return vm::String::New("unknown");
#endif
    }

    Il2CppString* RuntimeInformation::GetRuntimeArchitecture()
    {
#if IL2CPP_TARGET_ARM64
        return vm::String::New("armv8");
#elif IL2CPP_TARGET_ARMV7
        return vm::String::New("arm");
#elif IL2CPP_TARGET_X86
        return vm::String::New("x86");
#elif IL2CPP_TARGET_X64
        return vm::String::New("x86-64");
#else
        NOT_SUPPORTED_IL2CPP(RuntimeInformation::GetRuntimeArchitecture, "This icall is not supported by il2cpp on this architecture.");
        return 0;
#endif
    }
} // namespace InteropServices
} // namespace Runtime
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
