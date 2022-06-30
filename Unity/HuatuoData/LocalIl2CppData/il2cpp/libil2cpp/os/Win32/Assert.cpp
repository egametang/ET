#include "os/Assert.h"

#if IL2CPP_DEBUG

#if IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINRT
#include <crtdbg.h>

void il2cpp_assert(const char* assertion, const char* file, unsigned int line)
{
    if (_CrtDbgReport(_CRT_ASSERT, file, line, "", "%s", assertion) == 1)
    {
        _CrtDbgBreak();
    }
}

#endif // IL2CPP_TARGET_WINDOWS || IL2CPP_TARGET_XBOXONE || IL2CPP_TARGET_WINRT

#endif // IL2CPP_DEBUG
