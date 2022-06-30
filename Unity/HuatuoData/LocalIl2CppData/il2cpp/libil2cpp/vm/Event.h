#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct EventInfo;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Event
    {
    public:
        // exported
        static uint32_t GetToken(const EventInfo *eventInfo);
    };
} /* namespace vm */
} /* namespace il2cpp */
