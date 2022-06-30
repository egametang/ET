#include "il2cpp-config.h"
#include "Runtime.h"
#include <vm/String.h>
#include <vm/Exception.h>

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    int32_t Runtime::CheckCrashReportLog_internal(intptr_t directory, bool clear)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::CheckCrashReportLog_internal);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppString* Runtime::DumpStateSingle_internal(uint64_t* portable_hash, uint64_t* unportable_hash)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::DumpStateSingle_internal);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppString* Runtime::DumpStateTotal_internal(uint64_t* portable_hash, uint64_t* unportable_hash)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::DumpStateTotal_internal);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppString* Runtime::ExceptionToState_internal(Il2CppException* exc, uint64_t* portable_hash, uint64_t* unportable_hash)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::ExceptionToState_internal);
        IL2CPP_UNREACHABLE;
        return 0;
    }

    Il2CppString* Runtime::GetDisplayName()
    {
        return il2cpp::vm::String::New("Unity IL2CPP (" __DATE__ " " __TIME__ ")");
    }

    Il2CppString* Runtime::GetNativeStackTrace(Il2CppException* exception)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::GetNativeStackTrace);
        IL2CPP_UNREACHABLE;
        return NULL;
    }

    void Runtime::AnnotateMicrosoftTelemetry_internal(intptr_t key, intptr_t val)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::AnnotateMicrosoftTelemetry_internal);
        IL2CPP_UNREACHABLE;
    }

    void Runtime::EnableCrashReportLog_internal(intptr_t directory)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::EnableCrashReportLog_internal);
        IL2CPP_UNREACHABLE;
    }

    void Runtime::mono_runtime_cleanup_handlers()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::mono_runtime_cleanup_handlers);
        IL2CPP_UNREACHABLE;
    }

    void Runtime::mono_runtime_install_handlers()
    {
        NOT_SUPPORTED_IL2CPP(Runtime::mono_runtime_install_handlers, "This method appears to never be called.");
    }

    void Runtime::RegisterReportingForAllNativeLibs_internal()
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::RegisterReportingForAllNativeLibs_internal);
        IL2CPP_UNREACHABLE;
    }

    void Runtime::RegisterReportingForNativeLib_internal(intptr_t modulePathSuffix, intptr_t moduleName)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Runtime::RegisterReportingForNativeLib_internal);
        IL2CPP_UNREACHABLE;
    }
} /* namespace Mono */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
