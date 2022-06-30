#pragma once

#include <string>
#include <stdint.h>

namespace il2cpp
{
namespace os
{
namespace Console
{
// Isatty is in File class
    int32_t InternalKeyAvailable(int32_t ms_timeout);
    bool SetBreak(bool wantBreak);
    bool SetEcho(bool wantEcho);
    bool TtySetup(const std::string& keypadXmit, const std::string& teardown, uint8_t* control_characters, int32_t** size);
    const char* NewLine();
}
}
}
