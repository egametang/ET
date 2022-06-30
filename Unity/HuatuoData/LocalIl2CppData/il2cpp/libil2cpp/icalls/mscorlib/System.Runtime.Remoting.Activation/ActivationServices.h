#pragma once

#include "il2cpp-config.h"
struct Il2CppObject;
struct Il2CppReflectionType;

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
namespace Remoting
{
namespace Activation
{
    class LIBIL2CPP_CODEGEN_API ActivationServices
    {
    public:
        static void EnableProxyActivation(Il2CppReflectionType*, bool);
        static Il2CppObject * AllocateUninitializedClassInstance(Il2CppReflectionType*);
    };
} /* namespace Activation */
} /* namespace Remoting */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
