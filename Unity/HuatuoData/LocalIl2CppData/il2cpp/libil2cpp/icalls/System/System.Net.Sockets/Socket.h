#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
#include "il2cpp-object-internals.h"

struct Il2CppArray;
struct Il2CppObject;
struct Il2CppString;

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
        static intptr_t Accept(intptr_t, int32_t*, bool);
        static int32_t Available(intptr_t, int32_t*);
        static void Bind(intptr_t, Il2CppSocketAddress*, int32_t*);
        static void Blocking(intptr_t, bool, int32_t*);
        static void Close(intptr_t, int32_t*);
        static void Connect(intptr_t, Il2CppSocketAddress*, int32_t*);
        static void Disconnect(intptr_t, bool, int32_t*);
        static void GetSocketOptionArray(intptr_t, SocketOptionLevel, SocketOptionName, Il2CppArray**, int32_t*);
        static void GetSocketOptionObj(intptr_t, SocketOptionLevel, SocketOptionName, Il2CppObject**, int32_t*);
        static void Listen(intptr_t, int32_t, int32_t*);
        static Il2CppSocketAddress* LocalEndPoint(intptr_t, int32_t*);
        static bool Poll(intptr_t, SelectMode, int32_t, int32_t*);
        static int32_t ReceiveArray(intptr_t, Il2CppArray*, SocketFlags, int32_t*);
        static int32_t Receive(intptr_t, Il2CppArray*, int32_t, int32_t, SocketFlags, int32_t*);
        static int32_t RecvFrom(intptr_t, Il2CppArray*, int32_t, int32_t, SocketFlags, Il2CppSocketAddress**, int32_t*);
        static Il2CppSocketAddress* RemoteEndPoint(intptr_t, int32_t*);
        static void Select(Il2CppArray**, int32_t, int32_t*);
        static bool SendFile(intptr_t, Il2CppString*, Il2CppArray*, Il2CppArray*, TransmitFileOptions);
        static int32_t SendTo(intptr_t, Il2CppArray*, int32_t, int32_t, SocketFlags, Il2CppSocketAddress*, int32_t*);
        static int32_t SendArray(intptr_t, Il2CppArray*, SocketFlags, int32_t*);
        static int32_t Send(intptr_t, Il2CppArray*, int32_t, int32_t, SocketFlags, int32_t*);
        static void SetSocketOption(intptr_t, SocketOptionLevel, SocketOptionName, Il2CppObject*, Il2CppArray*, int32_t, int32_t*);
        static void Shutdown(intptr_t, SocketShutdown, int32_t*);
        static intptr_t Socket_internal(Il2CppObject * self, AddressFamily, SocketType, ProtocolType, int32_t*);
        static int32_t WSAIoctl(intptr_t, int32_t, Il2CppArray*, Il2CppArray*, int32_t*);
        static bool SendFile_internal(intptr_t sock, Il2CppString* filename, Il2CppArray* pre_buffer, Il2CppArray* post_buffer, int32_t flags, int32_t* error, bool blocking);
        static bool SupportsPortReuse(ProtocolType proto);
        static int32_t IOControl_internal(intptr_t sock, int32_t ioctl_code, Il2CppArray* input, Il2CppArray* output, int32_t* error);
        static int32_t ReceiveFrom_internal(intptr_t sock, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress** sockaddr, int32_t* error, bool blocking);
        static int32_t SendTo_internal(intptr_t sock, uint8_t* buffer, int32_t count, SocketFlags flags, Il2CppSocketAddress* sa, int32_t* error, bool blocking);
        static Il2CppSocketAddress* LocalEndPoint_internal(intptr_t socket, int32_t family, int32_t* error);
        static Il2CppSocketAddress* RemoteEndPoint_internal(intptr_t socket, int32_t family, int32_t* error);
        static void cancel_blocking_socket_operation(Il2CppObject* thread);
        static void Connect_internal(intptr_t sock, Il2CppSocketAddress* sa, int32_t* error, bool blocking);
        static bool Duplicate_internal(intptr_t handle, int32_t targetProcessId, intptr_t *duplicate_handle, int32_t *werror);

        static int32_t ReceiveArray40(intptr_t, void*, int32_t, SocketFlags, int32_t*, bool);
        static int32_t Receive40(intptr_t, uint8_t*, int32_t, SocketFlags, int32_t*, bool);
        static int32_t SendArray40(intptr_t, void*, int32_t, SocketFlags, int32_t*, bool);
        static int32_t Send40(intptr_t, uint8_t*, int32_t, SocketFlags, int32_t*, bool);
        static bool IsProtocolSupported_internal(int32_t networkInterface);
    };
} /* namespace Sockets */
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
