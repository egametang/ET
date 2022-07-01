#include "il2cpp-config.h"

#if IL2CPP_SUPPORT_SOCKETS

#include <map>

#include "os/Socket.h"
#include "os/Atomic.h"
#include "os/Mutex.h"

#include "Baselib.h"
#include "Cpp/ReentrantLock.h"

#if IL2CPP_TARGET_POSIX
# include "os/Posix/SocketImpl.h"
#elif IL2CPP_TARGET_WINDOWS
# include "os/Win32/SocketImpl.h"
#elif IL2CPP_TARGET_SWITCH
# include "os/SocketImpl.h"
#else
# include "os/Generic/SocketImpl.h"
#endif


namespace il2cpp
{
namespace os
{
    struct SocketHandleEntry
    {
        Socket* m_Socket;
        uint32_t m_RefCount;
    };

    typedef std::map<SocketHandle, SocketHandleEntry> SocketHandleTable;

    SocketHandle g_LastSocketHandle;
    baselib::ReentrantLock g_SocketHandleTableMutex;
    SocketHandleTable g_SocketHandleTable;

    SocketHandle CreateSocketHandle(Socket* socket)
    {
        // Allocate handle.
        SocketHandle newHandle = Atomic::Increment(&g_LastSocketHandle);

        // Populate entry.
        SocketHandleEntry handleEntry;
        handleEntry.m_Socket = socket;
        handleEntry.m_RefCount = 1;

        // Add to table.
        {
            FastAutoLock lock(&g_SocketHandleTableMutex);
            g_SocketHandleTable.insert(SocketHandleTable::value_type(newHandle, handleEntry));
        }

        return newHandle;
    }

    Socket* AcquireSocketHandle(SocketHandle handle)
    {
        if (handle == kInvalidSocketHandle)
            return NULL;

        FastAutoLock lock(&g_SocketHandleTableMutex);

        // Look up in table.
        SocketHandleTable::iterator iter = g_SocketHandleTable.find(handle);
        if (iter == g_SocketHandleTable.end())
            return NULL;

        // Increase reference count.
        SocketHandleEntry& entry = iter->second;
        ++entry.m_RefCount;

        return entry.m_Socket;
    }

    void ReleaseSocketHandle(SocketHandle handle)
    {
        if (handle == kInvalidSocketHandle)
            return;

        Socket* socketToDelete = NULL;
        {
            FastAutoLock lock(&g_SocketHandleTableMutex);

            // Look up in table.
            SocketHandleTable::iterator iter = g_SocketHandleTable.find(handle);
            if (iter == g_SocketHandleTable.end())
                return;

            // Decrease reference count.
            SocketHandleEntry& entry = iter->second;
            --entry.m_RefCount;
            if (!entry.m_RefCount)
            {
                // Kill socket. Should be the only place where we directly delete sockets that
                // have made it past the creation step.
                socketToDelete = entry.m_Socket;
                g_SocketHandleTable.erase(iter);
            }
        }

        // Perform the deletion after we have released the lock so we don't unnecessarily
        // prevent other threads from accessing the table.
        if (socketToDelete)
            delete socketToDelete;
    }

    void Socket::Startup()
    {
        return SocketImpl::Startup();
    }

    void Socket::Cleanup()
    {
        return SocketImpl::Cleanup();
    }

    WaitStatus Socket::GetHostName(std::string &name)
    {
        return SocketImpl::GetHostName(name);
    }

    WaitStatus Socket::GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        return SocketImpl::GetHostByAddr(address, name, aliases, addr_list);
    }

    WaitStatus Socket::GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addresses)
    {
        return SocketImpl::GetHostByName(host, name, aliases, addresses);
    }

    WaitStatus Socket::GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size)
    {
        return SocketImpl::GetHostByName(host, name, family, aliases, addr_list, addr_size);
    }

    Socket::Socket(ThreadStatusCallback thread_status_callback)
        : m_Socket(new SocketImpl(thread_status_callback))
    {
    }

    Socket::~Socket()
    {
        if (!IsClosed())
            Close();

        delete m_Socket;
        m_Socket = 0;
    }

    int64_t Socket::GetDescriptor()
    {
        if (IsClosed())
            return -1;

        return m_Socket->GetDescriptor();
    }

    WaitStatus Socket::Create(int64_t fd, int32_t family, int32_t type, int32_t protocol)
    {
        return m_Socket->Create((SocketImpl::SocketDescriptor)fd, family, type, protocol);
    }

    WaitStatus Socket::Create(AddressFamily family, SocketType type, ProtocolType protocol)
    {
        return m_Socket->Create(family, type, protocol);
    }

    bool Socket::IsClosed()
    {
        return m_Socket->IsClosed();
    }

    void Socket::Close()
    {
        m_Socket->Close();
    }

    ErrorCode Socket::GetLastError() const
    {
        return m_Socket->GetLastError();
    }

    WaitStatus Socket::SetBlocking(bool blocking)
    {
        return m_Socket->SetBlocking(blocking);
    }

    WaitStatus Socket::Bind(const char *path)
    {
        return m_Socket->Bind(path);
    }

    WaitStatus Socket::Bind(uint32_t address, uint16_t port)
    {
        return m_Socket->Bind(address, port);
    }

    WaitStatus Socket::Bind(const char *address, uint16_t port)
    {
        return m_Socket->Bind(address, port);
    }

    WaitStatus Socket::Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        return m_Socket->Bind(address, scope, port);
    }

    WaitStatus Socket::Connect(const char *path)
    {
        return m_Socket->Connect(path);
    }

    WaitStatus Socket::Connect(uint32_t address, uint16_t port)
    {
        return m_Socket->Connect(address, port);
    }

    WaitStatus Socket::Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
        return m_Socket->Connect(address, scope, port);
    }

    WaitStatus Socket::Disconnect(bool reuse)
    {
        return m_Socket->Disconnect(reuse);
    }

    WaitStatus Socket::Shutdown(int32_t how)
    {
        return m_Socket->Shutdown(how);
    }

    WaitStatus Socket::GetLocalEndPointInfo(EndPointInfo &info)
    {
        return m_Socket->GetLocalEndPointInfo(info);
    }

    WaitStatus Socket::GetRemoteEndPointInfo(EndPointInfo &info)
    {
        return m_Socket->GetRemoteEndPointInfo(info);
    }

    WaitStatus Socket::Listen(int32_t backlog)
    {
        return m_Socket->Listen(backlog);
    }

    WaitStatus Socket::Accept(Socket **socket)
    {
        return m_Socket->Accept(socket);
    }

    WaitStatus Socket::Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        return m_Socket->Receive(data, count, flags, len);
    }

    WaitStatus Socket::ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags c_flags)
    {
        return m_Socket->ReceiveArray(wsabufs, count, len, c_flags);
    }

    WaitStatus Socket::Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        return m_Socket->Send(data, count, flags, len);
    }

    WaitStatus Socket::SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags c_flags)
    {
        return m_Socket->SendArray(wsabufs, count, sent, c_flags);
    }

    WaitStatus Socket::SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        return m_Socket->SendTo(address, port, data, count, flags, len);
    }

    WaitStatus Socket::SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        return m_Socket->SendTo(path, data, count, flags, len);
    }

    WaitStatus Socket::SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        return m_Socket->SendTo(address, scope, port, data, count, flags, len);
    }

    WaitStatus Socket::RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        return m_Socket->RecvFrom(address, port, data, count, flags, len, ep);
    }

    WaitStatus Socket::RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        return m_Socket->RecvFrom(path, data, count, flags, len, ep);
    }

    WaitStatus Socket::RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        return m_Socket->RecvFrom(address, scope, port, data, count, flags, len, ep);
    }

    WaitStatus Socket::Available(int32_t *amount)
    {
        return m_Socket->Available(amount);
    }

    WaitStatus Socket::Ioctl(int32_t command, const uint8_t *in_data, int32_t in_len, uint8_t *out_data, int32_t out_len, int32_t *written)
    {
        return m_Socket->Ioctl(command, in_data, in_len, out_data, out_len, written);
    }

    WaitStatus Socket::GetSocketOption(SocketOptionLevel level, SocketOptionName name, uint8_t *buffer, int32_t *length)
    {
        return m_Socket->GetSocketOption(level, name, buffer, length);
    }

    WaitStatus Socket::GetSocketOptionFull(SocketOptionLevel level, SocketOptionName name, int32_t *first, int32_t *second)
    {
        return m_Socket->GetSocketOptionFull(level, name, first, second);
    }

    WaitStatus Socket::Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error)
    {
        return SocketImpl::Poll(requests, count, timeout, result, error);
    }

    WaitStatus Socket::Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error)
    {
        return SocketImpl::Poll(requests, timeout, result, error);
    }

    WaitStatus Socket::Poll(PollRequest& request, int32_t timeout, int32_t *result, int32_t *error)
    {
        return SocketImpl::Poll(request, timeout, result, error);
    }

    WaitStatus Socket::SetSocketOption(SocketOptionLevel level, SocketOptionName name, int32_t value)
    {
        return m_Socket->SetSocketOption(level, name, value);
    }

    WaitStatus Socket::SetSocketOptionLinger(SocketOptionLevel level, SocketOptionName name, bool enabled, int32_t seconds)
    {
        return m_Socket->SetSocketOptionLinger(level, name, enabled, seconds);
    }

    WaitStatus Socket::SetSocketOptionArray(SocketOptionLevel level, SocketOptionName name, const uint8_t *buffer, int32_t length)
    {
        return m_Socket->SetSocketOptionArray(level, name, buffer, length);
    }

    WaitStatus Socket::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, uint32_t group_address, uint32_t local_address)
    {
        return m_Socket->SetSocketOptionMembership(level, name, group_address, local_address);
    }

#if IL2CPP_SUPPORT_IPV6
    WaitStatus Socket::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, IPv6Address ipv6, uint64_t interfaceOffset)
    {
        return m_Socket->SetSocketOptionMembership(level, name, ipv6, interfaceOffset);
    }

#endif

#if IL2CPP_SUPPORT_IPV6_SUPPORT_QUERY
    bool Socket::IsIPv6Supported()
    {
        return SocketImpl::IsIPv6Supported();
    }

#endif

    WaitStatus Socket::SendFile(const char *filename, TransmitFileBuffers *buffers, TransmitFileOptions options)
    {
        return m_Socket->SendFile(filename, buffers, options);
    }
}
}

#endif
