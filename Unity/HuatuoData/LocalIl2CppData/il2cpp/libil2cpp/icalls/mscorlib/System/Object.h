#pragma once

#include <stdint.h>
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
    class LIBIL2CPP_CODEGEN_API Object
    {
    public:
        static int InternalGetHashCode(Il2CppObject* obj);
        static Il2CppObject* MemberwiseClone(Il2CppObject* obj);
        static Il2CppReflectionType* GetType(Il2CppObject* obj);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
