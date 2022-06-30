#pragma once

#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    class LIBIL2CPP_CODEGEN_API Runtime
    {
    public:
        static int32_t CheckCrashReportLog_internal(intptr_t directory, bool clear);
        static Il2CppString* DumpStateSingle_internal(uint64_t* portable_hash, uint64_t* unportable_hash);
        static Il2CppString* DumpStateTotal_internal(uint64_t* portable_hash, uint64_t* unportable_hash);
        static Il2CppString* ExceptionToState_internal(Il2CppException* exc, uint64_t* portable_hash, uint64_t* unportable_hash);
        static Il2CppString* GetDisplayName();
        static Il2CppString* GetNativeStackTrace(Il2CppException* exception);
        static void AnnotateMicrosoftTelemetry_internal(intptr_t key, intptr_t val);
        static void EnableCrashReportLog_internal(intptr_t directory);
        static void mono_runtime_cleanup_handlers();
        static void mono_runtime_install_handlers();
        static void RegisterReportingForAllNativeLibs_internal();
        static void RegisterReportingForNativeLib_internal(intptr_t modulePathSuffix, intptr_t moduleName);
    };
} /* namespace Mono */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
