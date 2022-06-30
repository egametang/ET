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

// Async file handle.
typedef struct Baselib_FileIO_AsyncFile {void* handle;} Baselib_FileIO_AsyncFile;

// Sync file handle.
typedef struct Baselib_FileIO_SyncFile {void* handle;} Baselib_FileIO_SyncFile;

// Event queue handle invalid constant.
static const Baselib_FileIO_EventQueue Baselib_FileIO_EventQueue_Invalid = { NULL };

// Async file handle invalid constant.
static const Baselib_FileIO_AsyncFile Baselib_FileIO_AsyncFile_Invalid = { NULL };

// Sync file handle invalid constant.
static const Baselib_FileIO_SyncFile Baselib_FileIO_SyncFile_Invalid = { (void*)-1 };

typedef enum Baselib_FileIO_OpenFlags_t
{
    // Allows read access to the file.
    Baselib_FileIO_OpenFlags_Read         = 0x01,
    // Allows write access to the file.
    Baselib_FileIO_OpenFlags_Write        = 0x02,
    // Opens existing file without changes or creates 0 size file if file doesn't exist.
    // On some platforms open will implicitly add write flag if required by native API's.
    Baselib_FileIO_OpenFlags_OpenAlways   = 0x04,
    // Always creates 0 size file.
    // On some platforms open will implicitly add write flag if required by native API's.
    Baselib_FileIO_OpenFlags_CreateAlways = 0x08,
} Baselib_FileIO_OpenFlags_t;
typedef uint32_t Baselib_FileIO_OpenFlags;

// File IO read request.
typedef struct Baselib_FileIO_ReadRequest
{
    // Offset in a file to read from.
    // If offset+size is pointing pass EOF, will read up to EOF bytes.
    // If offset is pointing pass EOF, will read 0 bytes.
    uint64_t offset;
    // Buffer to read to, must be available for duration of operation.
    void*    buffer;
    // Size of requested read.
    // If 0 is passed will read 0 bytes and raise no error.
    uint64_t size;
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
    // Bytes transferred during read.
    uint64_t bytesTransferred;
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

// Request dequeue to shutdown
//
// \param               eq Event queue to shutdown.
// \param               threadCount Number of threads to signal termination
//
// An empty queue will hang in Baselib_FileIO_EventQueue_Dequeue for as long as the timeout lasts.
// This function can be used to exit such a condition
BASELIB_API void Baselib_FileIO_EventQueue_Shutdown(
    Baselib_FileIO_EventQueue       eq,
    uint32_t                        threadCount
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
BASELIB_API Baselib_FileIO_AsyncFile Baselib_FileIO_AsyncOpen(
    Baselib_FileIO_EventQueue eq,
    const char*               pathname,
    uint64_t                  userdata,
    Baselib_FileIO_Priority   priority
);

// Asynchronously reads data from a file.
//
// Note scheduling reads on closed file is undefined.
//
// \param     file Async file to read from.
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
BASELIB_API void Baselib_FileIO_AsyncRead(
    Baselib_FileIO_AsyncFile   file,
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
// \param file Async file to close.
//             If invalid file handle is passed, will no-op.
//
// Please note errors are reported via Baselib_FileIO_EventQueue_Result::errorState
// If file is invalid handle, error can not be reported because event queue is not known.
// Possible error codes:
// - IOError:                     IO error occured.
BASELIB_API void Baselib_FileIO_AsyncClose(
    Baselib_FileIO_AsyncFile file
);

// Synchronously opens a file.
//
// Will try use the most open access permissions options that are available for each platform.
// Meaning it might be possible for other process to write to file opened via this API.
// On most platforms file can be simultaneously opened with different open flags.
// If you require more strict options, or platform specific access configuration, please use Baselib_FileIO_SyncFileFromNativeHandle.
//
// \param pathname  Platform defined pathname to open.
// \param openFlags Open flags.
//                  If file is created because one of Create flags is passed, it will have size of 0 bytes.
//
// Possible error codes:
// - InvalidArgument:             Invalid argument was passed.
// - RequestedAccessIsNotAllowed: Request access is not allowed.
// - IOError:                     Generic IO error occured.
//
// \returns SyncFile handle.
BASELIB_API Baselib_FileIO_SyncFile Baselib_FileIO_SyncOpen(
    const char*                           pathname,
    Baselib_FileIO_OpenFlags              openFlags,
    Baselib_ErrorState*                   errorState
);

// Transfer ownership of native handle to Baselib_FileIO_SyncFile handle.
//
// This function transfers ownership, meaning you don't need to close native handle yourself,
// instead returned SyncFile must closed via Baselib_FileIO_SyncClose.
// Implementations might cache information about the file state,
// so native handle shouldn't be used after transfering ownership.
//
// \param handle Platform defined native handle.
//               If invalid native handle is passed, will return Baselib_FileIO_SyncFile_Invalid.
// \param type   Platform defined native handle type from Baselib_FileIO_NativeHandleType enum.
//               If unsupported type is passed, will return Baselib_FileIO_SyncFile_Invalid.
//
// \returns SyncFile handle.
BASELIB_API Baselib_FileIO_SyncFile Baselib_FileIO_SyncFileFromNativeHandle(
    uint64_t handle,
    uint32_t type
);

// Synchronously reads data from a file.
//
// \param file   File to read from.
//               If invalid file handle is passed, will raise InvalidArgument error and return 0.
// \param offset Offset in the file to read data at.
//               If offset+size goes past end-of-file (EOF), function will read until EOF.
//               If offset points past EOF, will return 0.
// \param buffer Pointer to data to read into.
// \param size   Size of data to read.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
//
// \returns Amount of bytes read.
BASELIB_API uint64_t Baselib_FileIO_SyncRead(
    Baselib_FileIO_SyncFile file,
    uint64_t                offset,
    void*                   buffer,
    uint64_t                size,
    Baselib_ErrorState*     errorState
);

// Synchronously writes data to a file.
//
// \param file   File to write to.
//               If invalid file handle is passed, will raise InvalidArgument error and return 0.
// \param offset Offset in the file to write data at.
//               If offset+size goes past end-of-file (EOF), then file will be resized.
// \param buffer Pointer to data to write.
// \param size   Size of data to write.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
//
// \returns Amount of bytes written.
BASELIB_API uint64_t Baselib_FileIO_SyncWrite(
    Baselib_FileIO_SyncFile file,
    uint64_t                offset,
    const void*             buffer,
    uint64_t                size,
    Baselib_ErrorState*     errorState
);

// Synchronously flushes file buffers.
//
// Operating system might buffer some write operations.
// Flushing buffers is required to guarantee (best effort) writing data to disk.
//
// \param file File to flush.
//             If invalid file handle is passed, will no-op.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
BASELIB_API void Baselib_FileIO_SyncFlush(
    Baselib_FileIO_SyncFile file,
    Baselib_ErrorState*     errorState
);

// Synchronously changes file size.
//
// \param file File to get size of.
//             If invalid file handle is passed, will raise invalid argument error.
// \param size New file size.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
//
// \returns File size.
BASELIB_API void Baselib_FileIO_SyncSetFileSize(
    Baselib_FileIO_SyncFile file,
    uint64_t                size,
    Baselib_ErrorState*     errorState
);

// Synchronously retrieves file size.
//
// \param file File to get size of.
//             If invalid file handle is passed, will return 0.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
//
// \returns File size.
BASELIB_API uint64_t Baselib_FileIO_SyncGetFileSize(
    Baselib_FileIO_SyncFile file,
    Baselib_ErrorState*     errorState
);

// Synchronously closes a file.
//
// Close does not guarantee that the data was written to disk,
// Please use Baselib_FileIO_SyncFlush to guarantee (best effort) that data was written to disk.
//
// \param file File to close.
//             If invalid file handle is passed, will no-op.
//
// Possible error codes:
// - InvalidArgument:            Invalid argument was passed.
// - IOError:                    Generic IO error occured.
BASELIB_API void Baselib_FileIO_SyncClose(
    Baselib_FileIO_SyncFile file,
    Baselib_ErrorState*     errorState
);

#include <C/Baselib_FileIO.inl.h>

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
