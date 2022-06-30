#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"
#include "il2cpp-config.h"
#include "../Allocator.h"

#if IL2CPP_TARGET_PS4 || IL2CPP_TARGET_PS5
size_t g_args;
const void *g_argp;
#endif

// Have to define this due to Unity's customized
// version of UnitTest++ that integrates into the Unity
// editor.

extern "C"
{
    void printf_console_log(const char* log, va_list list) {}
}

// We can't use Il2CppNativeChar here due to an old bug in clang handling typdefs in main.
#if _MSC_VER
int main(int argc, const wchar_t* const argv[])
#else
int main(int argc, const char* const argv[])
#endif
{
    register_allocator(malloc);
    return UnitTest::RunAllTests();
}

#if IL2CPP_TARGET_WINDOWS

#if IL2CPP_TARGET_WINDOWS_DESKTOP
#include <windows.h>
#elif IL2CPP_TARGET_WINDOWS_GAMES
#include <windows.h>
#else
#include "ActivateApp.h"
#endif

int WINAPI wWinMain(HINSTANCE hInstance, HINSTANCE hPrevInstance, LPWSTR lpCmdLine, int nShowCmd)
{
#if IL2CPP_TARGET_WINDOWS_DESKTOP
    int argc;
    wchar_t** argv = CommandLineToArgvW(GetCommandLineW(), &argc);
    int returnValue = main(argc, argv);
    LocalFree(argv);
    return returnValue;
#elif IL2CPP_TARGET_WINDOWS_GAMES
    int result = main(__argc, __wargv);
    return result;
#else
    return WinRT::Activate(main);
#endif
}

#endif

#if IL2CPP_TARGET_ANDROID
//Needed to correct linker error since we don't compile with the GC that has the sig wrappers
extern "C"
{
    extern int __real_sigaction(int signum, const struct sigaction *action, struct sigaction *old_action);
    int __wrap_sigaction(int signum, const struct sigaction *action, struct sigaction *old_action)
    {
        return __real_sigaction(signum, action, old_action);
    }
}
#endif

#endif // ENABLE_UNIT_TESTS
