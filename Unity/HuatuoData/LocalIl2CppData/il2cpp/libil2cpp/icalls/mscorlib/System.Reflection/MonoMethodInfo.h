#pragma once

#include <stdint.h>
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
    class LIBIL2CPP_CODEGEN_API MonoMethodInfo
    {
    public:
        static void get_method_info(intptr_t methodPtr, Il2CppMethodInfo* info);
        static void* /* System.Reflection.Emit.UnmanagedMarshal */ get_retval_marshal(intptr_t handle);
        static Il2CppArray* get_parameter_info(intptr_t methodPtr, Il2CppReflectionMethod* member);

        static int32_t get_method_attributes(intptr_t methodPtr);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
