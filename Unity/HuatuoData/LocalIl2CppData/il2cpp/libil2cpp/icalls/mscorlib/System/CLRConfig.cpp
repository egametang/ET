#include "il2cpp-config.h"
#include "CLRConfig.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    bool CLRConfig::CheckThrowUnobservedTaskExceptions()
    {
        // TODO : Properly support this icall at some point.  This requires knowning that the ThrowUnobservedTaskExceptions=true flag was
        // set in the app.config and return true here if it was
        return false;
    }
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
