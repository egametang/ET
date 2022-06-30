#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS

#include "os/Console.h"

namespace il2cpp
{
namespace os
{
namespace Console
{
    int32_t InternalKeyAvailable(int32_t ms_timeout)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Console::InternalKeyAvailable);
        return 0;
    }

    bool SetBreak(bool wantBreak)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Console::SetBreak);
        return false;
    }

    bool SetEcho(bool wantEcho)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Console::SetEcho);
        return false;
    }

    bool TtySetup(const std::string& keypadXmit, const std::string& teardown, uint8_t* control_characters, int32_t** size)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Console::TtySetup);
        return false;
    }

    const char* NewLine()
    {
        return "\r\n";
    }
}
}
}

#endif
