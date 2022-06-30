#include "il2cpp-config.h"

#include "os/CrashHelpers.h"
#include "os/StackTrace.h"
#include "utils/Logging.h"

#include <string>

namespace il2cpp
{
namespace os
{
    void CrashHelpers::Crash()
    {
        std::string nativeStackTrace;
#if IL2CPP_ENABLE_NATIVE_STACKTRACES
        nativeStackTrace = il2cpp::os::StackTrace::NativeStackTrace();
#endif
        if (!nativeStackTrace.empty())
        {
            std::string nativeStackTraceMessage = "Native stack trace:\n" + nativeStackTrace;
            il2cpp::utils::Logging::Write(nativeStackTraceMessage.c_str());
        }
        else
        {
            il2cpp::utils::Logging::Write("No native stack trace exists. Make sure this is platform supports native stack traces.");
        }


        CrashHelpers::CrashImpl();
    }
}
}
