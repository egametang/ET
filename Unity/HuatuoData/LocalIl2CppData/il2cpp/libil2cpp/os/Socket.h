#pragma once

#include <string>
#include <vector>

#include "il2cpp-config.h"

#include "os/ErrorCodes.h"
#include "os/Atomic.h"
#include "os/Mutex.h"
#include "os/WaitStatus.h"
#include "utils/NonCopyable.h"

namespace il2cpp
{
namespace os
{
    class SocketImpl;

    enum AddressFamily
    {
        kAddressFamilyError             = -1,
        kAddressFamilyUnspecified       = 0,// AF_UNSPEC
        kAddressFamilyUnix              = 1,// AF_UNIX
        kAddressFamilyInterNetwork      = 2,// AF_INET
        kAddressFamilyIpx               = 3,// AF_IPX
        kAddressFamilySna               = 4,// AF_SNA
        kAddressFamilyDecNet            = 5,// AF_DECnet
        kAddressFamilyAppleTalk         = 6,// AF_APPLETALK
        kAddressFamilyInterNetworkV6    = 7,// AF_INET6
        kAddressFamilyIrda              = 8,// AF_IRDA
    };

    enum SocketType
    {
        kSocketTypeError        = -1,
        kSocketTypeStream       = 0,// SOCK_STREAM
        kSocketTypeDgram        = 1,// SOCK_DGRAM
        kSocketTypeRaw          = 2,// SOCK_RAW
        kSocketTypeRdm          = 3,// SOCK_RDM
        kSocketTypeSeqpacket    = 4,// SOCK_SEQPACKET
    };

    enum ProtocolType
    {
        kProtocolTypeUnknown                            = -1,
        kProtocolTypeIP                                 = 0,
        kProtocolTypeIcmp                               = 1,
        kProtocolTypeIgmp                               = 2,
        kProtocolTypeGgp                                = 3,
        kProtocolTypeTcp                                = 6,
        kProtocolTypePup                                = 12,
        kProtocolTypeUdp                                = 17,
        kProtocolTypeIdp                                = 22,
        kProtocolTypeND                                 = 77,
        kProtocolTypeRaw                                = 255,
        kProtocolTypeUnspecified                        = 0,
        kProtocolTypeIpx                                = 1000,
        kProtocolTypeSpx                                = 1256,
        kProtocolTypeSpxII                              = 1257,

// #if NET_1_1
        kProtocolTypeIPv6                               = 41,
// #endif

// #if NET_2_0
        kProtocolTypeIPv4                               = 4,
        kProtocolTypeIPv6RoutingHeader                  = 43,
        kProtocolTypeIPv6FragmentHeader                 = 44,
        kProtocolTypeIPSecEncapsulatingSecurityPayload  = 50,
        kProtocolTypeIPSecAuthenticationHeader          = 51,
        kProtocolTypeIcmpV6                             = 58,
        kProtocolTypeIPv6NoNextHeader                   = 59,
        kProtocolTypeIPv6DestinationOptions             = 60,
        kProtocolTypeIPv6HopByHopOptions                = 0,
// #endif
    };

    enum SocketFlags
    {
        kSocketFlagsNone                    = 0x00000000,
        kSocketFlagsOutOfBand               = 0x00000001,
        kSocketFlagsPeek                    = 0x00000002,
        kSocketFlagsDontRoute               = 0x00000004,
        kSocketFlagsMaxIOVectorLength       = 0x00000010,
// #if NET_2_0
        kSocketFlagsTruncated               = 0x00000100,
        kSocketFlagsControlDataTruncated    = 0x00000200,
        kSocketFlagsBroadcast               = 0x00000400,
        kSocketFlagsMulticast               = 0x00000800,
// #endif
        kSocketFlagsPartial                 = 0x00008000,
    };

    enum SocketOptionLevel
    {
        kSocketOptionLevelSocket    = 65535,
        kSocketOptionLevelIP        = 0,
        kSocketOptionLevelTcp       = 6,
        kSocketOptionLevelUdp       = 17,

//#if NET_1_1
        kSocketOptionLevelIPv6      = 41,
//#endif
    };

    enum SocketOptionName
    {
        kSocketOptionNameDebug                  = 1,
        kSocketOptionNameAcceptConnection       = 2,
        kSocketOptionNameReuseAddress           = 4,
        kSocketOptionNameKeepAlive              = 8,
        kSocketOptionNameDontRoute              = 16,
        kSocketOptionNameBroadcast              = 32,
        kSocketOptionNameUseLoopback            = 64,
        kSocketOptionNameLinger                 = 128,
        kSocketOptionNameOutOfBandInline        = 256,
        kSocketOptionNameDontLinger             = -129,
        kSocketOptionNameExclusiveAddressUse    = -5,
        kSocketOptionNameSendBuffer             = 4097,
        kSocketOptionNameReceiveBuffer          = 4098,
        kSocketOptionNameSendLowWater           = 4099,
        kSocketOptionNameReceiveLowWater        = 4100,
        kSocketOptionNameSendTimeout            = 4101,
        kSocketOptionNameReceiveTimeout         = 4102,
        kSocketOptionNameError                  = 4103,
        kSocketOptionNameType                   = 4104,
        kSocketOptionNameMaxConnections         = 2147483647,
        kSocketOptionNameIPOptions              = 1,
        kSocketOptionNameHeaderIncluded         = 2,
        kSocketOptionNameTypeOfService          = 3,
        kSocketOptionNameIpTimeToLive           = 4,
        kSocketOptionNameMulticastInterface     = 9,
        kSocketOptionNameMulticastTimeToLive    = 10,
        kSocketOptionNameMulticastLoopback      = 11,
        kSocketOptionNameAddMembership          = 12,
        kSocketOptionNameDropMembership         = 13,
        kSocketOptionNameDontFragment           = 14,
        kSocketOptionNameAddSourceMembership    = 15,
        kSocketOptionNameDropSourceMembership   = 16,
        kSocketOptionNameBlockSource            = 17,
        kSocketOptionNameUnblockSource          = 18,
        kSocketOptionNamePacketInformation      = 19,
        kSocketOptionNameNoDelay                = 1,
        kSocketOptionNameBsdUrgent              = 2,
        kSocketOptionNameExpedited              = 2,
        kSocketOptionNameNoChecksum             = 1,
        kSocketOptionNameChecksumCoverage       = 20,
        kSocketOptionNameIPv6Only               = 27,

// #if NET_2_0
        kSocketOptionNameHopLimit               = 21,
        kSocketOptionNameUpdateAcceptContext    = 28683,
        kSocketOptionNameUpdateConnectContext   = 28688,
// #endif
    };

    enum PollFlags
    {
        kPollFlagsNone  = 0,
        kPollFlagsIn    = 1,
        kPollFlagsPri   = 2,
        kPollFlagsOut   = 4,
        kPollFlagsErr   = 8,
        kPollFlagsHup   = 0x10,
        kPollFlagsNVal  = 0x20,
        kPollFlagsAny   = 0xffffffff
    };

    enum SocketError
    {
        kInterrupted    = 4,// EINTR on POSIX and WSAEINTR on Windows
        kInvalidHandle  = 9 // EBADF on POSIX and WSAEBADF on Windows
    };

    inline void operator|=(PollFlags& left, PollFlags right)
    {
        left = static_cast<PollFlags>(static_cast<int>(left) | static_cast<int>(right));
    }

    enum TransmitFileOptions
    {
        kTransmitFileOptionsUseDefaultWorkerThread  = 0x00000000,
        kTransmitFileOptionsDisconnect              = 0x00000001,
        kTransmitFileOptionsReuseSocket             = 0x00000002,
        kTransmitFileOptionsWriteBehind             = 0x00000004,
        kTransmitFileOptionsUseSystemThread         = 0x00000010,
        kTransmitFileOptionsUseKernelApc            = 0x00000020,
    };

    class Socket;

    struct PollRequest
    {
        PollRequest()
            : fd(-1)
            , events(kPollFlagsNone)
            , revents(kPollFlagsNone)
        {}

        PollRequest(int64_t value)
            : fd(value)
            , events(kPollFlagsNone)
            , revents(kPollFlagsNone)
        {}
        int64_t fd;
        PollFlags events;
        PollFlags revents;
    };

#if IL2CPP_SUPPORT_IPV6
    struct IPv6Address
    {
        uint8_t addr[16];
    };
#endif

// TODO: this should really be UNIX_PATH_MAX or SUN_LEN(n)
#define END_POINT_MAX_PATH_LEN  255

#if IL2CPP_COMPILER_MSVC
#pragma warning( push )
#pragma warning( disable : 4200 )
#endif

    struct EndPointInfo
    {
        AddressFamily family;

        union
        {
            struct
            {
                uint32_t port;
                uint32_t address;
            } inet;
            char path[END_POINT_MAX_PATH_LEN];
            uint8_t raw[IL2CPP_ZERO_LEN_ARRAY];
        } data;
    };

#if IL2CPP_COMPILER_MSVC
#pragma warning( pop )
#endif

// NOTE(gab): this must be binary compatible with Windows's WSABUF
    struct WSABuf
    {
        uint32_t length;
        void *buffer;
    };

// NOTE(gab): this must be binary compatible with Window's TRANSMIT_FILE_BUFFERS
    struct TransmitFileBuffers
    {
        void *head;
        uint32_t head_length;
        void *tail;
        uint32_t tail_length;
    };

// Note: this callback can be invoked by the os-specific implementation when an
// interrupt is received or when the native code is looping in a potentially long
// loop.
// If the callback retun false, the executiong of the os-specific method is
// gracefully interrupted, and an error is supposed to be returned by the
// os-specific implementation.
// The callback is allowed to throw exceptions (for example a ThreadAborted exception):
// in this case, it is up to the os-specific implementation to properly deal with
// cleaning up the temporarely allocated memory (if any).
    typedef bool (*ThreadStatusCallback)();

    class Socket : public il2cpp::utils::NonCopyable
    {
    public:

        Socket(ThreadStatusCallback thread_status_callback);
        ~Socket();

        // Note: this Create is only used internally
        WaitStatus Create(int64_t fd, int32_t family, int32_t type, int32_t protocol);
        WaitStatus Create(AddressFamily family, SocketType type, ProtocolType protocol);

        bool IsClosed();
        void Close();

        int64_t GetDescriptor();

        ErrorCode GetLastError() const;

        WaitStatus SetBlocking(bool blocking);

        WaitStatus Listen(int32_t blacklog);

        WaitStatus Bind(const char *path);
        WaitStatus Bind(uint32_t address, uint16_t port);
        WaitStatus Bind(const char *address, uint16_t port);
        WaitStatus Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port);

        WaitStatus Connect(const char *path);
        WaitStatus Connect(uint32_t address, uint16_t port);
        WaitStatus Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port);

        WaitStatus Disconnect(bool reuse);
        WaitStatus Shutdown(int32_t how);

        WaitStatus GetLocalEndPointInfo(EndPointInfo &info);
        WaitStatus GetRemoteEndPointInfo(EndPointInfo &info);

        WaitStatus Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);

        WaitStatus SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags c_flags);
        WaitStatus ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags c_flags);

        WaitStatus SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);
        WaitStatus SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len);

        WaitStatus RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);
        WaitStatus RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);
        WaitStatus RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep);

        WaitStatus Available(int32_t *amount);

        WaitStatus Accept(Socket **socket);

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

#if IL2CPP_SUPPORT_IPV6_SUPPORT_QUERY
        static bool IsIPv6Supported();
#endif

        static WaitStatus Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error);
        static WaitStatus Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error);
        static WaitStatus Poll(PollRequest &request, int32_t timeout, int32_t *result, int32_t *error);

        static WaitStatus GetHostName(std::string &name);
        static WaitStatus GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addresses);

        // The pointers in addr_list are allocated with the il2cpp::utils::Memory::Malloc method. They should he freed by the caller using il2cpp::utils::Memory::Free.
        static WaitStatus GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size);
        static WaitStatus GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list);

        static void Startup();
        static void Cleanup();

    private:
        SocketImpl* m_Socket;
    };

/// Sockets should generally be referenced through SocketHandles for thread-safety.
/// Handles are stored in a table and can be safely used even when the socket has already
/// been deleted.
    typedef uint32_t SocketHandle;

    enum
    {
        kInvalidSocketHandle = 0
    };

    SocketHandle CreateSocketHandle(Socket* socket);
    Socket* AcquireSocketHandle(SocketHandle handle);
    void ReleaseSocketHandle(SocketHandle handle);

    inline SocketHandle PointerToSocketHandle(void* ptr)
    {
        // Double cast to avoid warnings.
        return static_cast<SocketHandle>(reinterpret_cast<size_t>(ptr));
    }

/// Helper to automatically acquire and release a Socket within a scope.
    struct SocketHandleWrapper
    {
        SocketHandleWrapper()
            : m_Handle(kInvalidSocketHandle)
            , m_Socket(NULL) {}
        SocketHandleWrapper(SocketHandle handle)
            : m_Handle(handle)
        {
            m_Socket = AcquireSocketHandle(handle);
        }

        SocketHandleWrapper(const SocketHandleWrapper& other)
        {
            m_Handle = other.m_Handle;
            if (m_Handle != kInvalidSocketHandle)
                m_Socket = AcquireSocketHandle(m_Handle);
            else
                m_Socket = NULL;
        }

        ~SocketHandleWrapper()
        {
            Release();
        }

        void Acquire(SocketHandle handle)
        {
            Release();
            m_Handle = handle;
            m_Socket = AcquireSocketHandle(handle);
        }

        void Release()
        {
            if (m_Socket)
                ReleaseSocketHandle(m_Handle);
            m_Socket = NULL;
            m_Handle = kInvalidSocketHandle;
        }

        bool IsValid() const
        {
            return (m_Socket != NULL);
        }

        SocketHandle GetHandle() const
        {
            return m_Handle;
        }

        Socket* GetSocket() const
        {
            return m_Socket;
        }

        Socket* operator->() const
        {
            return GetSocket();
        }

        SocketHandleWrapper& operator=(const SocketHandleWrapper& other)
        {
            Acquire(other.GetHandle());
            return *this;
        }

    private:
        SocketHandle m_Handle;
        Socket* m_Socket;
    };
}
}
