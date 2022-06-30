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
        static intptr_t IntGetNextArgType(mscorlib_System_ArgIterator* thisPtr);
        static void IntGetNextArg(mscorlib_System_ArgIterator* thisPtr, void* res);
        static void IntGetNextArgWithType(mscorlib_System_ArgIterator* thisPtr, void* res, intptr_t rth);
        static void Setup(mscorlib_System_ArgIterator* thisPtr, intptr_t argsp, intptr_t start);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
