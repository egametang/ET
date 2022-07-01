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
        static Il2CppString* internalGetGacPath();
        static int32_t get_ProcessorCount();
        static bool get_SocketSecurityEnabled();
        static void internalBroadcastSettingChange();
        static Il2CppString* GetOSVersionString();
        static Il2CppString* GetMachineConfigPath();
        static Il2CppArray* GetLogicalDrivesInternal();
        static Il2CppArray* GetEnvironmentVariableNames();
        static Il2CppArray* GetCommandLineArgs();
        static Il2CppString* internalGetHome();
        static Il2CppString*  get_NewLine();
        static Il2CppString* get_MachineName();
        static int32_t get_ExitCode();
        static bool get_HasShutdownStarted();
        static Il2CppString* get_EmbeddingHostName();
        static Il2CppString* get_UserName();
        static int get_Platform();
        static int32_t get_TickCount();
        static void Exit(int32_t exitCode);
        static Il2CppString* GetWindowsFolderPath(int32_t folder);
        static Il2CppString* internalGetEnvironmentVariable(Il2CppString *);
        static void set_ExitCode(int32_t value);
        static void InternalSetEnvironmentVariable(Il2CppString* variable, Il2CppString* value);
        static bool GetIs64BitOperatingSystem();
        static int32_t GetPageSize();
        static Il2CppString* GetNewLine();
        static Il2CppString* internalGetEnvironmentVariable_native(intptr_t variable);
        static Il2CppString* get_bundled_machine_config();
#if IL2CPP_TINY
        static Il2CppString* GetStackTrace_internal();
        static void FailFast_internal(Il2CppString* message);
#endif
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
