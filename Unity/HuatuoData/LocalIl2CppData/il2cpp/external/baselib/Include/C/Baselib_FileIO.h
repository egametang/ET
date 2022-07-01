#pragma once

// Baselib FileIO
//
// This is a file reading abstraction api heavily influenced by next-gen async API's like io_uring, windows register I/O, etc.
// This api allows for platform independent async file reading.

#include "Baselib_ErrorState.h"
#include "Baselib_Memory.h"
#include "Internal/Baselib_EnumSizeCheck.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Event queue handle.
typedef struct Baselib_FileIO_EventQueue {void* handle;} Baselib_FileIO_EventQueue;

// File handle.
typedef struct Baselib_FileIO_File {void* handle;} Baselib_FileIO_File;

// Event queue handle invalid constant.
static const Baselib_FileIO_EventQueue Baselib_FileIO_EventQueue_Invalid = { NULL };

// File handle invalid constant.
static const Baselib_FileIO_File Baselib_FileIO_File_Invalid = { NULL };

// File IO read request.
typedef struct Baselib_FileIO_ReadRequest
{
    // Offset in a file to read from.
    uint64_t  offset;
    // Buffer to read to, must be available for duration of operation.
    void*     buffer;
    // Size of requested read, please note this it's 32 bit value.
    uint32_t  size;
} Baselib_FileIO_ReadRequest;

// File IO priorities.
// First we process all requests with high priority, then with normal priority.
// There's no round-robin, and high priority can starve normal priority.
typedef enum Baselib_FileIO_Priority
{
    Baselib_FileIO_Priority_Normal = 0,
    Baselib_FileIO_Priority_High   = 1
} Baselib_FileIO_Priority;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_FileIO_Priority);

typedef enum Baselib_FileIO_EventQueue_ResultType
{
    // Upon receiving this event, please call the provided callback with provided data argument.
    Baselib_FileIO_EventQueue_Callback     = 1,
    // Result of open file operation.
    Baselib_FileIO_EventQueue_OpenFile     = 2,
    // Result of read file operation.
    Baselib_FileIO_EventQueue_ReadFile     = 3,
    // Result of close file operation.
    Baselib_FileIO_EventQueue_CloseFile    = 4
} Baselib_FileIO_EventQueue_ResultType;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_FileIO_EventQueue_ResultType);

typedef void (*EventQueueCallback)(uint64_t userdata);

typedef struct Baselib_FileIO_EventQueue_Result_Callback
{
    // Please invoke this callback with userdata from the event.
    EventQueueCallback callback;
} Baselib_FileIO_EventQueue_Result_Callback;

typedef struct Baselib_FileIO_EventQueue_Result_OpenFile
{
    // Size of the file as seen on during open.
    uint64_t fileSize;
} Baselib_FileIO_EventQueue_Result_OpenFile;

typedef struct Baselib_FileIO_EventQueue_Result_ReadFile
{
    // Bytes transfered during read, please note it's 32 bit value.
    uint32_t bytesTransfered;
} Baselib_FileIO_EventQueue_Result_ReadFile;

// Event queue result.
typedef struct Baselib_FileIO_EventQueue_Result
{
    // Event type.
    Baselib_FileIO_EventQueue_ResultType type;
    // Userdata as provided to the request.
    uint64_t                             userdata;
    // Error state of the operation.
    Baselib_ErrorState                   errorState;
    union
    {
        Baselib_FileIO_EventQueue_Result_Callback callback;
        Baselib_FileIO_EventQueue_Result_OpenFile openFile;
        Baselib_FileIO_EventQueue_Result_ReadFile readFile;
    };
} Baselib_FileIO_EventQueue_Result;

// Creates event queue.
//
// \returns Event queue.
BASELIB_API Baselib_FileIO_EventQueue Baselib_FileIO_EventQueue_Create(void);

// Frees event queue.
//
// \param eq event queue to free.
BASELIB_API void Baselib_FileIO_EventQueue_Free(
    Baselib_FileIO_EventQueue eq
);

// Dequeue events from event queue.
//
// \param                    eq Event queue to dequeue from.
// \param               results Results array to dequeue elements into.
//                              If null will return 0.
// \param                 count Amount of elements in results array.
//                              If equals 0 will return 0.
// \param timeoutInMilliseconds If no elements are present in the queue,
//                              waits for any elements to be appear for specified amount of time.
//                              If 0 is passed, wait is omitted.
//                              If elements are present, dequeues up-to-count elements, and wait is omitted.
//
// File operations errors are reported via Baselib_FileIO_EventQueue_Result::errorState
// Possible error codes:
// - InvalidPathname:             Requested pathname is invalid (not found, a directory, etc).
// - RequestedAccessIsNotAllowed: Access to requested pathname is not allowed.
// - IOError:                     IO error occured.
//
// \returns Amount of results filled.
BASELIB_API uint64_t Baselib_FileIO_EventQueue_Dequeue(
    Baselib_FileIO_EventQueue        eq,
    Baselib_FileIO_EventQueue_Result results[],
    uint64_t                         count,
    uint32_t                         timeoutInMilliseconds // 0 will return immediately
);

// Asynchronously opens a file.
//
// \param       eq Event queue to associate file with.
//                 File can only be associated with one event queue,
//                 but one event queue can be associated with multiple files.
//                 If invalid event queue is passed, will return invalid file handle.
// \param pathname Platform defined pathname of a file.
//                 Can be freed after this function returns.
//                 If null is passed will return invalid file handle.
// \param userdata Userdata to be set in the completion event.
// \param priority Priority for file opening operation.
//
// Please note errors are reported via Baselib_FileIO_EventQueue_Result::errorState
// Possible error codes:
// - InvalidPathname:             Requested pathname is invalid (not found, a directory, etc).
// - RequestedAccessIsNotAllowed: Access to requested pathname is not allowed.
// - IOError:                     IO error occured.
//
// \returns Async file handle, which can be used immediately for scheduling other operations.
//          In case if file opening fails, all scheduled operations will fail as well.
//          In case if invalid arguments are passed, might return invalid file handle (see args descriptions).
BASELIB_API Baselib_FileIO_File Baselib_FileIO_File_Open(
    Baselib_FileIO_EventQueue eq,
    const char*               pathname,
    uint64_t                  userdata,
    Baselib_FileIO_Priority   priority
);

// Asynchronously reads data from a file.
//
// Note scheduling reads on closed file is undefined.
//
// \param     file File to read from.
//                 If invalid file handle is passed, will no-op.
//                 If file handle was already closed, behavior is undefined.
// \param requests Requests to schedule.
//                 If more than 1 provided,
//                 will provide completion event per individual request in the array.
//                 If null is passed, will no-op.
// \param    count Amount of requests in requests array.
//                 If 0 is passed, will no-op.
// \param userdata Userdata to be set in the completion event(s).
// \param priority Priority for file reading operation(s).
//
// Please note errors are reported via Baselib_FileIO_EventQueue_Result::errorState
// If file is invalid handle, error can not be reported because event queue is not known.
// Possible error codes:
// - IOError:                     IO error occured.
BASELIB_API void Baselib_FileIO_File_Read(
    Baselib_FileIO_File        file,
    Baselib_FileIO_ReadRequest requests[],
    uint64_t                   count,
    uint64_t                   userdata,
    Baselib_FileIO_Priority    priority
);

// Asynchronously closes a file.
//
// Will wait for all pending operations to complete,
// after that will close a file and put a completion event.
//
// \param file File to close.
//             If invalid file handle is passed, will no-op.
//
// Please note errors are reported via Baselib_FileIO_EventQueue_Result::errorState
// If file is invalid handle, error can not be reported because event queue is not known.
// Possible error codes:
// - IOError:                     IO error occured.
BASELIB_API void Baselib_FileIO_File_Close(
    Baselib_FileIO_File file
);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
