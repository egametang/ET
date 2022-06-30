#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppArray;
struct Il2CppObject;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API ValueType
    {
    public:
        static bool InternalEquals(Il2CppObject * thisPtr, Il2CppObject * that, Il2CppArray** fields);
        static int InternalGetHashCode(Il2CppObject *, Il2CppArray * *);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
