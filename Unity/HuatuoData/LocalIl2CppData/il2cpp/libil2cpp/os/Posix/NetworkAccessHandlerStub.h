#pragma once

#if !IL2CPP_USE_NETWORK_ACCESS_HANDLER

#include <stdint.h>

namespace il2cpp
{
namespace os
{
    // Define a stub for platforms that don't require requesting access to network hardware
    class NetworkAccessHandler
    {
    public:
        typedef int SocketDescriptor;

        void InheritNetworkAccessState(SocketDescriptor _fd) {}
        void CancelNetworkAccess() {}
        bool RequestNetwork(SocketDescriptor _fd, const struct sockaddr *sa = NULL, int32_t sa_size = 0) { return true; }
        bool PrepareForBind(SocketDescriptor _fd, const struct sockaddr *sa = NULL, int32_t sa_size = 0) { return true; }
  #if IL2CPP_SUPPORT_IPV6
        bool PrepareForBind(SocketDescriptor _fd, const struct sockaddr_in6* sa = NULL, int32_t sa_size = 0) { return true; }
  #endif
        bool PrepareForConnect(SocketDescriptor _fd, const struct sockaddr *sa = NULL, int32_t sa_size = 0) { return true; }
        bool WaitForNetworkStatus(SocketDescriptor _fd, bool isConnect = false) { return true; }

        int32_t GetError() { return 0; }

        class Auto
        {
        public:
            bool RequestAccessForAddressInfo(bool isLocalNetworkMode = false) { return true; }
            int32_t GetError() { return 0; }
        };
    };
}
}
#endif
