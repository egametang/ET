#pragma once

#include "il2cpp-config.h"

#include <stdint.h>
#include <string>

#include "os/ErrorCodes.h"

namespace il2cpp
{
namespace os
{
    struct ErrorDesc
    {
        ErrorCode code;
        const char *message;
    };

    extern ErrorDesc common_messages[];

#ifndef IL2CPP_DISABLE_FULL_MESSAGES
    extern ErrorDesc messages[];
#endif

    class Messages
    {
    public:
        static std::string FromCode(ErrorCode code);
    };
}
}
