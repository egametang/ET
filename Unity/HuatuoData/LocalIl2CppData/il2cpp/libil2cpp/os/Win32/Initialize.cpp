#include "il2cpp-config.h"
#include "os/Initialize.h"
#if IL2CPP_TARGET_WINDOWS

#include "os/Environment.h"
#include "os/WindowsRuntime.h"

#include "DllMain.h"
#include <crtdbg.h>

void il2cpp::os::Initialize()
{
#ifdef _DEBUG
    std::string buildMachine = il2cpp::os::Environment::GetEnvironmentVariable("UNITY_THISISABUILDMACHINE");
    if (!buildMachine.empty())
    {
        _CrtSetReportMode(_CRT_ASSERT, _CRTDBG_MODE_FILE | _CRTDBG_MODE_DEBUG);
        _CrtSetReportFile(_CRT_ASSERT, _CRTDBG_FILE_STDOUT);
        _CrtSetReportMode(_CRT_ERROR, _CRTDBG_MODE_FILE | _CRTDBG_MODE_DEBUG);
        _CrtSetReportFile(_CRT_ERROR, _CRTDBG_FILE_STDOUT);
    }
#endif

    // This is needed so we could extract exception text from bad hresults
#if !RUNTIME_TINY
    os::WindowsRuntime::EnableErrorReporting();
    os::InitializeDllMain();
#endif
}

#if !IL2CPP_TARGET_WINRT && !IL2CPP_TARGET_XBOXONE
void il2cpp::os::Uninitialize()
{
}

#endif

#endif
