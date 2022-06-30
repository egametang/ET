#include "il2cpp-config.h"

#if IL2CPP_TARGET_JAVASCRIPT
#include "os/SocketBridge.h"

#ifdef __EMSCRIPTEN_PTHREADS__
#include <emscripten/threading.h>
#include <emscripten/posix_socket.h>
#endif // __EMSCRIPTEN_PTHREADS__

namespace il2cpp
{
namespace os
{
    void SocketBridge::WaitForInitialization()
    {
#ifdef __EMSCRIPTEN_PTHREADS__
        EMSCRIPTEN_WEBSOCKET_T bridgeSocket = emscripten_init_websocket_to_posix_socket_bridge("ws://localhost:6690");
        // Synchronously wait until connection has been established
        uint16_t readyState = 0;
        do
        {
            emscripten_websocket_get_ready_state(bridgeSocket, &readyState);
            emscripten_thread_sleep(100);
        }
        while (readyState == 0);
#endif // __EMSCRIPTEN_PTHREADS__
    }
}
}

#endif // IL2CPP_TARGET_JAVASCRIPT
