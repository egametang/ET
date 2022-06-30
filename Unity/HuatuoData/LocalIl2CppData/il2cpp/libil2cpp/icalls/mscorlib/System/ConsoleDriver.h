#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppObject;
struct Il2CppDelegate;
struct Il2CppReflectionType;
struct Il2CppReflectionMethod;
struct Il2CppReflectionField;
struct Il2CppArray;
struct Il2CppException;
struct Il2CppReflectionModule;
struct Il2CppAssembly;
struct Il2CppAssemblyName;
struct Il2CppAppDomain;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API ConsoleDriver
    {
    public:
        static int32_t InternalKeyAvailable(int32_t ms_timeout);
        static bool SetBreak(bool wantBreak);
        static bool SetEcho(bool wantEcho);
        static bool TtySetup(Il2CppString* keypadXmit, Il2CppString* teardown, Il2CppArray** control_characters, int32_t** size);
        static bool Isatty(intptr_t handle);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
