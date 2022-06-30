#if ENABLE_UNIT_TESTS

#include "UnitTest++.h"

#include "../Environment-c-api.h"
#include "../../Environment.h"

SUITE(Environment)
{
    TEST(ApiGetOsUserNameMatchesClass)
    {
        std::string class_user_name = il2cpp::os::Environment::GetOsUserName();
        char* api_user_name = UnityPalGetOsUserName();
        CHECK_EQUAL(class_user_name, api_user_name);
        free(api_user_name);
    }

    TEST(ApiGetMachineNameMatchesClass)
    {
        std::string class_machine_name = il2cpp::os::Environment::GetMachineName();
        char* api_machine_name = UnityPalGetMachineName();
        CHECK_EQUAL(class_machine_name, api_machine_name);
        free(api_machine_name);
    }

    TEST(ApiGetEnvironmentVariableMatchesClass)
    {
        const std::string name = "IL2CPP_TEST_ENVIRONMENT_VARIABLE";
        il2cpp::os::Environment::SetEnvironmentVariable(name, "TEST");
        std::string class_environonment_path = il2cpp::os::Environment::GetEnvironmentVariable(name);
        char* api_environment_path = UnityPalGetEnvironmentVariable(name.c_str());
        CHECK_EQUAL(class_environonment_path, api_environment_path);
        free(api_environment_path);
    }

    TEST(ApiSetEnvironmentVariable_SetsAnEnvironmentVariable)
    {
        const std::string name = "IL2CPP_TEST_ENVIRONMENT_VARIABLE";
        const std::string expected_value = "TEST";
        UnityPalSetEnvironmentVariable(name.c_str(), expected_value.c_str());
        CHECK_EQUAL(expected_value, il2cpp::os::Environment::GetEnvironmentVariable(name));
    }

    TEST(GetEnvironmentVariableForAVariableThatDoesNotExist_ReturnsNull)
    {
        CHECK_NULL(UnityPalGetEnvironmentVariable("TEST_VARIABLE_THAT_DOES_NOT_EXIST"));
    }

    TEST(GetHomeDirectoryMatchesClass)
    {
        std::string class_home_directory = il2cpp::os::Environment::GetHomeDirectory();
        char* api_home_directory = UnityPalGetHomeDirectory();
        CHECK_EQUAL(class_home_directory, api_home_directory);
        free(api_home_directory);
    }

    TEST(GetProcessorCount)
    {
        CHECK(UnityPalGetProcessorCount() > 0);
    }
}

#endif // ENABLE_UNIT_TESTS
