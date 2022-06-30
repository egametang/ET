#pragma once

#include "il2cpp-config.h"
struct Il2CppArray;
struct Il2CppObject;
struct Il2CppReflectionMethod;
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
    class LIBIL2CPP_CODEGEN_API RemotingServices
    {
    public:
        static Il2CppReflectionMethod * GetVirtualMethod(Il2CppReflectionType*, Il2CppReflectionMethod*);
        static Il2CppObject* InternalExecute(Il2CppReflectionMethod*, Il2CppObject*, Il2CppArray*, Il2CppArray**);
    };
} /* namespace Remoting */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
