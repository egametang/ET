#include "vm/Event.h"
#include "il2cpp-class-internals.h"

namespace il2cpp
{
namespace vm
{
    uint32_t Event::GetToken(const EventInfo *eventInfo)
    {
        return eventInfo->token;
    }
} /* namespace vm */
} /* namespace il2cpp */
