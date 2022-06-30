#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppArray;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API CurrentSystemTimeZone
    {
    public:
        static bool GetTimeZoneData40(int year, Il2CppArray * *, Il2CppArray * *, bool * daylight_inverted);
    };
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
