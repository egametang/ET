#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_SOCKET_BRIDGE
#include "os/SocketBridge.h"

namespace il2cpp
{
namespace os
{
    void SocketBridge::WaitForInitialization()
    {
        // Do nothing in the generic case, as there is no socket bridge to wait for.
    }
}
}

#endif // IL2CPP_USE_GENERIC_SOCKET_BRIDGE
