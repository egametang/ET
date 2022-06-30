#include "il2cpp-config.h"

#if IL2CPP_TARGET_WINDOWS && !RUNTIME_TINY

#include "DllMain.h"
#include "ThreadImpl.h"
#include "WindowsHeaders.h"

BOOL WINAPI DllMain(HINSTANCE hinstDLL, DWORD reason, LPVOID lpvReserved)
{
    if (reason == DLL_THREAD_DETACH)
        il2cpp::os::ThreadImpl::OnCurrentThreadExiting();

    return TRUE;
}

#if LIBIL2CPP_IS_IN_EXECUTABLE
typedef BOOL(WINAPI* DllMainFunc)(HINSTANCE hinstDLL, DWORD reason, LPVOID lpvReserved);
__declspec(dllimport) extern void Libil2cppLackeySetDllMain(DllMainFunc dllMain);
#endif

void il2cpp::os::InitializeDllMain()
{
#if LIBIL2CPP_IS_IN_EXECUTABLE && !IL2CPP_TINY
    Libil2cppLackeySetDllMain(DllMain);
#endif
}

#endif
