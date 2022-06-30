// Generic implementation for platforms that don't have this implemented
#include "il2cpp-config.h"
#include "os/Debug.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
#if !IL2CPP_PLATFORM_SUPPORTS_DEBUGGER_PRESENT
    bool Debug::IsDebuggerPresent()
    {
        return false;
    }

#endif

#if IL2CPP_USE_GENERIC_DEBUG_LOG
    void Debug::WriteString(const utils::StringView<Il2CppNativeChar>& message)
    {
        il2cpp::utils::StringUtils::Printf(message.Str());
    }

#endif
}
}
