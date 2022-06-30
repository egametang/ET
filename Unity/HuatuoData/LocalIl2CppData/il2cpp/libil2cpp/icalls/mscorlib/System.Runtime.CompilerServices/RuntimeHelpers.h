#pragma once

#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

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
namespace CompilerServices
{
    class LIBIL2CPP_CODEGEN_API RuntimeHelpers
    {
    public:
        static Il2CppObject* GetObjectValue(Il2CppObject* obj);
        static void RunClassConstructor(intptr_t type);
        static void RunModuleConstructor(intptr_t module);
        static int get_OffsetToStringData(void);
        static void InitializeArray(Il2CppArray* arr, intptr_t ptr);

        static bool SufficientExecutionStack();
    };
} /* namespace CompilerServices */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
