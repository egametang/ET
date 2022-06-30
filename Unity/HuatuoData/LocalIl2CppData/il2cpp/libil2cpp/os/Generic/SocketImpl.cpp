#include "il2cpp-config.h"

#if IL2CPP_USE_GENERIC_SOCKET_IMPL && IL2CPP_SUPPORT_SOCKETS

#include "os/Generic/SocketImpl.h"

#define SOCKET_NOT_IMPLEMENTED \
    IL2CPP_ASSERT(0 && "The target platform does not support Sockets");

namespace il2cpp
{
namespace os
{
    void SocketImpl::Startup()
    {
    }

    void SocketImpl::Cleanup()
    {
    }

    WaitStatus SocketImpl::GetHostName(std::string &name)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addresses)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    SocketImpl::SocketImpl(ThreadStatusCallback thread_status_callback)
    {
        SOCKET_NOT_IMPLEMENTED
    }

    SocketImpl::~SocketImpl()
    {
    }

    WaitStatus SocketImpl::Create(AddressFamily family, SocketType type, ProtocolType protocol)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Create(SocketDescriptor fd, int32_t family, int32_t type, int32_t protocol)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Close()
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetBlocking(bool blocking)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    ErrorCode SocketImpl::GetLastError() const
    {
        SOCKET_NOT_IMPLEMENTED

        return (ErrorCode) - 1;
    }

    WaitStatus SocketImpl::Bind(const char *path)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Bind(const char *address, uint16_t port)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Bind(uint32_t address, uint16_t port)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Connect(const char *path)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Shutdown(int32_t how)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Disconnect(bool reuse)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Connect(uint32_t address, uint16_t port)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetLocalEndPointInfo(EndPointInfo &info)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetRemoteEndPointInfo(EndPointInfo &info)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Listen(int32_t backlog)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Accept(os::Socket **socket)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags c_flags)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags flags)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Available(int32_t *amount)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Ioctl(int32_t command, const uint8_t *in_data, int32_t in_len, uint8_t *out_data, int32_t out_len, int32_t *written)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetSocketOption(SocketOptionLevel level, SocketOptionName name, uint8_t *buffer, int32_t *length)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::GetSocketOptionFull(SocketOptionLevel level, SocketOptionName name, int32_t *first, int32_t *second)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::Poll(PollRequest& request, int32_t timeout, int32_t *result, int32_t *error)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetSocketOption(SocketOptionLevel level, SocketOptionName name, int32_t value)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetSocketOptionLinger(SocketOptionLevel level, SocketOptionName name, bool enabled, int32_t seconds)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetSocketOptionArray(SocketOptionLevel level, SocketOptionName name, const uint8_t *buffer, int32_t length)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, uint32_t group_address, uint32_t local_address)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

#if IL2CPP_SUPPORT_IPV6
    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, IPv6Address ipv6, uint64_t interfaceOffset)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }

#endif

    WaitStatus SocketImpl::SendFile(const char *filename, TransmitFileBuffers *buffers, TransmitFileOptions options)
    {
        SOCKET_NOT_IMPLEMENTED

        return kWaitStatusFailure;
    }
}
}
#endif
