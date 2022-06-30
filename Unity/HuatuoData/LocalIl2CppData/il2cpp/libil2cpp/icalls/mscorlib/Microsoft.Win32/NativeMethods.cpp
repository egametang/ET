#include "il2cpp-config.h"
#include "NativeMethods.h"

#include "os/Process.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Microsoft
{
namespace Win32
{
    int32_t NativeMethods::GetCurrentProcessId()
    {
        return os::Process::GetCurrentProcessId();
    }
} // namespace Win32
} // namespace Microsoft
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
