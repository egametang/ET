#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppArray;
struct Il2CppObject;
struct Il2CppString;

namespace il2cpp
{
namespace os
{
    struct WSABuf;
}
}

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace System
{
namespace Net
{
namespace Sockets
{
    enum AddressFamily
    {
        kAddressFamilyUnknown           = -1,
        kAddressFamilyUnspecified       = 0,
        kAddressFamilyUnix              = 1,
        kAddressFamilyInterNetwork      = 2,
        kAddressFamilyImpLink           = 3,
        kAddressFamilyPup               = 4,
        kAddressFamilyChaos             = 5,
        kAddressFamilyNS                = 6,
        kAddressFamilyIpx               = 6,
        kAddressFamilyIso               = 7,
        kAddressFamilyOsi               = 7,
        kAddressFamilyEcma              = 8,
        kAddressFamilyDataKit           = 9,
        kAddressFamilyCcitt             = 10,
        kAddressFamilySna               = 11,
        kAddressFamilyDecNet            = 12,
        kAddressFamilyDataLink          = 13,
        kAddressFamilyLat               = 14,
        kAddressFamilyHyperChannel      = 15,
        kAddressFamilyAppleTalk         = 16,
        kAddressFamilyNetBios           = 17,
        kAddressFamilyVoiceView         = 18,
        kAddressFamilyFireFox           = 19,
        kAddressFamilyBanyan            = 21,
        kAddressFamilyAtm               = 22,
        kAddressFamilyInterNetworkV6    = 23,
        kAddressFamilyCluster           = 24,
        kAddressFamilyIeee12844         = 25,
        kAddressFamilyIrda              = 26,
        kAddressFamilyNetworkDesigners  = 28,
        kAddressFamilyMax               = 29,
    };

    enum SocketType
    {
        kSocketTypeUnknown  = -1,
        kSocketTypeStream   = 1,
        kSocketTypeDgram    = 2,
        kSocketTypeRaw      = 3,
        kSocketTypeRdm      = 4,
        kSocketTypeSeqpacket = 5,
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

// #if NET_2_0
        kSocketOptionNameHopLimit               = 21,
        kSocketOptionNameUpdateAcceptContext    = 28683,
        kSocketOptionNameUpdateConnectContext   = 28688,
// #endif
    };

    enum SelectMode
    {
        kSelectModeSelectRead   = 0,
        kSelectModeSelectWrite  = 1,
        kSelectModeSelectError  = 2,
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

    enum TransmitFileOptions
    {
        kTransmitFileOptionsUseDefaultWorkerThread  = 0x00000000,
        kTransmitFileOptionsDisconnect              = 0x00000001,
        kTransmitFileOptionsReuseSocket             = 0x00000002,
        kTransmitFileOptionsWriteBehind             = 0x00000004,
        kTransmitFileOptionsUseSystemThread         = 0x00000010,
        kTransmitFileOptionsUseKernelApc            = 0x00000020,
    };

    enum SocketShutdown
    {
        kSocketShutdownReceive  = 0,
        kSocketShutdownSend     = 1,
        kSocketShutdownBoth     = 2,
    };

    class LIBIL2CPP_CODEGEN_API Socket
    {
    public:
        static bool Duplicate_icall(intptr_t handle, int32_t targetProcessId, intptr_t* duplicateHandle, int32_t* error);
        static bool IsProtocolSupported_internal(int32_t networkInterface);
        static bool Poll_icall(intptr_t socket, SelectMode mode, int32_t timeout, int32_t* error);
        static bool SendFile_icall(intptr_t socket, Il2CppString* filename, Il2CppArray* pre_buffer, Il2CppArray* post_buffer, TransmitFileOptions flags, int32_t* error, bool blocking);
        static bool SupportsPortReuse(int32_t proto);
        static int32_t Available_icall(intptr_t socket, int32_t* error);
        static int32_t IOControl_icall(intptr_t socket, int32_t ioctl_code, Il2CppArray* input, Il2CppArray* output, int32_t* error);
        static int32_t Receive_array_icall(intptr_t socket, os::WSABuf* bufarray, int32_t count, SocketFlags flags, int32_t *error, bool blocking);
        static int32_t Receive_icall(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, int32_t* error, bool blocking);
        static int32_t ReceiveFrom_icall(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress** socket_address, int32_t* error, bool blocking);
        static int32_t Send_array_icall(intptr_t socket, os::WSABuf* bufarray, int32_t count, SocketFlags flags, int32_t* error, bool blocking);
        static int32_t Send_icall(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, int32_t* error, bool blocking);
        static int32_t SendTo_icall(intptr_t socket, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress* socket_address, int32_t* error, bool blocking);
        static intptr_t Accept_icall(intptr_t socket, int32_t* error, bool blocking);
        static intptr_t Socket_icall(AddressFamily family, SocketType type, ProtocolType proto, int32_t* error);
        static Il2CppSocketAddress* LocalEndPoint_icall(intptr_t socket, int32_t family, int32_t* error);
        static Il2CppSocketAddress* RemoteEndPoint_icall(intptr_t socket, int32_t family, int32_t* error);
        static void Bind_icall(intptr_t socket, Il2CppSocketAddress* socket_address, int32_t* error);
        static void Blocking_icall(intptr_t socket, bool block, int32_t* error);
        static void cancel_blocking_socket_operation(Il2CppObject* thread);
        static void Close_icall(intptr_t socket, int32_t* error);
        static void Connect_icall(intptr_t socket, Il2CppSocketAddress* sa, int32_t* error, bool blocking);
        static void Disconnect_icall(intptr_t socket, bool reuse, int32_t* error);
        static void GetSocketOption_arr_icall(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppArray** byte_val, int32_t *error);
        static void GetSocketOption_obj_icall(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppObject** obj_val, int32_t *error);
        static void Listen_icall(intptr_t socket, int32_t backlog, int32_t* error);
        static void Select_icall(Il2CppArray** sockets, int32_t microSeconds, int32_t* error);
        static void SetSocketOption_icall(intptr_t socket, SocketOptionLevel level, SocketOptionName name, Il2CppObject* obj_val, Il2CppArray* byte_val, int32_t int_val, int32_t* error);
        static void Shutdown_icall(intptr_t socket, SocketShutdown how, int32_t* error);
    };
} /* namespace Sockets */
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
