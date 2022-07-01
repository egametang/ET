#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppString;
struct Il2CppArray;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API String
    {
    public:
        static void RedirectToCreateString();
        static Il2CppString* InternalAllocateStr(int length);
        static Il2CppString* InternalIntern(Il2CppString* str);
        static Il2CppArray* InternalSplit(Il2CppString *, Il2CppArray*, int, int);
        static Il2CppString* InternalIsInterned(Il2CppString* str);
        static Il2CppString* FastAllocateString(int32_t length);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
