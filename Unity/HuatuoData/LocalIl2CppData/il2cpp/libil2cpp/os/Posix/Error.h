#pragma once

#include <string>
#include <stdint.h>
#include "os/ErrorCodes.h"

namespace il2cpp
{
namespace os
{
    ErrorCode SocketErrnoToErrorCode(int32_t code);
    ErrorCode FileErrnoToErrorCode(int32_t code);
    ErrorCode PathErrnoToErrorCode(const std::string& path, int32_t code);
}
}
