#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppArray;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API Environment
    {
    public:
        static bool get_HasShutdownStarted();
        static bool GetIs64BitOperatingSystem();
        static int32_t get_ExitCode();
        static int32_t get_ProcessorCount();
        static int32_t get_TickCount();
        static int32_t GetPageSize();
        static int32_t get_Platform();
        static Il2CppString* get_bundled_machine_config();
        static Il2CppString* get_MachineName();
        static Il2CppString* get_UserName();
        static Il2CppString* GetMachineConfigPath();
        static Il2CppString* GetNewLine();
        static Il2CppString* GetOSVersionString();
        static Il2CppString* GetWindowsFolderPath(int32_t folder);
        static Il2CppString* internalGetEnvironmentVariable_native(intptr_t variable);
        static Il2CppString* internalGetHome();
        static Il2CppArray* GetCommandLineArgs();
        static Il2CppArray* GetEnvironmentVariableNames();
        static Il2CppArray* GetLogicalDrivesInternal();
        static void Exit(int32_t exitCode);
        static void FailFast(Il2CppString* message, Il2CppException* exception, Il2CppString* errorSource);
        static void InternalSetEnvironmentVariable(Il2CppChar* variable, int32_t variable_length, Il2CppChar* value, int32_t value_length);
        static void set_ExitCode(int32_t value);
#if IL2CPP_TINY
        static Il2CppString* GetStackTrace_internal();
        static void FailFast_internal(Il2CppString* message);
#endif
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
