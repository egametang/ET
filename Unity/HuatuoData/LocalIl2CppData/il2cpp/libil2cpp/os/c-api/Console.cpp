#include "os/c-api/il2cpp-config-platforms.h"


#include "os/Console.h"
#include "os/c-api/Console-c-api.h"
#include "os/File.h"
#include "utils/StringUtils.h"

extern "C"
{
#if !RUNTIME_TINY
    int32_t UnityPalConsoleInternalKeyAvailable(int32_t ms_timeout)
    {
        return il2cpp::os::Console::InternalKeyAvailable(ms_timeout);
    }

    int32_t UnityPalConsoleSetBreak(int32_t wantBreak)
    {
        return il2cpp::os::Console::SetBreak(wantBreak);
    }

    int32_t UnityPalConsoleSetEcho(int32_t wantEcho)
    {
        return il2cpp::os::Console::SetEcho(wantEcho);
    }

    int32_t UnityPalConsoleTtySetup(const char* keypadXmit, const char* teardown, uint8_t* control_characters, int32_t** size)
    {
        return il2cpp::os::Console::TtySetup(keypadXmit, teardown, control_characters, size);
    }

#endif

#if IL2CPP_TINY
    void STDCALL UnityPalConsoleWrite(const char* message, bool newline)
    {
        il2cpp::os::FileHandle* fileHandle = il2cpp::os::File::GetStdOutput();
        int unused;
        if (message == NULL)
        {
            il2cpp::os::File::Write(fileHandle, "", 0, &unused);
        }
        else
        {
            std::string formattedMessage = il2cpp::utils::StringUtils::Printf("%s%s", message, newline ? il2cpp::os::Console::NewLine() : "");
            il2cpp::os::File::Write(fileHandle, formattedMessage.c_str(), (int)formattedMessage.size(), &unused);
        }
    }

#endif
}
