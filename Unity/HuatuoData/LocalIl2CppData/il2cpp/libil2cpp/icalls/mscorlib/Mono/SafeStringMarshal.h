#pragma once

#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
    class LIBIL2CPP_CODEGEN_API SafeStringMarshal
    {
    public:
        static intptr_t StringToUtf8(Il2CppString* str);
        static void GFree(intptr_t ptr);
    };
} // namespace Mono
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
