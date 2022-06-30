#pragma once

#include "os/ErrorCodes.h"

namespace il2cpp
{
namespace os
{
    class Error
    {
    public:
        static ErrorCode GetLastError();
        static void SetLastError(ErrorCode code);
    };
}
}
