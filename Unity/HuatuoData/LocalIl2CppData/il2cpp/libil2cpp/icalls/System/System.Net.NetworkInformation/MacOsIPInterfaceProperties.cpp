#include "il2cpp-config.h"
#include "MacOsIPInterfaceProperties.h"

#if IL2CPP_TARGET_OSX || IL2CPP_TARGET_IOS

#include "il2cpp-class-internals.h"
#include "gc/GarbageCollector.h"
#include "gc/WriteBarrier.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"
#include "vm/Array.h"
#include "vm/Class.h"
#include "vm/Path.h"
#include "vm/String.h"
#include "vm/Exception.h"
#include "utils/Memory.h"
#include "utils/StringUtils.h"

#include <sys/socket.h>
#include <net/if.h>
#include <net/if_dl.h>
#if IL2CPP_TARGET_OSX
#include <net/route.h>
#else

// Borrowed necessary defines and structs from the macOS route.h for use on iOS/tvOS

/*
 * These numbers are used by reliable protocols for determining
 * retransmission behavior and are included in the routing structure.
 */
struct rt_metrics
{
    u_int32_t       rmx_locks;      /* Kernel leaves these values alone */
    u_int32_t       rmx_mtu;        /* MTU for this path */
    u_int32_t       rmx_hopcount;   /* max hops expected */
    int32_t         rmx_expire;     /* lifetime for route, e.g. redirect */
    u_int32_t       rmx_recvpipe;   /* inbound delay-bandwidth product */
    u_int32_t       rmx_sendpipe;   /* outbound delay-bandwidth product */
    u_int32_t       rmx_ssthresh;   /* outbound gateway buffer limit */
    u_int32_t       rmx_rtt;        /* estimated round trip time */
    u_int32_t       rmx_rttvar;     /* estimated rtt variance */
    u_int32_t       rmx_pksent;     /* packets sent using this route */
    u_int32_t       rmx_state;      /* route state */
    u_int32_t       rmx_filler[3];  /* will be used for T/TCP later */
};

/*
 * Structures for routing messages.
 */
struct rt_msghdr
{
    u_short rtm_msglen;     /* to skip over non-understood messages */
    u_char  rtm_version;    /* future binary compatibility */
    u_char  rtm_type;       /* message type */
    u_short rtm_index;      /* index for associated ifp */
    int     rtm_flags;      /* flags, incl. kern & message, e.g. DONE */
    int     rtm_addrs;      /* bitmask identifying sockaddrs in msg */
    pid_t   rtm_pid;        /* identify sender */
    int     rtm_seq;        /* for sender to identify action */
    int     rtm_errno;      /* why failed */
    int     rtm_use;        /* from rtentry */
    u_int32_t rtm_inits;    /* which metrics we are initializing */
    struct rt_metrics rtm_rmx; /* metrics themselves */
};

#define RTA_GATEWAY     0x2     /* gateway sockaddr present */
#define RTM_VERSION     5       /* Up the ante and ignore older versions */
#endif

#include <netinet/in.h>
#include <sys/param.h>
#include <sys/sysctl.h>
#include <stdlib.h>
#include <string.h>


static in_addr_t gateway_from_rtm(struct rt_msghdr *rtm)
{
    struct sockaddr *gw;
    unsigned int l;

    struct sockaddr *addr = (struct sockaddr *)(rtm + 1);
    l = roundup(addr->sa_len, sizeof(long));
    gw = (struct sockaddr *)((char *)addr + l);

    if (rtm->rtm_addrs & RTA_GATEWAY)
    {
        if (gw->sa_family == AF_INET)
        {
            struct sockaddr_in *sockin = (struct sockaddr_in *)gw;
            return (sockin->sin_addr.s_addr);
        }
    }

    return 0;
}

#endif // IL2CPP_TARGET_OSX || IL2CPP_TARGET_IOS

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
namespace NetworkInformation
{
    bool MacOsIPInterfaceProperties::ParseRouteInfo_icall(Il2CppString* iface, Il2CppArray** gw_addr_list)
    {
#if IL2CPP_TARGET_OSX || IL2CPP_TARGET_IOS
        size_t needed;
        in_addr_t in;
        int mib[6];
        int num_gws = 0;
        int gwnum = 0;
        unsigned int ifindex = 0;
        char *buf;
        char *next;
        char *lim;
        struct rt_msghdr *rtm;

        std::string ifacename = il2cpp::utils::StringUtils::Utf16ToUtf8(utils::StringUtils::GetChars(iface));

        if ((ifindex = if_nametoindex(ifacename.c_str())) == 0)
            return false;

        // MIB array defining data to read from sysctl
        mib[0] = CTL_NET; // Networking
        mib[1] = PF_ROUTE; // Routing messages
        mib[2] = 0; // Protocol number (always zero)
        mib[3] = AF_INET; // Address family (IPv4)
        mib[4] = NET_RT_DUMP; // Dump routing table
        mib[5] = 0; //

        // First sysctl call with oldp set to NULL to determine size of available data
        if (sysctl(mib, 6, NULL, &needed, NULL, 0) < 0)
            return false;

        // Allocate suffcient memory for available data based on the previous sysctl call
        if ((buf = (char*)IL2CPP_MALLOC(needed)) == NULL)
            return false;

        // Second sysctl call to retrieve data into appropriately sized buffer
        if (sysctl(mib, 6, buf, &needed, NULL, 0) < 0)
        {
            IL2CPP_FREE(buf);
            return false;
        }


        lim = buf + needed;
        for (next = buf; next < lim; next += rtm->rtm_msglen)
        {
            rtm = (struct rt_msghdr *)next;
            if (rtm->rtm_version != RTM_VERSION)
                continue;
            if (rtm->rtm_index != ifindex)
                continue;
            if ((in = gateway_from_rtm(rtm)) == 0)
                continue;
            num_gws++;
        }

        *gw_addr_list = (Il2CppArray*)vm::Array::New(il2cpp_defaults.string_class, num_gws);

        for (next = buf; next < lim; next += rtm->rtm_msglen)
        {
            rtm = (struct rt_msghdr *)next;
            if (rtm->rtm_version != RTM_VERSION)
                continue;
            if (rtm->rtm_index != ifindex)
                continue;
            if ((in = gateway_from_rtm(rtm)) == 0)
                continue;

            char addr[16], *ptr;
            int len;

            ptr = (char *)&in;
            len = snprintf(addr, sizeof(addr), "%u.%u.%u.%u",
                (unsigned char)ptr[0],
                (unsigned char)ptr[1],
                (unsigned char)ptr[2],
                (unsigned char)ptr[3]);

            if ((len >= sizeof(addr)) || (len < 0))
                continue;

            il2cpp_array_setref(*gw_addr_list, gwnum, il2cpp::vm::String::New(addr));
            gwnum++;
        }

        IL2CPP_FREE(buf);

        return true;
#else
        IL2CPP_NOT_IMPLEMENTED_ICALL(MacOsIPInterfaceProperties::ParseRouteInfo_icall);
        IL2CPP_UNREACHABLE;
        return false;
#endif
    }
} // namespace NetworkInformation
} // namespace Net
} // namespace System
} // namespace System
} // namespace icalls
} // namespace il2cpp
