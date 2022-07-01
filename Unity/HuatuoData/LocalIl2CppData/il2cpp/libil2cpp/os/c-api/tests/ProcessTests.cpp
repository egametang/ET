#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../../Process.h"
#include "../Process-c-api.h"


SUITE(Process)
{
    TEST(GetProcessID)
    {
        CHECK(UnityPalGetCurrentProcessId() > 0);
    }

    TEST(GetProcessIDMatchesClass)
    {
        CHECK_EQUAL(il2cpp::os::Process::GetCurrentProcessId(), UnityPalGetCurrentProcessId());
    }

    TEST(GetProcessHandleValid)
    {
        UnityPalProcessHandle* handle = NULL;
        handle = UnityPalGetProcess(UnityPalGetCurrentProcessId());

        CHECK_NOT_NULL(handle);
    }

// GetProcessName is only supported on macOS for Posix targets
#if IL2CPP_TARGET_POSIX && !IL2CPP_TARGET_DARWIN
    TEST(GetProcessHandleMatchesClass)
    {
        int processId = il2cpp::os::Process::GetCurrentProcessId();
        UnityPalProcessHandle* api_handle = UnityPalGetProcess(processId);
        il2cpp::os::ProcessHandle* class_handle = il2cpp::os::Process::GetProcess(processId);

        std::string api_name = il2cpp::os::Process::GetProcessName(api_handle);
        std::string class_name = il2cpp::os::Process::GetProcessName(class_handle);

        CHECK_EQUAL(class_name, api_name);
    }

    TEST(GetProcessNameNotNull)
    {
        int processId = il2cpp::os::Process::GetCurrentProcessId();
        UnityPalProcessHandle* processHandle = UnityPalGetProcess(processId);

        const char* processName = UnityPalGetProcessName(processHandle);

        CHECK_NOT_NULL(processName);
    }

    TEST(GetProcessNameValid)
    {
        int processId = il2cpp::os::Process::GetCurrentProcessId();
        UnityPalProcessHandle* processHandle = UnityPalGetProcess(processId);

        const char* processName = UnityPalGetProcessName(processHandle);

        CHECK(strlen(processName) > 0);
    }

    TEST(GetProcessNameMatchesClass)
    {
        int processId = il2cpp::os::Process::GetCurrentProcessId();
        UnityPalProcessHandle* api_handle = UnityPalGetProcess(processId);
        il2cpp::os::ProcessHandle* class_handle = il2cpp::os::Process::GetProcess(processId);

        const char* api_name = UnityPalGetProcessName(api_handle);
        std::string class_name = il2cpp::os::Process::GetProcessName(class_handle);

        CHECK_EQUAL(class_name.c_str(), api_name);
    }

    TEST(GetProcessNameWithInvalidId)
    {
        const char* processName = NULL;

        // First check the staticness, grab a real string
        processName = UnityPalGetProcessName(UnityPalGetProcess(UnityPalGetCurrentProcessId()));

        // Now test that it goes blank
        processName = UnityPalGetProcessName(UnityPalGetProcess(-9999999));

        CHECK_EQUAL("", processName);
    }
#endif
}

#endif // ENABLE_UNIT_TESTS
