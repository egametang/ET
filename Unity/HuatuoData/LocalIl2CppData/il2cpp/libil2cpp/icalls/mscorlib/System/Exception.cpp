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
    void Exception::ReportUnhandledException(Il2CppException* exception)
    {
        vm::Runtime::UnhandledException(exception);
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
