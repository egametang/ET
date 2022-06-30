#include "il2cpp-config.h"

#include "icalls/System/System.IO/FileSystemWatcher.h"
#include "os/FileSystemWatcher.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace IO
{
    int32_t FileSystemWatcher::InternalSupportsFSW()
    {
        return os::FileSystemWatcher::IsSupported();
    }
} /* namespace IO */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
