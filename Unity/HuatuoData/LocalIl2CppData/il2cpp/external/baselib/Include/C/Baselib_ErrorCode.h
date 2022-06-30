#pragma once

#include "Internal/Baselib_EnumSizeCheck.h"

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// All possible baselib error codes.
typedef enum Baselib_ErrorCode
{
    Baselib_ErrorCode_Success = 0x00000000,

    // Common
    Baselib_ErrorCode_OutOfMemory = 0x01000000,
    Baselib_ErrorCode_OutOfSystemResources,
    Baselib_ErrorCode_InvalidAddressRange,
    // nativeErrorCode contains name of invalid argument
    Baselib_ErrorCode_InvalidArgument,
    Baselib_ErrorCode_InvalidBufferSize,
    Baselib_ErrorCode_InvalidState,
    Baselib_ErrorCode_NotSupported,
    Baselib_ErrorCode_Timeout,

    // Memory
    Baselib_ErrorCode_UnsupportedAlignment = 0x02000000,
    Baselib_ErrorCode_InvalidPageSize,
    Baselib_ErrorCode_InvalidPageCount,
    Baselib_ErrorCode_UnsupportedPageState,

    // Thread
    Baselib_ErrorCode_ThreadCannotJoinSelf = 0x03000000,

    // Socket
    Baselib_ErrorCode_NetworkInitializationError = 0x04000000,
    Baselib_ErrorCode_AddressInUse,
    // Risen in case if destination cannot be reached or requested address for bind was not local.
    Baselib_ErrorCode_AddressUnreachable,
    Baselib_ErrorCode_AddressFamilyNotSupported,
    Baselib_ErrorCode_Disconnected,

    // FileIO
    Baselib_ErrorCode_InvalidPathname = 0x05000000,
    Baselib_ErrorCode_RequestedAccessIsNotAllowed,
    Baselib_ErrorCode_IOError,

    // DynamicLibrary
    Baselib_ErrorCode_FailedToOpenDynamicLibrary = 0x06000000,
    Baselib_ErrorCode_FunctionNotFound,

    // An error that was not anticipated by the baselib authors.
    // Occurrence of this error is preceeded by a debug assertion.
    Baselib_ErrorCode_UnexpectedError  = 0xFFFFFFFF,
} Baselib_ErrorCode;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_ErrorCode);

#ifdef __cplusplus
} // BASELIB_C_INTERFACE
#endif
