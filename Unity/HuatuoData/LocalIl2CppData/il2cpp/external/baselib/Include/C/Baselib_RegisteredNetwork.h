#pragma once

#include "Baselib_ErrorState.h"
#include "Baselib_Memory.h"
#include "Baselib_NetworkAddress.h"
#include "Internal/Baselib_EnumSizeCheck.h"

#include <stdint.h>

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// ------------------------------------------------------------------------------------------------
// Network buffers

// Implementation defined internal buffer id.
typedef void* Baselib_RegisteredNetwork_Buffer_Id;
static const Baselib_RegisteredNetwork_Buffer_Id Baselib_RegisteredNetwork_Buffer_Id_Invalid = 0;

// Network buffer structure.
// One buffer can contain multiple packets and endpoints.
typedef struct Baselib_RegisteredNetwork_Buffer
{
    Baselib_RegisteredNetwork_Buffer_Id id;
    Baselib_Memory_PageAllocation       allocation;
} Baselib_RegisteredNetwork_Buffer;

// Create a network buffer from a set of previously allocated memory pages.
//
// Possible error codes:
// - InvalidAddressRange:       if pageAllocation is invalid
//
// \returns A network buffer. If registration fails, then buffer id is set to Baselib_RegisteredNetwork_Buffer_Id_Invalid.
BASELIB_API Baselib_RegisteredNetwork_Buffer Baselib_RegisteredNetwork_Buffer_Register(
    Baselib_Memory_PageAllocation     pageAllocation,
    Baselib_ErrorState*               errorState
);

// Deregister network buffer. Disassociate memory pages and buffer representation.
//
// Allocated pages will stay allocated and can now be used for something else.
// Passing an invalid buffer results in a no-op.
BASELIB_API void Baselib_RegisteredNetwork_Buffer_Deregister(
    Baselib_RegisteredNetwork_Buffer  buffer
);

// ------------------------------------------------------------------------------------------------
// Network buffers slices

// Slice of a network buffer.
typedef struct Baselib_RegisteredNetwork_BufferSlice
{
    Baselib_RegisteredNetwork_Buffer_Id id;
    void*                               data; // data of the slice
    uint32_t                            size; // size of the slice in bytes
    uint32_t                            offset; // offset in main buffer
} Baselib_RegisteredNetwork_BufferSlice;

// Creates slice from network buffer
//
// \param buffer Buffer to create slice from.
// \param offset Offset in buffer in bytes.
// \param size   Size of the slice in bytes.
BASELIB_API Baselib_RegisteredNetwork_BufferSlice Baselib_RegisteredNetwork_BufferSlice_Create(
    Baselib_RegisteredNetwork_Buffer buffer,
    uint32_t                         offset,
    uint32_t                         size
);

// Create empty slice that doesn't point to anything
//
// Guaranteed to reference Baselib_RegisteredNetwork_Buffer_Id_Invalid and have all other values zeroed out.
BASELIB_API Baselib_RegisteredNetwork_BufferSlice Baselib_RegisteredNetwork_BufferSlice_Empty(void);

// ------------------------------------------------------------------------------------------------
// Network endpoints are platform defined representation (sockaddr_in-like) of network address (family, ip, port).

typedef struct Baselib_RegisteredNetwork_Endpoint { Baselib_RegisteredNetwork_BufferSlice slice; } Baselib_RegisteredNetwork_Endpoint;

static const uint32_t Baselib_RegisteredNetwork_Endpoint_MaxSize = 28; // in bytes

// Place network address into the network buffer.
//
// Destination must be able to accommodate Baselib_RegisteredNetwork_Endpoint_MaxSize bytes.
//
// \param srcAddress Network address to use, pass nullptr to create an empty endpoint.
// \param dstSlice   Where to write encoded data.
//
// Possible error codes:
// - InvalidArgument:       if dstSlice is invalid
// - InvalidBufferSize:     if dstSlice is smaller than Baselib_RegisteredNetwork_Endpoint_MaxSize
//
// \returns Endpoint or Endpoint_Empty in case of failure.
BASELIB_API Baselib_RegisteredNetwork_Endpoint Baselib_RegisteredNetwork_Endpoint_Create(
    const Baselib_NetworkAddress*         srcAddress,
    Baselib_RegisteredNetwork_BufferSlice dstSlice,
    Baselib_ErrorState*                   errorState
);

// Return empty endpoint that doesn't point to anything
//
// Guaranteed to contain Baselib_RegisteredNetwork_BufferSlice_Empty
BASELIB_API Baselib_RegisteredNetwork_Endpoint Baselib_RegisteredNetwork_Endpoint_Empty(void);

// Decode endpoint.
//
// \param endpoint   Endpoint to be converted.
// \param dstAddress Pointer to address to write data to.
//
// Possible error codes:
// - InvalidArgument: if endpoint is invalid or dstAddress is null
BASELIB_API void Baselib_RegisteredNetwork_Endpoint_GetNetworkAddress(
    Baselib_RegisteredNetwork_Endpoint endpoint,
    Baselib_NetworkAddress*            dstAddress,
    Baselib_ErrorState*                errorState
);

// ------------------------------------------------------------------------------------------------
// Request & Completion

// Send/receive request.
typedef struct Baselib_RegisteredNetwork_Request
{
    Baselib_RegisteredNetwork_BufferSlice payload;

    // for sending:     remote address to which the payload is sent (required for UDP)
    // for receiving:   address from which the data was sent (optional)
    Baselib_RegisteredNetwork_Endpoint    remoteEndpoint;

    // TODO: Not support yet. (We would also need to support this in Baselib_Socket first)
    // for sending:     unused
    // for receiving:   local address on which the data was received (optional)
    //Baselib_RegisteredNetwork_Endpoint    localEndpoint;

    void*                                 requestUserdata;
} Baselib_RegisteredNetwork_Request;

// Success or failure of a Baselib_RegisteredNetwork_CompletionResult.
typedef enum Baselib_RegisteredNetwork_CompletionStatus
{
    // Networking request failed.
    Baselib_RegisteredNetwork_CompletionStatus_Failed = 0,
    // Networking request successfully finished.
    Baselib_RegisteredNetwork_CompletionStatus_Success = 1,
} Baselib_RegisteredNetwork_CompletionStatus;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_RegisteredNetwork_CompletionStatus);

// Result of a previously scheduled send/receive
//
// When a networking request is completed, this is placed into an internal completion queue.
typedef struct Baselib_RegisteredNetwork_CompletionResult
{
    Baselib_RegisteredNetwork_CompletionStatus  status;
    uint32_t                                    bytesTransferred;
    void*                                       requestUserdata;
} Baselib_RegisteredNetwork_CompletionResult;

// ------------------------------------------------------------------------------------------------
// UDP connectionless socket.

typedef struct Baselib_RegisteredNetwork_Socket_UDP { struct Baselib_RegisteredNetwork_Socket_UDP_Impl* handle; } Baselib_RegisteredNetwork_Socket_UDP;
static const Baselib_RegisteredNetwork_Socket_UDP Baselib_RegisteredNetwork_Socket_UDP_Invalid = { NULL };

// Creates an UDP socket with internal request and completion queues.
//
// \param bindAddress     Address to bind socket to, in connectionless UDP every socket has to be bound.
// \param endpointReuse   Allows multiple sockets to be bound to the same address/port if set to AddressReuse_Allow,
//                        All sockets bound to the same address/port need to have this flag set.
// \param sendQueueSize   Send queue size in amount of entries.
// \param recvQueueSize   Receive queue size in amount of entries.
//
// Known issues (behavior may change in the future):
// - Some platforms do not support sending zero sized UDP packets.
//
// Possible error codes:
// - InvalidArgument:           if bindAddress pointer is null or incompatible or both sendQueueSize and recvQueueSize are zero
// - EndpointInUse:             endpoint is already in use
// - AddressFamilyNotSupported: if the requested address family is not available.
// - OutOfSystemResources:      if network session limit was exceeded
//
// \returns A UDP socket. If socket creation fails, socket holds a Baselib_RegisteredNetwork_Socket_UDP_InvalidHandle.
BASELIB_API Baselib_RegisteredNetwork_Socket_UDP Baselib_RegisteredNetwork_Socket_UDP_Create(
    const Baselib_NetworkAddress*       bindAddress,
    Baselib_NetworkAddress_AddressReuse endpointReuse,
    uint32_t                            sendQueueSize,
    uint32_t                            recvQueueSize,
    Baselib_ErrorState*                 errorState
);

// Schedules receive requests.
//
// \param socket        Socket to be used.
// \param requests      Array of pointers to requests. No-op if null.
//                      Request objects can be freed after the function call.
// \param requestsCount Amount of requests in the array. No-op if zero.
//
// If requests is null or requestsCount is zero, this operation is a no-op.
// Note that actual receiving may be deferred until you call Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv.
// UDP message data that doesn't fit a message buffer is silently discarded.
//
// Known issues (behavior may change in the future):
// - Some platforms does not support receiving zero sized UDP packets.
//
// Possible error codes:
// - InvalidArgument:   if socket is invalid
//
// \returns The number of scheduled items. If scheduling fails this function return zero.
BASELIB_API uint32_t Baselib_RegisteredNetwork_Socket_UDP_ScheduleRecv(
    Baselib_RegisteredNetwork_Socket_UDP     socket,
    const Baselib_RegisteredNetwork_Request* requests,
    uint32_t                                 requestsCount,
    Baselib_ErrorState*                      errorState
);

// Schedules send requests.
//
// \param socket        Socket to be used.
// \param requests      Array of pointers to requests. No-op if null.
//                      Request objects can be freed after the function call.
// \param requestsCount Amount of requests in the array. No-op if zero.
//
// If requests is null or requestsCount is zero, this operation is a no-op.
// Note that actual receiving may be deferred until you call Baselib_RegisteredNetwork_Socket_UDP_ProcessSend.
//
// Possible error codes:
// - InvalidArgument:   if socket is invalid
//
// \returns The number of scheduled items. If scheduling fails this function return zero.
BASELIB_API uint32_t Baselib_RegisteredNetwork_Socket_UDP_ScheduleSend(
    Baselib_RegisteredNetwork_Socket_UDP     socket,
    const Baselib_RegisteredNetwork_Request* requests,
    uint32_t                                 requestsCount,
    Baselib_ErrorState*                      errorState
);

// Status of processing send/recv.
typedef enum Baselib_RegisteredNetwork_ProcessStatus
{
    // No further items to process.
    //
    // Note that this does not imply that all requests have been fully processed at any moment in time.
    Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately = 0,

    // deprecated, same as Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately
    Baselib_RegisteredNetwork_ProcessStatus_Done
        COMPILER_DEPRECATED_ENUM_VALUE("Use Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately instead (equivalent)") = 0,

    // Should call again, there is more workload to process.
    Baselib_RegisteredNetwork_ProcessStatus_Pending = 1,
} Baselib_RegisteredNetwork_ProcessStatus;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_RegisteredNetwork_ProcessStatus);

// Processes the receive queue on a socket.
//
// Needs to be called periodically to ensure requests are processed.
// You should call this in loop until either your time budget is exceed or the function returns false.
//
// Platforms emulating RIO behavior with sockets, perform one receive per call until there are no more receive requests in the queue.
// Requests failed due to empty socket receive buffer are requeued and processed at the next call to Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv.
// In that case Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately is returned since an immediate retry will not have any effect.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns Baselib_RegisteredNetwork_ProcessStatus_Pending if there is more workload to process immediately, Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately if otherwise
BASELIB_API Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessRecv(
    Baselib_RegisteredNetwork_Socket_UDP socket,
    Baselib_ErrorState*                  errorState
);

// Processes the send queue on a socket.
//
// Needs to be called periodically to ensure requests are processed.
// You should call this in loop until either your time budget is exceed or the function returns false.
//
// Platforms emulating RIO behavior with sockets, perform one send per call until there are no more send requests in the queue.
// Requests failed due to full socket send buffer are requeued processed at the next call to Baselib_RegisteredNetwork_Socket_UDP_ProcessSend.
// In that case Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately is returned since an immediate retry will not have any effect.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns Baselib_RegisteredNetwork_ProcessStatus_Pending if there is more workload to process immediately, Baselib_RegisteredNetwork_ProcessStatus_NonePendingImmediately if otherwise
BASELIB_API Baselib_RegisteredNetwork_ProcessStatus Baselib_RegisteredNetwork_Socket_UDP_ProcessSend(
    Baselib_RegisteredNetwork_Socket_UDP socket,
    Baselib_ErrorState*                  errorState
);

// Status of a recv/send completion queue.
typedef enum Baselib_RegisteredNetwork_CompletionQueueStatus
{
    // No results are ready for dequeing.
    Baselib_RegisteredNetwork_CompletionQueueStatus_NoResultsAvailable = 0,
    // Results are available for dequeing.
    Baselib_RegisteredNetwork_CompletionQueueStatus_ResultsAvailable = 1,
} Baselib_RegisteredNetwork_CompletionQueueStatus;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_RegisteredNetwork_CompletionQueueStatus);

// Wait until results appears for a previously scheduled receive.
//
// \param timeoutInMilliseconds Wait timeout.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns Baselib_RegisteredNetwork_CompletionQueueStatus_ResultsAvailable if results are available for dequeue, Baselib_RegisteredNetwork_CompletionQueueStatus_NoResultsAvailable otherwise
BASELIB_API Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedRecv(
    Baselib_RegisteredNetwork_Socket_UDP socket,
    uint32_t                             timeoutInMilliseconds,
    Baselib_ErrorState*                  errorState
);

// Wait until results appears for a previously scheduled send.
//
// \param timeoutInMilliseconds Wait timeout.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns Baselib_RegisteredNetwork_CompletionQueueStatus_ResultsAvailable if results are available for dequeue, Baselib_RegisteredNetwork_CompletionQueueStatus_NoResultsAvailable otherwise
BASELIB_API Baselib_RegisteredNetwork_CompletionQueueStatus Baselib_RegisteredNetwork_Socket_UDP_WaitForCompletedSend(
    Baselib_RegisteredNetwork_Socket_UDP socket,
    uint32_t                             timeoutInMilliseconds,
    Baselib_ErrorState*                  errorState
);

// Dequeue receive result.
//
// \param results         Results array. No-op if null.
// \param resultsCount    Amount of elements in results array. No-op if zero.
//
// If you're calling this method on multiple threads for the same completion queue in parallel, it may spuriously return 0.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns number of dequeued entries
BASELIB_API uint32_t Baselib_RegisteredNetwork_Socket_UDP_DequeueRecv(
    Baselib_RegisteredNetwork_Socket_UDP       socket,
    Baselib_RegisteredNetwork_CompletionResult results[],
    uint32_t                                   resultsCount,
    Baselib_ErrorState*                        errorState
);

// Dequeue send result.
//
// \param results         Results array. No-op if null.
// \param resultsCount    Amount of elements in results array. No-op if zero.
//
// If you're calling this method on multiple threads for the same completion queue in parallel, it may spuriously return 0.
//
// Possible error codes:
// - InvalidArgument:     if socket is invalid
//
// \returns number of dequeued entries
BASELIB_API uint32_t Baselib_RegisteredNetwork_Socket_UDP_DequeueSend(
    Baselib_RegisteredNetwork_Socket_UDP       socket,
    Baselib_RegisteredNetwork_CompletionResult results[],
    uint32_t                                   resultsCount,
    Baselib_ErrorState*                        errorState
);

// Get bind address of udp socket.
//
// \param socket        Socket to be used.
// \param dstAddress    Pointer to address to write data to.
//
// Possible error codes:
// - InvalidArgument:   if socket is invalid or if dstAddress is null
BASELIB_API void Baselib_RegisteredNetwork_Socket_UDP_GetNetworkAddress(
    Baselib_RegisteredNetwork_Socket_UDP socket,
    Baselib_NetworkAddress*              dstAddress,
    Baselib_ErrorState*                  errorState
);

// Closes UDP socket.
//
// Passing an invalid socket handle result in a no-op.
//
// \param socket    Socket to be closed.
BASELIB_API void Baselib_RegisteredNetwork_Socket_UDP_Close(
    Baselib_RegisteredNetwork_Socket_UDP socket
);

// ------------------------------------------------------------------------------------------------

#ifdef __cplusplus
}
#endif
