#pragma once

#if IL2CPP_TARGET_WINDOWS

#include "WindowsHeaders.h"
#include "os/WaitStatus.h"

#if IL2CPP_TARGET_WINRT
#include "os/WinRT/Win32ApiWinRTEmulation.h"
#endif

#if IL2CPP_TARGET_XBOXONE
#include "os/XboxOne/Win32ApiXboxEmulation.h"
#endif


#if IL2CPP_TARGET_WINDOWS_GAMES
#include "os/WindowsGames/Win32ApiWindowsGamesEmulation.h"
#endif

#if IL2CPP_TARGET_WINRT || IL2CPP_TARGET_XBOXONE
#include "os/WinRT/Win32ApiSharedEmulation.h"
#endif

#include <vector>

namespace il2cpp
{
namespace os
{
namespace win
{
// Wait for a release of the given handle in way that can be interrupted by APCs.
    WaitStatus WaitForSingleObjectAndAccountForAPCs(HANDLE handle, uint32_t ms, bool interruptible);
    int32_t WaitForAnyObjectAndAccountForAPCs(const std::vector<HANDLE>& handles, uint32_t ms, bool interruptible);
    bool WaitForAllObjectsAndAccountForAPCs(const std::vector<HANDLE>& handles, uint32_t ms, bool interruptible);
}
}
}

#endif // IL2CPP_TARGET_WINDOWS
