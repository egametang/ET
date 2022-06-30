#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"
#include "Output.h"
#include "os/ErrorCodes.h"
#include "os/File.h"
#include "StringUtils.h"

using namespace il2cpp::utils;

static inline void WriteToHandle(il2cpp::os::FileHandle* handle, const char* message)
{
    int error = il2cpp::os::kErrorCodeSuccess;
    il2cpp::os::File::Write(handle, message, static_cast<int>(strlen(message)), &error);
    IL2CPP_ASSERT(error == il2cpp::os::kErrorCodeSuccess);
}

void Output::WriteToStdout(const char* message)
{
    WriteToHandle(os::File::GetStdOutput(), message);
}

void Output::WriteToStderr(const char* message)
{
    WriteToHandle(os::File::GetStdError(), message);
}
