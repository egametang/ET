#include "il2cpp-config.h"
#include "Exception.h"
#include "vm/Runtime.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool Exception::nIsTransient(int32_t hr)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Exception::nIsTransient);
        IL2CPP_UNREACHABLE;
        return false;
    }

    Il2CppObject* Exception::GetMethodFromStackTrace(Il2CppObject* stackTrace)
    {
        IL2CPP_NOT_IMPLEMENTED_ICALL(Exception::GetMethodFromStackTrace);
        IL2CPP_UNREACHABLE;
        return NULL;
    }

    void Exception::ReportUnhandledException(Il2CppException* exception)
    {
        vm::Runtime::UnhandledException(exception);
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
