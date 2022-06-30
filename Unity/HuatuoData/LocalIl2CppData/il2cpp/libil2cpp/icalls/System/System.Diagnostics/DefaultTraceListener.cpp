#include "il2cpp-config.h"

#include "icalls/System/System.Diagnostics/DefaultTraceListener.h"
#include "os/Debug.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Diagnostics
{
    void DefaultTraceListener::WriteWindowsDebugString(Il2CppChar* message)
    {
        DECLARE_IL2CPP_STRING_AS_STRING_VIEW_OF_NATIVE_CHARS(messageNative, message);
        il2cpp::os::Debug::WriteString(messageNative);
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
