#include "il2cpp-config.h"
#include "icalls/System/System.Diagnostics/Stopwatch.h"
#include "os/Time.h"

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
    int64_t Stopwatch::GetTimestamp()
    {
        return il2cpp::os::Time::GetTicks100NanosecondsMonotonic();
    }
} /* namespace Diagnostics */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
