#pragma once

// Baselib Network Address

#include "Baselib_ErrorState.h"
#include "Baselib_Alignment.h"
#include "Internal/Baselib_EnumSizeCheck.h"

#include <string.h>

#ifdef __cplusplus
BASELIB_C_INTERFACE
{
#endif

// Address family.
typedef enum Baselib_NetworkAddress_Family
{
    Baselib_NetworkAddress_Family_Invalid = 0,
    Baselib_NetworkAddress_Family_IPv4 = 1,
    Baselib_NetworkAddress_Family_IPv6 = 2
} Baselib_NetworkAddress_Family;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_NetworkAddress_Family);

// Fixed size address structure, large enough to hold IPv4 and IPv6 addresses.
typedef struct Baselib_NetworkAddress
{
    union
    {
        uint8_t data[16];
        uint8_t ipv6[16]; // in network byte order
        uint8_t ipv4[4];  // in network byte order
    };
    BASELIB_ALIGN_AS(2) uint8_t port[2]; // in network byte order
    uint8_t family;
    uint8_t _padding; // Explicit padding to allow for deterministic bitwise compare.

    // Scope zone index for IPv6 (ignored for IPv4)
    // Defaults to zero if not specified.
    // Note that unlike the other fields in this struct, this is *not* in network byte order!
    uint32_t ipv6_scope_id;
} Baselib_NetworkAddress;

// Max length of any string representing an IP address
static const uint32_t Baselib_NetworkAddress_IpMaxStringLength = 46;

// Binary encode string representation of an address.
//
// Neither port not ipAddressBuffer scope id are parsed from the ip string.
// dstAddress->ipv6_scope_id is set to zero and needs to be manually set if required.
//
// Possible error codes:
//  - Baselib_ErrorCode_InvalidArgument - One or more of the input parameters are invalid
BASELIB_API void Baselib_NetworkAddress_Encode(
    Baselib_NetworkAddress*         dstAddress,
    Baselib_NetworkAddress_Family   family,
    const char                      ip[],
    uint16_t                        port,
    Baselib_ErrorState*             errorState
);

// Decode binary representation of an address.
//
// family, ipAddressBuffer, and port are all optional arguments.
// passing zero as ipAddressBufferLen is the same as passing an ipAddressBuffer nullptr.
// Port and IPv6 scope id are not encodeded to ipAddressBuffer.
//
// Possible error codes:
//  - Baselib_ErrorCode_InvalidArgument - srcAddress is null or otherwise invalid.
//  - Baselib_ErrorCode_InvalidBufferSize - ipAddressBuffer is too small to hold decoded ip address.
BASELIB_API void Baselib_NetworkAddress_Decode(
    const Baselib_NetworkAddress*   srcAddress,
    Baselib_NetworkAddress_Family*  family,
    char                            ipAddressBuffer[],
    uint32_t                        ipAddressBufferLen,
    uint16_t*                       port,
    Baselib_ErrorState*             errorState
);

// Returns zero initialized network address struct
static inline Baselib_NetworkAddress Baselib_NetworkAddress_Empty(void)
{
    Baselib_NetworkAddress address;
    memset(&address, 0, sizeof(address));
    return address;
}

typedef enum Baselib_NetworkAddress_AddressReuse
{
    Baselib_NetworkAddress_AddressReuse_DoNotAllow = 0,

    // Allow multiple sockets to be bound to the same address/port.
    // All sockets bound to the same address/port need to have this flag set.
    Baselib_NetworkAddress_AddressReuse_Allow = 1,
} Baselib_NetworkAddress_AddressReuse;
BASELIB_ENUM_ENSURE_ABI_COMPATIBILITY(Baselib_NetworkAddress_AddressReuse);

#ifdef __cplusplus
}
#endif
