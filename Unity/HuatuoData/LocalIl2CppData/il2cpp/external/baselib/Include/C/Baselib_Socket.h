#pragma once

// Baselib Socket
//
// This is a socket platform abstraction api heavily influenced by non-blocking Berkeley Sockets.
// Berkeley Sockets look like they behave in similar fashion on all platforms, but there are a lot of small differences.
// Compared to Berkeley Sockets this API is somewhat more high level and doesn't provide as fine grained control.
#include "Baselib_ErrorState.h"
#include "Baselib_NetworkAddress.h"
#include "Internal/Baselib_EnumSizeCheck.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Socket Handle, a handle to a specific socket.
typedef struct Baselib_Socket_Handle { intptr_t handle; } Baselib_Socket_Handle;
static const Baselib_Socket_Handle Baselib_Socket_Handle_Invalid = { -1 };

// Socket protocol.
typedef enum Baselib_Socket_Protocol
{
    Baselib_Socket_Protocol_UDP = 1,
    Baselib_Socket_Protocol_TCP = 2,
} Baselib_Socket_Protocol;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_Socket_Protocol);

// Socket message. Used to send or receive data in message based protocols such as UDP.
typedef struct Baselib_Socket_Message
{
    Baselib_NetworkAddress*         address;
    void*                           data;
    uint32_t                        dataLen;
} Baselib_Socket_Message;

// Create a socket.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument:           if context, family or protocol is invalid or unknown.
// - Baselib_ErrorCode_AddressFamilyNotSupported: if the requested address family is not available.
BASELIB_API Baselib_Socket_Handle Baselib_Socket_Create(
    Baselib_NetworkAddress_Family   family,
    Baselib_Socket_Protocol         protocol,
    Baselib_ErrorState*             errorState
);

// Bind socket to a local address and port.
//
// Bind can only be called once per socket.
// Address can either be a specific interface ip address.
// In case if encoded ip is nullptr / "0.0.0.0" / "::" (same as INADDR_ANY) will bind to all interfaces.
//
// \param addressReuse                     A set of sockets can be bound to the same address port combination if all
//                                         sockets are bound with this flag set to AddressReuse_Allow, similar to
//                                         SO_REUSEADDR+SO_REUSEPORT.
//                                         Please note that setting this flag to false doesn't mean anyone is forbidden
//                                         to binding to the same ip/port combo, or in other words it does NOT use
//                                         SO_EXCLUSIVEADDRUSE where it's available.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument:    Socket does not represent a valid open socket. Address pointer is null or incompatible.
// - Baselib_ErrorCode_AddressInUse:       Address or port is already bound by another socket, or the system is out of ephemeral ports.
// - Baselib_ErrorCode_AddressUnreachable: Address doesn't map to any known interface.
BASELIB_API void Baselib_Socket_Bind(
    Baselib_Socket_Handle               socket,
    const Baselib_NetworkAddress*       address,
    Baselib_NetworkAddress_AddressReuse addressReuse,
    Baselib_ErrorState*                 errorState
);

// Connect a socket to a remote address.
//
// Note that this function initiates an asynchronous connection. You must call
// Baselib_Socket_Poll with Baselib_Socket_PollEvents.requestedEvents =
// Baselib_Socket_PollEvents_Connected to wait for the connection to finish.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument:    Socket does not represent a valid socket, or socket is not a TCP socket. Address pointer is null or incompatible.
// - Baselib_ErrorCode_AddressUnreachable: Unable to establish a connection with peer.
BASELIB_API void Baselib_Socket_TCP_Connect(
    Baselib_Socket_Handle               socket,
    const Baselib_NetworkAddress*       address,
    Baselib_NetworkAddress_AddressReuse addressReuse,
    Baselib_ErrorState*                 errorState
);

// Bitmask of events to be used in Baselib_Socket_Poll
typedef enum Baselib_Socket_PollEvents
{
    Baselib_Socket_PollEvents_Readable = 1,
    Baselib_Socket_PollEvents_Writable = 2,
    // Note: Connected cannot be set at the same time as Readable and Writable.
    Baselib_Socket_PollEvents_Connected = 4,
} Baselib_Socket_PollEvents;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_Socket_PollEvents);

// Socket entry to be passed into Baselib_Socket_Poll.
//
// Note that the name `Fd` does not refer to the fact that these are file
// descriptors (they are sockets), but rather the fact that nearly every socket
// API calls this struct "pollfd".
typedef struct Baselib_Socket_PollFd
{
    Baselib_Socket_Handle           handle;
    Baselib_Socket_PollEvents       requestedEvents;
    Baselib_Socket_PollEvents       resultEvents;
    Baselib_ErrorState*             errorState;
} Baselib_Socket_PollFd;

// Helper method to construct a Baselib_Socket_PollFd. Use of this method is not
// necessary, you may fill out the struct yourself if desired.
static inline Baselib_Socket_PollFd Baselib_Socket_PollFd_New(Baselib_Socket_Handle handle, Baselib_Socket_PollEvents events, Baselib_ErrorState* errorState)
{
    Baselib_Socket_PollFd result;
    result.handle = handle;
    result.requestedEvents = events;
    result.resultEvents = (Baselib_Socket_PollEvents)0;
    result.errorState = errorState;
    return result;
}

// Wait for a socket being readable, writable, or an error occurs. Specific
// events that occurred will be set in sockets[i].resultEvents. Errors
// associated with particular sockets will be reported in sockets[i].errorState.
//
// It is valid to have sockets[i].errorState to point to the same ErrorState as
// the outer parameter errorState - or, more generally, you may alias whatever
// error states within sockets[i].errorState and the parameter errorState.
//
// If timeoutInMilliseconds==0, Poll() will not block. There is no option to
// wait indefinitely.
//
// Possible error codes on the outer parameter errorState:
// - Baselib_ErrorCode_InvalidArgument: Sockets list is null. An individual socket handle is invalid.
//
// Possible error codes on sockets[i].errorState:
// - Baselib_ErrorCode_AddressUnreachable: Asynchronous Connect() failed.
// - Baselib_ErrorCode_Disconnected: Socket has been disconnected, or asynchronous Connect() failed (apple devices).
BASELIB_API void Baselib_Socket_Poll(
    Baselib_Socket_PollFd*          sockets,
    uint32_t                        socketsCount,
    uint32_t                        timeoutInMilliseconds,
    Baselib_ErrorState*             errorState
);

// Get address of locally bound socket.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid bound socket. Address pointer is null.
BASELIB_API void Baselib_Socket_GetAddress(
    Baselib_Socket_Handle           socket,
    Baselib_NetworkAddress*         address,
    Baselib_ErrorState*             errorState
);

// Configure a TCP server socket to begin listening for incoming connections.
// The maximum queue size is used for each platform.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid socket, or socket is not a TCP socket.
// - Baselib_ErrorCode_AddressInUse: Another socket is already listening on the same port, or the system is out of ephemeral ports.
BASELIB_API void Baselib_Socket_TCP_Listen(
    Baselib_Socket_Handle           socket,
    Baselib_ErrorState*             errorState
);

// Accept an incoming TCP connection to this server socket. When there are no
// incoming connections, this returns Baselib_Socket_Handle_Invalid and does not
// raise an error.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid socket, or socket is not a TCP socket.
BASELIB_API Baselib_Socket_Handle Baselib_Socket_TCP_Accept(
    Baselib_Socket_Handle           socket,
    Baselib_ErrorState*             errorState
);

// Send messages to unconnected destinations.
//
// Socket does not need to be bound before calling SendMessages.
// When sending multiple messages an error may be raised after some of the messages were submitted.
//
// If the socket is not already bound to a port SendMessages will implicitly bind the socket before issuing the send operation.
//
// Warning: This function may not fail when called with a TCP socket, as it may
// simply ignore the address parameter, and send to whatever the socket is
// connected to. However, as there is no way to retreive the actual number of
// bytes sent with this API, its use in this manner is strongly discouraged.
//
// Known issues (behavior may change in the future):
// Some platforms do not support sending zero sized UDP packets.
//
// Possible error codes:
// - Baselib_ErrorCode_AddressUnreachable: Message destination is known to not be reachable from this machine.
// - Baselib_ErrorCode_InvalidArgument:    Socket does not represent a valid socket. Messages is `NULL` or a message has an invalid or incompatible destination.
// - Baselib_ErrorCode_InvalidBufferSize:  Message payload exceeds max message size.
//
// \returns The number of messages successfully sent. This number may be lower than messageCount if send buffer is full or an error was raised. Reported error will be about last message tried to send.
BASELIB_API uint32_t Baselib_Socket_UDP_Send(
    Baselib_Socket_Handle           socket,
    Baselib_Socket_Message          messages[],
    uint32_t                        messagesCount,
    Baselib_ErrorState*             errorState
);

// Send a message to the connected peer.
//
// \returns The possibly-zero length of the message actually sent, which may be less than `dataLen`.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid socket, or socket is not a TCP socket. Socket validity is not checked if dataLen==0.
// - Baselib_ErrorCode_Disconnected: Socket has been disconnected.
BASELIB_API uint32_t Baselib_Socket_TCP_Send(
    Baselib_Socket_Handle           socket,
    void*                           data,
    uint32_t                        dataLen,
    Baselib_ErrorState*             errorState
);

// Receive messages from unconnected sources.
//
// UDP message data that doesn't fit a message buffer is silently discarded.
//
// Warning: This function may not fail when called with a TCP socket, as it may
// simply ignore the address parameter, and receive from whatever the socket is
// connected to. However, as there is no way to retreive the actual number of
// bytes received with this API, its use in this manner is strongly discouraged.
//
// Known issues (behavior may change in the future):
// If the socket is not bound to a port RecvMessages will return zero without raising an error.
// Some platforms does not support receiveing zero sized UDP packets.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid socket. Or messages is `NULL`.
//
// \returns The number of messages successfully received. This number may be lower than messageCount if recv buffer is empty or an error was raised. Reported error will be about last message tried to receive.
BASELIB_API uint32_t Baselib_Socket_UDP_Recv(
    Baselib_Socket_Handle           socket,
    Baselib_Socket_Message          messages[],
    uint32_t                        messagesCount,
    Baselib_ErrorState*             errorState
);

// Receive a message from a connected source. Note that this method differs from
// traditional socket APIs in that it is valid to return 0, this means that no
// data were received. Disconnection is detected by errorState being
// Baselib_ErrorCode_Disconnected.
//
// This function may or may not work when passed a UDP socket. Graceful error
// handling of this case is omitted due to performance reasons.
//
// \returns The length of the message actually received, which may be less than `dataLen` or even zero.
//
// Possible error codes:
// - Baselib_ErrorCode_InvalidArgument: Socket does not represent a valid socket.
// - Baselib_ErrorCode_Disconnected: Socket has been disconnected.
BASELIB_API uint32_t Baselib_Socket_TCP_Recv(
    Baselib_Socket_Handle           socket,
    void*                           data,
    uint32_t                        dataLen,
    Baselib_ErrorState*             errorState
);

// Close socket.
//
// Closing an already closed socket results in a no-op.
BASELIB_API void Baselib_Socket_Close(
    Baselib_Socket_Handle           socket
);

#ifdef __cplusplus
}
#endif
