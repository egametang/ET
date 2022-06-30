#pragma once

#if IL2CPP_TARGET_WINDOWS

#include <string>
#include <vector>
#include <stdint.h>

#include "os/Socket.h"
#include "os/ErrorCodes.h"
#include "os/WaitStatus.h"
#include "utils/Expected.h"
#include "utils/NonCopyable.h"

struct sockaddr;

namespace il2cpp
{
namespace os
{
    class SocketImpl : public il2cpp::utils::NonCopyable
    {
    public:

        /// Handle for socket. Must be large enough to hold a pointer.
        typedef uint64_t SocketDescriptor;

        SocketImpl(ThreadStatusCallback thread_status_callback);
        ~SocketImpl();

        inline SocketDescriptor GetDescriptor()
        {
            return _fd;
        }

        ErrorCode GetLastError() const;

        WaitStatus Create(SocketDescriptor fd, int32_t family, int32_t type, int32_t protocol);
        WaitStatus Create(AddressFamily family, SocketType type, ProtocolType protocol);

        WaitStatus Close();
        bool IsClosed() { return (_fd == -1); }

        WaitStatus SetBlocking(bool blocking);

        WaitStatus Listen(int32_t blacklog);

        WaitStatus Bind(const char *path);
        WaitStatus Bind(const char *address, uint16_t port);
        WaitStatus Bind(uint32_t address, uint16_t port);
        WaitStatus Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port);

        WaitStatus Connect(const char *path);
        WaitStatus Connect(uint32_t address, uint16_t port);
        WaitStatus Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port);

        WaitStatus Disconnect(bool reuse);
        WaitStatus Shutdown(int32_t how);

        WaitStatus GetLocalEndPointInfo(EndPointInfo &info);
        WaitStatus GetRemoteEndPointInfo(EndPointInfo &info);

        WaitStatus Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus ReceiveFromInternal(const uint8_t *data, size_t count, int32_t flags, int32_t *len, struct sockaddr *from, int32_t *fromlen);

        WaitStatus Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);

        WaitStatus SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags c_flags);
        WaitStatus ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags c_flags);

        WaitStatus SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        utils::Expected<WaitStatus> SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);

        WaitStatus RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);
        utils::Expected<WaitStatus> RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);
        WaitStatus RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);

        WaitStatus Available(int32_t *amount);

        WaitStatus Accept(os::Socket **socket);

        WaitStatus Ioctl(int32_t command, const uint8_t *in_data, int32_t in_len, uint8_t *out_data, int32_t out_len, int32_t *written);

        WaitStatus GetSocketOption(SocketOptionLevel level, SocketOptionName name, uint8_t *buffer, int32_t *length);
        WaitStatus GetSocketOptionFull(SocketOptionLevel level, SocketOptionName name, int32_t *first, int32_t *second);

        WaitStatus SetSocketOption(SocketOptionLevel level, SocketOptionName name, int32_t value);
        WaitStatus SetSocketOptionLinger(SocketOptionLevel level, SocketOptionName name, bool enabled, int32_t seconds);
        WaitStatus SetSocketOptionArray(SocketOptionLevel level, SocketOptionName name, const uint8_t *buffer, int32_t length);
        WaitStatus SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, uint32_t group_address, uint32_t local_address);
#if IL2CPP_SUPPORT_IPV6
        WaitStatus SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, IPv6Address ipv6, uint64_t interfaceOffset);
#endif

        WaitStatus SendFile(const char *filename, TransmitFileBuffers *buffers, TransmitFileOptions options);

        static WaitStatus Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error);
        static WaitStatus Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error);
        static WaitStatus Poll(PollRequest& request, int32_t timeout, int32_t *result, int32_t *error);

        static WaitStatus GetHostName(std::string &name);
        static WaitStatus GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addresses);
        static WaitStatus GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size);
        static WaitStatus GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list);

        static void Startup();
        static void Cleanup();

    private:

        bool _is_valid;
        SocketDescriptor _fd;
        int32_t _domain;
        int32_t _type;
        int32_t _protocol;
        ErrorCode _saved_error;
        int32_t _still_readable;
        ThreadStatusCallback _thread_status_callback;

        void StoreLastError();
        void StoreLastError(int32_t error_no);

        WaitStatus ConnectInternal(struct sockaddr *sa, int32_t sa_size);
        WaitStatus SendToInternal(struct sockaddr *sa, int32_t sa_size, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus RecvFromInternal(struct sockaddr* sa, int32_t sa_size, const uint8_t* data, int32_t count, os::SocketFlags flags, int32_t* len, os::EndPointInfo& ep);
        WaitStatus SetSocketOptionInternal(int32_t level, int32_t name, const void *value, int32_t len);
    };
}
}

#endif
