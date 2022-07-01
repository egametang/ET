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
namespace Reflection
{
    class LIBIL2CPP_CODEGEN_API MethodBase
    {
    public:
        static Il2CppReflectionMethod* GetCurrentMethod();
        static void* /* System.Reflection.MethodBody */ GetMethodBodyInternal(intptr_t handle);
        static Il2CppReflectionMethod* GetMethodFromHandleInternalType(intptr_t method, intptr_t type);

        static Il2CppReflectionMethod* GetMethodFromHandleInternalType_native(intptr_t method_handle, intptr_t type_handle, bool genericCheck);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
