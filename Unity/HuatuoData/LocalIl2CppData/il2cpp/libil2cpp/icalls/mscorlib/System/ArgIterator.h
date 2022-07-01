#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct mscorlib_System_ArgIterator;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API ArgIterator
    {
    public:
        static void* /* System.TypedReference */ IntGetNextArg(ArgIterator self);
        static Il2CppTypedRef IntGetNextArg_mscorlib_System_TypedReference(mscorlib_System_ArgIterator * thisPtr);
        static Il2CppTypedRef IntGetNextArg_mscorlib_System_TypedReference_mscorlib_System_IntPtr(mscorlib_System_ArgIterator * thisPtr, intptr_t rth);
        static void Setup(mscorlib_System_ArgIterator * thisPtr, intptr_t argsp, intptr_t start);
        static intptr_t IntGetNextArgType(mscorlib_System_ArgIterator * thisPtr);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
