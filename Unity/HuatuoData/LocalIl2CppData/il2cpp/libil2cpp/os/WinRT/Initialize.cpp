#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE

#include "os/Initialize.h"
#include "os/Win32/WindowsHelpers.h"

#if IL2CPP_TARGET_WINRT
#include "BrokeredFileSystem.h"
#endif

#include <io.h>

void il2cpp::os::Uninitialize()
{
#if IL2CPP_TARGET_WINRT
    BrokeredFileSystem::CleanupStatics();
#endif

    HANDLE stdoutHandle = reinterpret_cast<HANDLE>(_get_osfhandle(_fileno(stdout)));
    HANDLE stderrHandle = reinterpret_cast<HANDLE>(_get_osfhandle(_fileno(stderr)));

    if (stdoutHandle != INVALID_HANDLE_VALUE)
        FlushFileBuffers(stdoutHandle);

    if (stderrHandle != INVALID_HANDLE_VALUE)
        FlushFileBuffers(stderrHandle);
}

#endif
