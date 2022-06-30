#include "il2cpp-config.h"

#include "os/Environment.h"
#include "os/c-api/Environment-c-api.h"
#include "Allocator.h"

extern "C"
{
#if !RUNTIME_TINY

    char* UnityPalGetOsUserName()
    {
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::os::Environment::GetOsUserName());
    }

    char* UnityPalGetMachineName()
    {
        return Allocator::CopyToAllocatedStringBuffer(il2cpp::os::Environment::GetMachineName());
    }

    char* UnityPalGetEnvironmentVariable(const char* name)
    {
        std::string name_string = name;
        std::string variable = il2cpp::os::Environment::GetEnvironmentVariable(name_string);
        if (variable.empty())
            return NULL;
        return Allocator::CopyToAllocatedStringBuffer(variable);
    }

    void UnityPalSetEnvironmentVariable(const char* name, const char* value)
    {
        std::string name_string = name;
        std::string value_string = value;
        il2cpp::os::Environment::SetEnvironmentVariable(name, value);
    }

    char* UnityPalGetHomeDirectory()
    {
        std::string home_directory = il2cpp::os::Environment::GetHomeDirectory();
        if (home_directory.empty())
            return NULL;
        return Allocator::CopyToAllocatedStringBuffer(home_directory);
    }

#endif

    int32_t UnityPalGetProcessorCount()
    {
        return il2cpp::os::Environment::GetProcessorCount();
    }
}
