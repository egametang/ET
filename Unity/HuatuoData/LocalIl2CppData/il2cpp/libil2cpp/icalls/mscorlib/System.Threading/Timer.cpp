#include "il2cpp-config.h"
#include "Timer.h"
#include "os/Time.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Threading
{
    int64_t Timer::GetTimeMonotonic()
    {
        return os::Time::GetTicks100NanosecondsMonotonic();
    }
} // namespace Threading
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
