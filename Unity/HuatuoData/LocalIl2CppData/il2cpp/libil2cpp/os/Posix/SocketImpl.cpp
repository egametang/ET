#include "il2cpp-config.h"

#if !IL2CPP_USE_GENERIC_SOCKET_IMPL && (IL2CPP_TARGET_POSIX || IL2CPP_SUPPORT_SOCKETS_POSIX_API) && IL2CPP_SUPPORT_SOCKETS

// enable support for AF_UNIX and socket paths
#define SUPPORT_UNIXSOCKETS (1)

// some platforms require a different function to close sockets
#define SOCK_CLOSE close

// allow option include file to configure platform
#if IL2CPP_USE_POSIX_SOCKET_PLATFORM_CONFIG
#include "SocketImplPlatformConfig.h"
#endif

#include <string.h>
#include <unistd.h>
#include <sys/socket.h>
#include <netinet/in.h>
#include <netinet/tcp.h>
#include <sys/ioctl.h>
#include <net/if.h>
#include <netdb.h>
#include <arpa/inet.h>
#include <errno.h>
#include <fcntl.h>
 #if SUPPORT_UNIXSOCKETS
#include <sys/un.h>
#endif
#include <sys/poll.h>
#include <sys/stat.h>

#if IL2CPP_TARGET_LINUX || IL2CPP_TARGET_ANDROID || IL2CPP_TARGET_LUMIN || IL2CPP_TARGET_JAVASCRIPT
#include <sys/sendfile.h>
#endif

#include "os/Error.h"
#include "os/Socket.h"
#include "os/ErrorCodes.h"
#include "os/Posix/Error.h"
#include "os/Posix/PosixHelpers.h"
#include "os/Posix/SocketImpl.h"
#include "os/Posix/ThreadImpl.h"
#include "utils/Memory.h"
#include "utils/Il2CppError.h"
#include "utils/StringUtils.h"

namespace il2cpp
{
namespace os
{
    static bool is_loopback(int32_t family, uint8_t *addr)
    {
        if (family == AF_INET)
            return addr[0] == 127;
#if IL2CPP_SUPPORT_IPV6
        else if (family == AF_INET6)
            return (IN6_IS_ADDR_LOOPBACK((struct in6_addr *)addr));
#endif
        return false;
    }

    static bool is_loopback(const char* address)
    {
        if (strcmp(address, "localhost") == 0)
        {
            return true;
        }

        {
            sockaddr_in sin = {0};
            if (inet_pton(AF_INET, address, &sin.sin_addr) > 0)
            {
                return is_loopback(AF_INET, (uint8_t*)&sin.sin_addr);
            }
        }
#if IL2CPP_SUPPORT_IPV6
        {
            sockaddr_in6 sin = {0};
            if (inet_pton(AF_INET6, address, &sin.sin6_addr) > 0)
            {
                return is_loopback(AF_INET6, (uint8_t*)&sin.sin6_addr);
            }
        }
#endif
        return false;
    }

    static bool is_limited_broadcast(const struct sockaddr *sa, socklen_t sa_size)
    {
        if (sa != NULL && sa_size >= sizeof(sockaddr_in))
        {
            const sockaddr_in *sin = (const sockaddr_in *)sa;
            return sin->sin_family == AF_INET && sin->sin_addr.s_addr == htonl(INADDR_BROADCAST);
        }

        return false;
    }

    bool SocketImpl::is_private(const struct sockaddr *sa, socklen_t sa_size)
    {
        if (is_limited_broadcast(sa, sa_size))
        {
            return true;
        }

        if (sa != NULL)
        {
            if (sa_size >= sizeof(const sockaddr_in) && sa->sa_family == AF_INET)
            {
                const uint8_t *addr = (uint8_t *)&(((const sockaddr_in *)sa)->sin_addr);
                if (addr[0] == 10) // Class A
                {
                    return true;
                }
                if (addr[0] == 172 && (addr[1] & 0xf0) == 16) // Class B
                {
                    return true;
                }
                if (addr[0] == 192 && addr[1] == 168) // Class C
                {
                    return true;
                }
            }
#if IL2CPP_SUPPORT_IPV6
            else if (sa_size >= sizeof(struct sockaddr_in6) && sa->sa_family == AF_INET6)
            {
                const uint8_t *addr = (uint8_t *)&(((const sockaddr_in6 *)sa)->sin6_addr);
                if (addr[0] == 0xf && (addr[1] & 0xe) == 0xc) // Unique local unicast address(ULA)
                {
                    return true;
                }
                if (addr[0] == 0xf && addr[1] == 0xe && (addr[2] & 0xc) == 0x8) // Link local unicast address
                {
                    return true;
                }
            }
#endif
        }

        return false;
    }

    bool SocketImpl::is_private(const char* address)
    {
        if (address == 0 || address[0] == 0)
        {
            return false;
        }

        {
            sockaddr_in sin = {0};
            if (inet_pton(AF_INET, address, &sin.sin_addr) > 0)
            {
                sin.sin_family = AF_INET;
                return is_private((const sockaddr *)&sin, sizeof(sin));
            }
        }
#if IL2CPP_SUPPORT_IPV6
        {
            sockaddr_in6 sin = {0};
            if (inet_pton(AF_INET6, address, &sin.sin6_addr) > 0)
            {
                sin.sin6_family = AF_INET6;
                return is_private((const sockaddr *)&sin, sizeof(sin));
            }
        }
#endif
        return false;
    }

#define IFCONF_BUFF_SIZE 1024
#ifndef _SIZEOF_ADDR_IFREQ
#define _SIZEOF_ADDR_IFREQ(ifr) (sizeof (struct ifreq))
#endif

#define FOREACH_IFR(IFR, IFC) \
    for (IFR = (IFC).ifc_req;   \
    ifr < (struct ifreq*)((char*)(IFC).ifc_req + (IFC).ifc_len); \
    ifr = (struct ifreq*)((char*)(IFR) + _SIZEOF_ADDR_IFREQ (*(IFR))))

    static int address_size_for_family(int family)
    {
        switch (family)
        {
            case AF_INET:
                return sizeof(struct in_addr);
#if IL2CPP_SUPPORT_IPV6
            case AF_INET6:
                return sizeof(struct in6_addr);
#endif
        }
        return 0;
    }

    static void*
    get_address_from_sockaddr(struct sockaddr *sa)
    {
        switch (sa->sa_family)
        {
            case AF_INET:
                return &((struct sockaddr_in*)sa)->sin_addr;
#if IL2CPP_SUPPORT_IPV6
            case AF_INET6:
                return &((struct sockaddr_in6*)sa)->sin6_addr;
#endif
        }
        return NULL;
    }

    static struct in_addr *get_local_ips(int32_t family, int32_t *interface_count)
    {
        int fd;
        struct ifconf ifc;
        struct ifreq *ifr;
        int if_count = 0;
        bool ignore_loopback = false;
        void *result = NULL;
        char *result_ptr;

        *interface_count = 0;

        if (!address_size_for_family(family))
            return NULL;

        fd = socket(family, SOCK_STREAM, 0);
        if (fd == -1)
            return NULL;

        memset(&ifc, 0, sizeof(ifc));
        ifc.ifc_len = IFCONF_BUFF_SIZE;
        ifc.ifc_buf = (char*)malloc(IFCONF_BUFF_SIZE);   /* We can't have such huge buffers on the stack. */
        if (ioctl(fd, SIOCGIFCONF, &ifc) < 0)
            goto done;

        FOREACH_IFR(ifr, ifc) {
            struct ifreq iflags;

            //only return addresses of the same type as @family
            if (ifr->ifr_addr.sa_family != family)
            {
                ifr->ifr_name[0] = '\0';
                continue;
            }

            strcpy(iflags.ifr_name, ifr->ifr_name);

            //ignore interfaces we can't get props for
            if (ioctl(fd, SIOCGIFFLAGS, &iflags) < 0)
            {
                ifr->ifr_name[0] = '\0';
                continue;
            }

            //ignore interfaces that are down
            if ((iflags.ifr_flags & IFF_UP) == 0)
            {
                ifr->ifr_name[0] = '\0';
                continue;
            }

            //If we have a non-loopback iface, don't return any loopback
            if ((iflags.ifr_flags & IFF_LOOPBACK) == 0)
            {
                ignore_loopback = true;
                ifr->ifr_name[0] = 1; //1 means non-loopback
            }
            else
            {
                ifr->ifr_name[0] = 2;  //2 means loopback
            }
            ++if_count;
        }

        result = (char*)malloc(if_count * address_size_for_family(family));
        result_ptr = (char*)result;
        FOREACH_IFR(ifr, ifc) {
            if (ifr->ifr_name[0] == '\0')
                continue;

            if (ignore_loopback && ifr->ifr_name[0] == 2)
            {
                --if_count;
                continue;
            }

            memcpy(result_ptr, get_address_from_sockaddr(&ifr->ifr_addr), address_size_for_family(family));
            result_ptr += address_size_for_family(family);
        }
        IL2CPP_ASSERT(result_ptr <= (char*)result + if_count * address_size_for_family(family));

    done:
        *interface_count = if_count;
        free(ifc.ifc_buf);
        close(fd);
        return (struct in_addr*)result;
    }

    static bool hostent_get_info(struct hostent *he, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        if (he == NULL)
            return false;

        if (he->h_length != 4 || he->h_addrtype != AF_INET)
            return false;

        name.assign(he->h_name);

        for (int32_t i = 0; he->h_aliases[i] != NULL; ++i)
            aliases.push_back(he->h_aliases[i]);

        for (int32_t i = 0; he->h_addr_list[i] != NULL; ++i)
            addr_list.push_back(
                utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                    (uint8_t)he->h_addr_list[i][0],
                    (uint8_t)he->h_addr_list[i][1],
                    (uint8_t)he->h_addr_list[i][2],
                    (uint8_t)he->h_addr_list[i][3]));

        return true;
    }

    static bool hostent_get_info_with_local_ips(struct hostent *he, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        int32_t nlocal_in = 0;

        if (he != NULL)
        {
            if (he->h_length != 4 || he->h_addrtype != AF_INET)
                return false;

            name.assign(he->h_name);

            for (int32_t i = 0; he->h_aliases[i] != NULL; ++i)
                aliases.push_back(he->h_aliases[i]);
        }

        struct in_addr *local_in = get_local_ips(AF_INET, &nlocal_in);

        if (nlocal_in)
        {
            for (int32_t i = 0; i < nlocal_in; ++i)
            {
                const uint8_t *ptr = (uint8_t*)&local_in[i];

                addr_list.push_back(
                    utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                        (uint8_t)ptr[0],
                        (uint8_t)ptr[1],
                        (uint8_t)ptr[2],
                        (uint8_t)ptr[3]));
            }

            free(local_in);
        }
        else if (he == NULL)
        {
            // If requesting "" and there are no other interfaces up, MS returns 127.0.0.1
            addr_list.push_back("127.0.0.1");
            return true;
        }

        if (nlocal_in == 0 && he != NULL)
        {
            for (int32_t i = 0; he->h_addr_list[i] != NULL; ++i)
            {
                addr_list.push_back(
                    utils::StringUtils::NPrintf("%u.%u.%u.%u", 16,
                        (uint8_t)he->h_addr_list[i][0],
                        (uint8_t)he->h_addr_list[i][1],
                        (uint8_t)he->h_addr_list[i][2],
                        (uint8_t)he->h_addr_list[i][3]));
            }
        }

        return true;
    }

    static int32_t convert_socket_flags(os::SocketFlags flags)
    {
        int32_t c_flags = 0;

        if (flags)
        {
            // Check if contains invalid flag values
            if (flags & ~(os::kSocketFlagsOutOfBand | os::kSocketFlagsMaxIOVectorLength | os::kSocketFlagsPeek | os::kSocketFlagsDontRoute | os::kSocketFlagsPartial))
            {
                return -1;
            }
    #ifdef MSG_OOB
            if (flags & os::kSocketFlagsOutOfBand)
                c_flags |= MSG_OOB;
    #endif
            if (flags & os::kSocketFlagsPeek)
                c_flags |= MSG_PEEK;

            if (flags & os::kSocketFlagsDontRoute)
                c_flags |= MSG_DONTROUTE;

            // Ignore Partial - see bug 349688.  Don't return -1, because
            // according to the comment in that bug ms runtime doesn't for
            // UDP sockets (this means we will silently ignore it for TCP
            // too)

    #ifdef MSG_MORE
            if (flags & os::kSocketFlagsPartial)
                c_flags |= MSG_MORE;
    #endif
        }

        return c_flags;
    }

    void SocketImpl::Startup()
    {
    }

    void SocketImpl::Cleanup()
    {
    }

#if IL2CPP_SUPPORT_IPV6
    static void AddrinfoGetAddresses(struct addrinfo *info, std::string& name, bool add_local_ips,
        std::vector<std::string> &addr_list)
    {
        if (add_local_ips)
        {
            bool any_local_ips_added = false;
            int nlocal_in = 0;
            int nlocal_in6 = 0;
            in_addr* local_in = (struct in_addr *)get_local_ips(AF_INET, &nlocal_in);
            in6_addr* local_in6 = (struct in6_addr *)get_local_ips(AF_INET6, &nlocal_in6);
            if (nlocal_in || nlocal_in6)
            {
                if (nlocal_in)
                {
                    for (int i = 0; i < nlocal_in; i++)
                    {
                        char addr[16];
                        inet_ntop(AF_INET, &local_in[i], addr, sizeof(addr));
                        addr_list.push_back(std::string(addr));
                        any_local_ips_added = true;
                    }
                }

                if (nlocal_in6)
                {
                    for (int i = 0; i < nlocal_in6; i++)
                    {
                        char addr[48];
                        const char* ret = inet_ntop(AF_INET6, &local_in6[i], addr, sizeof(addr));
                        if (ret != NULL)
                        {
                            addr_list.push_back(std::string(addr));
                            any_local_ips_added = true;
                        }
                    }
                }
            }

            free(local_in);
            free(local_in6);

            if (any_local_ips_added)
                return;
        }

        bool nameSet = false;
        for (addrinfo* ai = info; ai != NULL; ai = ai->ai_next)
        {
            const char *ret;
            char buffer[48]; /* Max. size for IPv6 */

            if ((ai->ai_family != PF_INET) && (ai->ai_family != PF_INET6))
                continue;

            if (ai->ai_family == PF_INET)
                ret = inet_ntop(ai->ai_family, (void*)&(((struct sockaddr_in*)ai->ai_addr)->sin_addr), buffer, 16);
            else
                ret = inet_ntop(ai->ai_family, (void*)&(((struct sockaddr_in6*)ai->ai_addr)->sin6_addr), buffer, 48);

            if (ret)
                addr_list.push_back(std::string(buffer));
            else
                addr_list.push_back(std::string());

            if (!nameSet)
            {
                if (ai->ai_canonname != NULL)
                    name = std::string(ai->ai_canonname);
                else
                    name = std::string();

                nameSet = true;
            }
        }
    }

    WaitStatus GetAddressInfo(const char* hostname, bool add_local_ips, std::string &name, std::vector<std::string> &addr_list)
    {
        NetworkAccessHandler::Auto scopedAccess;
        if (!is_loopback(hostname) || add_local_ips)
        {
            if (!scopedAccess.RequestAccessForAddressInfo(SocketImpl::is_private(hostname)))
            {
                return kWaitStatusFailure;
            }
        }

        addrinfo *info = NULL;

        addrinfo hints;
        memset(&hints, 0, sizeof(hints));

        // Here Mono inspects the ipv4Supported and ipv6Supported properties on the managed Socket class.
        // This seems to be unnecessary though, as we can use PF_UNSPEC in all cases, and getaddrinfo works.
        hints.ai_family = PF_UNSPEC;
        hints.ai_socktype = SOCK_STREAM;
        hints.ai_flags = AI_CANONNAME | AI_ADDRCONFIG;

        if (*hostname && getaddrinfo(hostname, NULL, &hints, &info) == -1)
            return kWaitStatusFailure;

        AddrinfoGetAddresses(info, name, add_local_ips, addr_list);

        if (info)
            freeaddrinfo(info);

        if (name.empty())
            name.assign(hostname);

        return kWaitStatusSuccess;
    }

#endif

    WaitStatus SocketImpl::GetHostByAddr(const std::string &address, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addr_list)
    {
        NetworkAccessHandler::Auto scopedAccess;
        if (!is_loopback(address.c_str()))
        {
            if (!scopedAccess.RequestAccessForAddressInfo(is_private(address.c_str())))
            {
                return kWaitStatusFailure;
            }
        }

#if IL2CPP_SUPPORT_IPV6
        struct sockaddr_in saddr;
        struct sockaddr_in6 saddr6;
        int32_t family;
        char hostname[1024] = {0};
        int flags = 0;

        if (inet_pton(AF_INET, address.c_str(), &saddr.sin_addr) <= 0)
        {
            /* Maybe an ipv6 address */
            if (inet_pton(AF_INET6, address.c_str(), &saddr6.sin6_addr) <= 0)
            {
                return kWaitStatusFailure;
            }
            else
            {
                family = AF_INET6;
                saddr6.sin6_family = AF_INET6;
            }
        }
        else
        {
            family = AF_INET;
            saddr.sin_family = AF_INET;
        }

        if (family == AF_INET)
        {
#if HAVE_SOCKADDR_IN_SIN_LEN
            saddr.sin_len = sizeof(saddr);
#endif
            if (getnameinfo((struct sockaddr*)&saddr, sizeof(saddr),
                hostname, sizeof(hostname), NULL, 0,
                flags) != 0)
            {
                return kWaitStatusFailure;
            }
        }
        else if (family == AF_INET6)
        {
#if HAVE_SOCKADDR_IN6_SIN_LEN
            saddr6.sin6_len = sizeof(saddr6);
#endif
            if (getnameinfo((struct sockaddr*)&saddr6, sizeof(saddr6),
                hostname, sizeof(hostname), NULL, 0,
                flags) != 0)
            {
                return kWaitStatusFailure;
            }
        }

        return GetAddressInfo(hostname, false, name, addr_list);
#else
        struct in_addr inaddr;
        if (inet_pton(AF_INET, address.c_str(), &inaddr) <= 0)
            return kWaitStatusFailure;

        struct hostent *he = gethostbyaddr((char*)&inaddr, sizeof(inaddr), AF_INET);

        if (he == NULL)
        {
            name = address;
            addr_list.push_back(name);

            return kWaitStatusSuccess;
        }

        return hostent_get_info(he, name, aliases, addr_list)
            ? kWaitStatusSuccess
            : kWaitStatusFailure;
#endif
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, std::vector<std::string> &aliases, std::vector<std::string> &addresses)
    {
        char this_hostname[256] = {0};

        const char *hostname = host.c_str();
        bool add_local_ips = (*hostname == '\0');

        if (!add_local_ips && gethostname(this_hostname, sizeof(this_hostname)) != -1)
        {
            if (!strcmp(hostname, this_hostname))
                add_local_ips = true;
        }

#if IL2CPP_SUPPORT_IPV6
        return GetAddressInfo(hostname, add_local_ips, name, addresses);
#else
        struct hostent *he = NULL;
        if (*hostname)
            he = gethostbyname(hostname);

        if (*hostname && he == NULL)
            return kWaitStatusFailure;

        return (add_local_ips
            ? hostent_get_info_with_local_ips(he, name, aliases, addresses)
            : hostent_get_info(he, name, aliases, addresses))
            ? kWaitStatusSuccess
            : kWaitStatusFailure;
#endif
    }

    static bool HasAnyIPv4Addresses(const std::vector<std::string>& addresses)
    {
        for (std::vector<std::string>::const_iterator it = addresses.begin(); it != addresses.end(); ++it)
        {
            in_addr address;
            if (inet_pton(AF_INET, it->c_str(), &address))
                return true;
        }

        return false;
    }

    WaitStatus SocketImpl::GetHostByName(const std::string &host, std::string &name, int32_t &family, std::vector<std::string> &aliases, std::vector<void*> &addr_list, int32_t &addr_size)
    {
        std::vector<std::string> addresses;
        WaitStatus result = GetHostByName(host, name, aliases, addresses);

        // If we got an IPv4 address, use that and any others, skipping IPv6 addresses.
        // We can only return one address size, so we need to choose.
        if (HasAnyIPv4Addresses(addresses))
        {
            addr_size = sizeof(in_addr);
            family = AF_INET;
            for (std::vector<std::string>::iterator it = addresses.begin(); it != addresses.end(); ++it)
            {
                in_addr address;
                if (inet_pton(family, it->c_str(), &address))
                {
                    void* addressLocation = il2cpp::utils::Memory::Malloc(addr_size);
                    memcpy(addressLocation, &address.s_addr, addr_size);
                    addr_list.push_back(addressLocation);
                }
            }
        }
#if IL2CPP_SUPPORT_IPV6
        else
        {
            addr_size = sizeof(in6_addr);
            family = AF_INET6;
            for (std::vector<std::string>::iterator it = addresses.begin(); it != addresses.end(); ++it)
            {
                in6_addr address;
                if (inet_pton(family, it->c_str(), &address))
                {
                    void* addressLocation = il2cpp::utils::Memory::Malloc(addr_size);
                    memcpy(addressLocation, &address.s6_addr, addr_size);
                    addr_list.push_back(addressLocation);
                }
            }
        }
#endif
        return result;
    }

    WaitStatus SocketImpl::GetHostName(std::string &name)
    {
        char hostname[256];
        int32_t ret = gethostname(hostname, sizeof(hostname));

        if (ret == -1)
            return kWaitStatusFailure;

        name.assign(hostname);

        return kWaitStatusSuccess;
    }

    SocketImpl::SocketImpl(ThreadStatusCallback thread_status_callback)
        :   _is_valid(false)
        ,   _fd(-1)
        ,   _domain(-1)
        ,   _type(-1)
        ,   _protocol(-1)
        ,   _saved_error(kErrorCodeSuccess)
        ,   _still_readable(0)
        ,   _thread_status_callback(thread_status_callback)
    {
    }

    SocketImpl::~SocketImpl()
    {
    }

    static int32_t convert_address_family(AddressFamily family)
    {
        switch (family)
        {
            case kAddressFamilyUnspecified:
                return AF_UNSPEC;

            case kAddressFamilyUnix:
                return AF_UNIX;

            case kAddressFamilyInterNetwork:
                return AF_INET;
#ifdef AF_IPX
            case kAddressFamilyIpx:
                return AF_IPX;
#endif
#ifdef AF_SNA
            case kAddressFamilySna:
                return AF_SNA;
#endif
#ifdef AF_DECnet
            case kAddressFamilyDecNet:
                return AF_DECnet;
#endif
#ifdef AF_APPLETALK
            case kAddressFamilyAppleTalk:
                return AF_APPLETALK;
#endif
#ifdef AF_INET6
            case kAddressFamilyInterNetworkV6:
                return AF_INET6;
#endif
#ifdef AF_IRDA
            case kAddressFamilyIrda:
                return AF_IRDA;
#endif

            default:
                break;
        }

        return -1;
    }

    static AddressFamily convert_define_to_address_family(int32_t family)
    {
        switch (family)
        {
            case AF_UNSPEC:
                return kAddressFamilyUnspecified;

            case AF_UNIX:
                return kAddressFamilyUnix;

            case AF_INET:
                return kAddressFamilyInterNetwork;
#ifdef AF_IPX
            case AF_IPX:
                return kAddressFamilyIpx;
#endif
#ifdef AF_SNA
            case AF_SNA:
                return kAddressFamilySna;
#endif
#ifdef AF_DECnet
            case AF_DECnet:
                return kAddressFamilyDecNet;
#endif
#ifdef AF_APPLETALK
            case AF_APPLETALK:
                return kAddressFamilyAppleTalk;
#endif
#ifdef AF_INET6
            case AF_INET6:
                return kAddressFamilyInterNetworkV6;
#endif
#ifdef AF_IRDA
            case AF_IRDA:
                return kAddressFamilyIrda;
#endif

            default:
                break;
        }

        return kAddressFamilyError;
    }

    static int32_t convert_socket_type(SocketType type)
    {
        switch (type)
        {
            case kSocketTypeStream:
                return SOCK_STREAM;

            case kSocketTypeDgram:
                return SOCK_DGRAM;

            case kSocketTypeRaw:
                return SOCK_RAW;
#ifdef SOCK_RDM
            case kSocketTypeRdm:
                return SOCK_RDM;
#endif
#ifdef SOCK_SEQPACKET
            case kSocketTypeSeqpacket:
                return SOCK_SEQPACKET;
#endif
            default:
                break;
        }

        return -1;
    }

    static int32_t convert_socket_protocol(ProtocolType protocol)
    {
        switch (protocol)
        {
            case kProtocolTypeIP:
            case kProtocolTypeIPv6:
            case kProtocolTypeIcmp:
            case kProtocolTypeIgmp:
            case kProtocolTypeGgp:
            case kProtocolTypeTcp:
            case kProtocolTypePup:
            case kProtocolTypeUdp:
            case kProtocolTypeIdp:
                // In this case the enum values map exactly.
                return (int32_t)protocol;

            default:
                break;
        }

        // Everything else in unsupported and unexpected
        return -1;
    }

    WaitStatus SocketImpl::Create(AddressFamily family, SocketType type, ProtocolType protocol)
    {
        _fd = -1;
        _is_valid = false;
        _still_readable = 1;
        _domain = convert_address_family(family);
        _type = convert_socket_type(type);
        _protocol = convert_socket_protocol(protocol);

        IL2CPP_ASSERT(_type != -1 && "Unsupported socket type");
        IL2CPP_ASSERT(_domain != -1 && "Unsupported address family");
        IL2CPP_ASSERT(_protocol != -1 && "Unsupported protocol type");

        _fd = socket(_domain, _type, _protocol);
        if (_fd == -1 && _domain == AF_INET && _type == SOCK_RAW && _protocol == 0)
        {
            // Retry with protocol == 4 (see bug #54565)
            _protocol = 4;
            _fd = socket(AF_INET, SOCK_RAW, 4);
        }

        if (_fd == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        // if (fd >= _wapi_fd_reserve)
        // {
        //  WSASetLastError (WSASYSCALLFAILURE);
        //  close (fd);

        //  return(INVALID_SOCKET);
        // }

        /* .net seems to set this by default for SOCK_STREAM, not for
         * SOCK_DGRAM (see bug #36322)
         *
         * It seems winsock has a rather different idea of what
         * SO_REUSEADDR means.  If it's set, then a new socket can be
         * bound over an existing listening socket.  There's a new
         * windows-specific option called SO_EXCLUSIVEADDRUSE but
         * using that means the socket MUST be closed properly, or a
         * denial of service can occur.  Luckily for us, winsock
         * behaves as though any other system would when SO_REUSEADDR
         * is true, so we don't need to do anything else here.  See
         * bug 53992.
         */
        {
            int32_t v = 1;
            const int32_t ret = setsockopt(_fd, SOL_SOCKET, SO_REUSEADDR, &v, sizeof(v));

            if (ret == -1)
            {
                if (SOCK_CLOSE(_fd) == -1)
                    StoreLastError();

                return kWaitStatusFailure;
            }
        }

#if IL2CPP_TARGET_DARWIN
        int32_t value = 1;
        setsockopt(_fd, SOL_SOCKET, SO_NOSIGPIPE, &value, sizeof(value));
#endif

        // mono_once (&socket_ops_once, socket_ops_init);

        // handle = _wapi_handle_new_fd (WAPI_HANDLE_SOCKET, fd, &socket_handle);
        // if (handle == _WAPI_HANDLE_INVALID) {
        //  g_warning ("%s: error creating socket handle", __func__);
        //  WSASetLastError (WSASYSCALLFAILURE);
        //  close (fd);

        //  return(INVALID_SOCKET);
        // }

        _is_valid = true;

        _networkAccess.InheritNetworkAccessState(_fd);

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Create(SocketDescriptor fd, int32_t family, int32_t type, int32_t protocol)
    {
        _fd = fd;
        _is_valid = (fd != -1);
        _still_readable = 1;
        _domain = family;
        _type = type;
        _protocol = protocol;

        _networkAccess.InheritNetworkAccessState(_fd);

        IL2CPP_ASSERT(_type != -1 && "Unsupported socket type");
        IL2CPP_ASSERT(_domain != -1 && "Unsupported address family");
        IL2CPP_ASSERT(_protocol != -1 && "Unsupported protocol type");

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Close()
    {
        _saved_error = kErrorCodeSuccess;

        if (_is_valid && _fd != -1)
        {
            if (SOCK_CLOSE(_fd) == -1)
                StoreLastError();
        }

        _fd = -1;
        _is_valid = false;
        _still_readable = 0;
        _domain = -1;
        _type = -1;
        _protocol = -1;

        _networkAccess.CancelNetworkAccess();

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SetBlocking(bool blocking)
    {
#if IL2CPP_USE_SOCKET_SETBLOCKING
        return (WaitStatus)setBlocking(_fd, blocking);
#else

        int32_t flags = fcntl(_fd, F_GETFL, 0);
        if (flags == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        flags = blocking
            ? (flags & ~O_NONBLOCK)
            : (flags | O_NONBLOCK);

        if (fcntl(_fd, F_SETFL, flags) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }
        return kWaitStatusSuccess;
#endif
    }

    ErrorCode SocketImpl::GetLastError() const
    {
        return _saved_error;
    }

    void SocketImpl::StoreLastError()
    {
        const ErrorCode error = SocketErrnoToErrorCode(errno);

        Error::SetLastError(error);

        _saved_error = error;
    }

    void SocketImpl::StoreLastError(int32_t error_no)
    {
        const ErrorCode error = SocketErrnoToErrorCode(error_no);

        Error::SetLastError(error);

        _saved_error = error;
    }

#if SUPPORT_UNIXSOCKETS
    static struct sockaddr* sockaddr_from_path(const char *path, socklen_t *sa_size)
    {
        struct sockaddr_un* sa_un;
        const size_t len = strlen(path);

        if (len >= sizeof(sa_un->sun_path))
            return NULL;

        sa_un = (struct sockaddr_un*)IL2CPP_CALLOC(1, sizeof(sockaddr_un));

        sa_un->sun_family = AF_UNIX;
        memcpy(sa_un->sun_path, path, len);

        *sa_size = sizeof(sockaddr_un);
        return (struct sockaddr *)sa_un;
    }

#endif

#if IL2CPP_SUPPORT_IPV6
    static void sockaddr_from_address(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, sockaddr_in6* sa, socklen_t *sa_size)
    {
        sa->sin6_family = AF_INET6;
        sa->sin6_port = port;
        memcpy(&sa->sin6_addr, &address[0], 16);
        sa->sin6_scope_id = scope;

        *sa_size = sizeof(struct sockaddr_in6);
    }

#endif

    static void sockaddr_from_address(uint32_t address, uint16_t port, struct sockaddr *sa, socklen_t *sa_size)
    {
        struct sockaddr_in sa_in = {0};

        sa_in.sin_family = AF_INET;
        sa_in.sin_port = port;
        sa_in.sin_addr.s_addr = address;

        *sa_size = sizeof(struct sockaddr_in);
        *sa = *((struct sockaddr*)&sa_in);
    }

    static bool socketaddr_to_endpoint_info(const struct sockaddr *address, socklen_t address_len, EndPointInfo &info)
    {
        info.family = convert_define_to_address_family(address->sa_family);

        if (info.family == os::kAddressFamilyInterNetwork)
        {
            const struct sockaddr_in *address_in = (const struct sockaddr_in *)address;

            info.data.inet.port = ntohs(address_in->sin_port);
            info.data.inet.address = ntohl(address_in->sin_addr.s_addr);

            return true;
        }

        if (info.family == os::kAddressFamilyUnix)
        {
            for (int32_t i = 0; i < address_len; i++)
                info.data.path[i] = address->sa_data[i];

            return true;
        }

#if IL2CPP_SUPPORT_IPV6
        if (info.family == os::kAddressFamilyInterNetworkV6)
        {
            const struct sockaddr_in6 *address_in = (const struct sockaddr_in6 *)address;

            uint16_t port = ntohs(address_in->sin6_port);

            info.data.raw[2] = (port >> 8) & 0xff;
            info.data.raw[3] = port & 0xff;

            for (int i = 0; i < 16; i++)
                info.data.raw[i + 8] = address_in->sin6_addr.s6_addr[i];

            info.data.raw[24] = address_in->sin6_scope_id & 0xff;
            info.data.raw[25] = (address_in->sin6_scope_id >> 8) & 0xff;
            info.data.raw[26] = (address_in->sin6_scope_id >> 16) & 0xff;
            info.data.raw[27] = (address_in->sin6_scope_id >> 24) & 0xff;

            return true;
        }
#endif

        return false;
    }

    WaitStatus SocketImpl::Bind(const char *path)
    {
#if SUPPORT_UNIXSOCKETS
        socklen_t sa_size = 0;

        struct sockaddr* sa = sockaddr_from_path(path, &sa_size);

        int result = bind(_fd, sa, sa_size);

        IL2CPP_FREE(sa);

        if (result == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
#else
        return kWaitStatusFailure;
#endif
    }

    WaitStatus SocketImpl::Bind(const char *address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(inet_addr(address), htons(port), &sa, &sa_size);

        if (!_networkAccess.PrepareForBind(_fd, &sa, sa_size))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        if (bind(_fd, &sa, sa_size) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Bind(uint32_t address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        if (!_networkAccess.PrepareForBind(_fd, &sa, sa_size))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        if (bind(_fd, &sa, sa_size) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    utils::Expected<WaitStatus> SocketImpl::Bind(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
#if IL2CPP_SUPPORT_IPV6
        struct sockaddr_in6 sa = { 0 };
        socklen_t sa_size = 0;

        sockaddr_from_address(address, scope, htons(port), &sa, &sa_size);

        if (!_networkAccess.PrepareForBind(_fd, &sa, sa_size))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        if (bind(_fd, (sockaddr*)&sa, sa_size) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
#else
        return utils::Il2CppError(utils::NotSupported, "IPv6 is not supported on this platform.");
#endif
    }

    WaitStatus SocketImpl::ConnectInternal(struct sockaddr *sa, int32_t sa_size)
    {
        if (!_networkAccess.PrepareForConnect(_fd, sa, sa_size))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        if (connect(_fd, sa, (socklen_t)sa_size) != -1)
            return kWaitStatusSuccess;

        if (errno != EINTR)
        {
            // errnum = errno_to_WSA (errnum, __func__);
            // if (errnum == WSAEINPROGRESS)
            //  errnum = WSAEWOULDBLOCK; /* see bug #73053 */

            StoreLastError();

            return kWaitStatusFailure;
        }

        struct pollfd fds = {0};

        fds.fd = _fd;
        fds.events = POLLOUT;

        while (poll(&fds, 1, -1) == -1)
        {
            if (errno != EINTR)
            {
                StoreLastError();
                return kWaitStatusFailure;
            }
        }

        int32_t so_error = 0;
        socklen_t len = sizeof(so_error);

        if (getsockopt(_fd, SOL_SOCKET, SO_ERROR, &so_error, &len) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (so_error != 0)
        {
            StoreLastError(so_error);
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Connect(const char *path)
    {
#if SUPPORT_UNIXSOCKETS
        socklen_t sa_size = 0;

        struct sockaddr* sa = sockaddr_from_path(path, &sa_size);

        WaitStatus status = ConnectInternal(sa, sa_size);

        IL2CPP_FREE(sa);

        return status;
#else
        return kWaitStatusFailure;
#endif
    }

    utils::Expected<WaitStatus> SocketImpl::Connect(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port)
    {
#if IL2CPP_SUPPORT_IPV6
        struct sockaddr_in6 sa = { 0 };
        socklen_t sa_size = 0;

        sockaddr_from_address(address, scope, htons(port), &sa, &sa_size);

        return ConnectInternal((struct sockaddr *)&sa, sa_size);
#else
        return utils::Il2CppError(utils::NotSupported, "IPv6 is not supported on this platform.");
#endif
    }

    WaitStatus SocketImpl::Connect(uint32_t address, uint16_t port)
    {
        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        return ConnectInternal((struct sockaddr *)&sa, sa_size);
    }

    WaitStatus SocketImpl::GetLocalEndPointInfo(EndPointInfo &info)
    {
        // Note: the size here could probably be smaller
        uint8_t buffer[END_POINT_MAX_PATH_LEN + 3] = {0};
        socklen_t address_len = sizeof(buffer);

        if (getsockname(_fd, (struct sockaddr *)buffer, &address_len) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (!socketaddr_to_endpoint_info((struct sockaddr *)buffer, address_len, info))
        {
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::GetRemoteEndPointInfo(EndPointInfo &info)
    {
        // Note: the size here could probably be smaller
        uint8_t buffer[END_POINT_MAX_PATH_LEN + 3] = {0};
        socklen_t address_len = sizeof(buffer);

        if (getpeername(_fd, (struct sockaddr *)buffer, &address_len) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (!socketaddr_to_endpoint_info((struct sockaddr *)buffer, address_len, info))
        {
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Listen(int32_t backlog)
    {
        if (listen(_fd, backlog) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Shutdown(int32_t how)
    {
        if (shutdown(_fd, how) == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        if (how == SHUT_RD || how == SHUT_RDWR)
            _still_readable = 0;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Accept(os::Socket **socket)
    {
        int32_t new_fd = 0;

        *socket = NULL;

        do
        {
            new_fd = accept(_fd, NULL, 0);
        }
        while (new_fd == -1 && errno == EINTR);

        if (new_fd == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        *socket = new os::Socket(_thread_status_callback);

        const WaitStatus status = (*socket)->Create(new_fd, _domain, _type, _protocol);

        if (status != kWaitStatusSuccess)
        {
            delete *socket;
            *socket = NULL;
            return status;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Disconnect(bool reuse)
    {
        int32_t new_sock = socket(_domain, _type, _protocol);
        if (new_sock == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        // According to Stevens "Advanced Programming in the UNIX
        // Environment: UNIX File I/O" dup2() is atomic so there
        // should not be a race condition between the old fd being
        // closed and the new socket fd being copied over

        int32_t ret = 0;

        do
        {
            ret = dup2(new_sock, _fd);
        }
        while (ret == -1 && errno == EAGAIN);

        if (ret == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        SOCK_CLOSE(new_sock);

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Receive(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        return ReceiveFromInternal(data, count, c_flags, len, NULL, 0);
    }

    WaitStatus SocketImpl::ReceiveFromInternal(const uint8_t *data, size_t count, int32_t flags, int32_t *len, struct sockaddr *from, int32_t *fromlen)
    {
        int32_t ret = 0;

        if (!_networkAccess.WaitForNetworkStatus(_fd))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        do
        {
            ret = (int32_t)recvfrom(_fd, (void*)data, count, flags, from, (socklen_t*)fromlen);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == 0 && count > 0)
        {
            // According to the Linux man page, recvfrom only
            // returns 0 when the socket has been shut down
            // cleanly.  Turn this into an EINTR to simulate win32
            // behaviour of returning EINTR when a socket is
            // closed while the recvfrom is blocking (we use a
            // shutdown() in socket_close() to trigger this.) See
            // bug 75705.

            // Distinguish between the socket being shut down at
            // the local or remote ends, and reads that request 0
            // bytes to be read

            // If this returns FALSE, it means the socket has been
            // closed locally.  If it returns TRUE, but
            // still_readable != 1 then shutdown
            // (SHUT_RD|SHUT_RDWR) has been called locally.

            if (_still_readable != 1)
            {
                ret = -1;
                errno = EINTR;
            }
        }

        if (ret == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Send(const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

#if IL2CPP_USE_SEND_NOSIGNAL
        c_flags |= MSG_NOSIGNAL;
#endif

        int32_t ret = 0;

        do
        {
            ret = (int32_t)send(_fd, (void*)data, count, c_flags);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendArray(WSABuf *wsabufs, int32_t count, int32_t *sent, SocketFlags flags)
    {
#if IL2CPP_SUPPORT_SEND_MSG
        int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        struct msghdr hdr = {0};

        hdr.msg_iovlen = count;
        hdr.msg_iov = (struct iovec*)malloc(sizeof(struct iovec) * count);

        for (int32_t i = 0; i < count; ++i)
        {
            hdr.msg_iov[i].iov_base = wsabufs[i].buffer;
            hdr.msg_iov[i].iov_len  = wsabufs[i].length;
        }

#if IL2CPP_USE_SEND_NOSIGNAL
        c_flags |= MSG_NOSIGNAL;
#endif

        int32_t ret = 0;

        do
        {
            ret = (int32_t)sendmsg(_fd, &hdr, c_flags);
        }
        while (ret == -1 && errno == EINTR);

        free(hdr.msg_iov);

        if (ret == -1)
        {
            *sent = 0;

            StoreLastError();

            return kWaitStatusFailure;
        }

        *sent = ret;

        return kWaitStatusSuccess;
#else
        if (sent != NULL)
        {
            *sent = 0; // Sent bytes.
        }

        if (wsabufs == NULL && count > 0)
        {
            _saved_error = kErrorInvalidFunction;
            return kWaitStatusFailure;
        }

        int32_t c_flags = convert_socket_flags(flags);

        for (int32_t i = 0; i < count; ++i)
        {
            ssize_t ret = 0;
            do
            {
                ret = send(_fd, wsabufs[i].buffer, wsabufs[i].length, c_flags);
            }
            while (ret == EINTR);

            if (ret == -1)
            {
                StoreLastError();
                return kWaitStatusFailure;
            }

            if (sent != NULL)
            {
                *sent += ret;
            }
        }

        return kWaitStatusSuccess;
#endif
    }

    WaitStatus SocketImpl::ReceiveArray(WSABuf *wsabufs, int32_t count, int32_t *len, SocketFlags flags)
    {
#if IL2CPP_SUPPORT_RECV_MSG
        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        struct msghdr hdr = {0};

        hdr.msg_iovlen = count;
        hdr.msg_iov = (struct iovec*)malloc(sizeof(struct iovec) * count);

        for (int32_t i = 0; i < count; ++i)
        {
            hdr.msg_iov[i].iov_base = wsabufs[i].buffer;
            hdr.msg_iov[i].iov_len  = wsabufs[i].length;
        }

        int32_t ret = 0;

        do
        {
            ret = (int32_t)recvmsg(_fd, &hdr, c_flags);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == 0)
        {
            // See SocketImpl::ReceiveFromInternal
            if (_still_readable != 1)
            {
                ret = -1;
                errno = EINTR;
            }
        }

        free(hdr.msg_iov);

        if (ret == -1)
        {
            *len = 0;

            StoreLastError();

            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
#else
        if (len != NULL)
        {
            *len = 0;
        }

        if (wsabufs == NULL && count > 0)
        {
            _saved_error = kErrorInvalidFunction;
            return kWaitStatusFailure;
        }

        int32_t c_flags = convert_socket_flags(flags);

        for (int32_t i = 0; i < count; ++i)
        {
            int32_t ret = 0;
            do
            {
                ret = recvfrom(_fd, wsabufs[i].buffer, wsabufs[i].length, c_flags, NULL, NULL);
            }
            while (ret == EINTR);

            if (ret == 0 && count > 0)
            {
                if (_still_readable != 1)
                {
                    _saved_error = SocketErrnoToErrorCode(EINTR);
                    return kWaitStatusFailure;
                }
            }

            if (ret == -1)
            {
                StoreLastError();
                return kWaitStatusFailure;
            }

            if (len != NULL)
            {
                *len += ret;
            }
        }

        return kWaitStatusSuccess;
#endif
    }

    WaitStatus SocketImpl::SendToInternal(struct sockaddr *sa, int32_t sa_size, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

#if IL2CPP_USE_SEND_NOSIGNAL
        c_flags |= MSG_NOSIGNAL;
#endif

        if (!_networkAccess.RequestNetwork(_fd, sa, sa_size))
        {
            StoreLastError(_networkAccess.GetError());
            return kWaitStatusFailure;
        }

        int32_t ret = 0;

        do
        {
            ret = (int32_t)sendto(_fd, (void*)data, count, c_flags, sa, sa_size);
        }
        while (ret == -1 && errno == EINTR);

        if (ret == -1)
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        *len = ret;

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::SendTo(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        return SendToInternal(&sa, sa_size, data, count, flags, len);
    }

    WaitStatus SocketImpl::SendTo(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
#if SUPPORT_UNIXSOCKETS
        *len = 0;

        socklen_t sa_size = 0;

        struct sockaddr* sa = sockaddr_from_path(path, &sa_size);

        WaitStatus status = SendToInternal(sa, sa_size, data, count, flags, len);

        IL2CPP_FREE(sa);

        return status;
#else
        return kWaitStatusFailure;
#endif
    }

    utils::Expected<WaitStatus> SocketImpl::SendTo(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len)
    {
#if IL2CPP_SUPPORT_IPV6
        struct sockaddr_in6 sa = { 0 };
        socklen_t sa_size = 0;

        sockaddr_from_address(address, scope, htons(port), &sa, &sa_size);

        return SendToInternal((sockaddr*)&sa, sa_size, data, count, flags, len);
#else
        return utils::Il2CppError(utils::NotSupported, "IPv6 is not supported on this platform.");
#endif
    }

    WaitStatus SocketImpl::RecvFrom(uint32_t address, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
        *len = 0;

        struct sockaddr sa = {0};
        socklen_t sa_size = 0;

        sockaddr_from_address(htonl(address), htons(port), &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const WaitStatus status = ReceiveFromInternal(data, count, c_flags, len, &sa, (int32_t*)&sa_size);

        if (status != kWaitStatusSuccess)
        {
            ep.family = os::kAddressFamilyError;
            return status;
        }

        if (sa_size == 0)
            return kWaitStatusSuccess;

        if (!socketaddr_to_endpoint_info(&sa, sa_size, ep))
        {
            ep.family = os::kAddressFamilyError;
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::RecvFrom(const char *path, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
#if SUPPORT_UNIXSOCKETS
        *len = 0;

        socklen_t sa_size = 0;

        struct sockaddr* sa = sockaddr_from_path(path, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            IL2CPP_FREE(sa);
            return kWaitStatusFailure;
        }

        const WaitStatus status = ReceiveFromInternal(data, count, c_flags, len, sa, (int32_t*)&sa_size);

        if (status != kWaitStatusSuccess)
        {
            ep.family = os::kAddressFamilyError;
            IL2CPP_FREE(sa);
            return kWaitStatusFailure;
        }

        if (sa_size == 0)
        {
            IL2CPP_FREE(sa);
            return kWaitStatusSuccess;
        }

        if (!socketaddr_to_endpoint_info(sa, sa_size, ep))
        {
            ep.family = os::kAddressFamilyError;
            _saved_error = kWSAeafnosupport;
            IL2CPP_FREE(sa);
            return kWaitStatusFailure;
        }

        IL2CPP_FREE(sa);
        return kWaitStatusSuccess;
#else
        return kWaitStatusFailure;
#endif
    }

    utils::Expected<WaitStatus> SocketImpl::RecvFrom(uint8_t address[ipv6AddressSize], uint32_t scope, uint16_t port, const uint8_t *data, int32_t count, os::SocketFlags flags, int32_t *len, os::EndPointInfo &ep)
    {
#if IL2CPP_SUPPORT_IPV6
        struct sockaddr_in6 sa = { 0 };
        socklen_t sa_size = 0;

        sockaddr_from_address(address, scope, htons(port), &sa, &sa_size);

        const int32_t c_flags = convert_socket_flags(flags);

        if (c_flags == -1)
        {
            _saved_error = kWSAeopnotsupp;
            return kWaitStatusFailure;
        }

        const WaitStatus status = ReceiveFromInternal(data, count, c_flags, len, (sockaddr*)&sa, (int32_t*)&sa_size);

        if (status != kWaitStatusSuccess)
        {
            ep.family = os::kAddressFamilyError;
            return kWaitStatusFailure;
        }

        if (sa_size == 0)
            return kWaitStatusSuccess;

        if (!socketaddr_to_endpoint_info((sockaddr*)&sa, sa_size, ep))
        {
            ep.family = os::kAddressFamilyError;
            _saved_error = kWSAeafnosupport;
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
#else
        return utils::Il2CppError(utils::NotSupported, "IPv6 is not supported on this platform.");
#endif
    }

    WaitStatus SocketImpl::Available(int32_t *amount)
    {
        // ioctl (fd, FIONREAD, XXX) returns the size of
        // the UDP header as well on Darwin.
        //
        // Use getsockopt SO_NREAD instead to get the
        // right values for TCP and UDP.
        //
        // ai_canonname can be null in some cases on darwin, where the runtime assumes it will
        // be the value of the ip buffer.

        *amount = 0;
#if IL2CPP_TARGET_DARWIN
        socklen_t optlen = sizeof(int32_t);
        if (getsockopt(_fd, SOL_SOCKET, SO_NREAD, amount, &optlen) == -1)
#else
        if (ioctl(_fd, FIONREAD, amount) == -1)
#endif
        {
            StoreLastError();
            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Ioctl(int32_t command, const uint8_t *in_data, int32_t in_len, uint8_t *out_data, int32_t out_len, int32_t *written)
    {
        IL2CPP_ASSERT(command != 0xC8000006 /* SIO_GET_EXTENSION_FUNCTION_POINTER */ && "SIO_GET_EXTENSION_FUNCTION_POINTER ioctl command not supported");

        if (command == 0x98000004 /* SIO_KEEPALIVE_VALS */)
        {
            if (in_len < 3 * sizeof(uint32_t))
            {
                StoreLastError();
                return kWaitStatusFailure;
            }

            uint32_t onoff = *((uint32_t*)in_data);
            int32_t ret = setsockopt(_fd, SOL_SOCKET, SO_KEEPALIVE, &onoff, sizeof(uint32_t));
            if (ret < 0)
            {
                StoreLastError();
                return kWaitStatusFailure;
            }
        }
        else
        {
            uint8_t *buffer = NULL;

            if (in_len > 0)
            {
                buffer = (uint8_t*)malloc(in_len);
                memcpy(buffer, in_data, in_len);
            }

            const int32_t ret = ioctl(_fd, command, buffer);
            if (ret == -1)
            {
                StoreLastError();

                free(buffer);

                return kWaitStatusFailure;
            }

            if (buffer == NULL)
            {
                *written = 0;
                return kWaitStatusSuccess;
            }

            // We just copy the buffer to the out_data. Some ioctls
            // don't even out_data any data, but, well ...
            //
            // NB: windows returns WSAEFAULT if out_len is too small

            const int32_t len = (in_len > out_len) ? out_len : in_len;

            if (len > 0 && out_data != NULL)
                memcpy(out_data, buffer, len);

            free(buffer);

            *written = len;
        }

        return kWaitStatusSuccess;
    }

#define     SKIP_OPTION         -2
#define     INVALID_OPTION_NAME -1

    static int32_t level_and_name_to_system(SocketOptionLevel level, SocketOptionName name, int32_t *system_level, int32_t *system_name)
    {
        switch (level)
        {
            case kSocketOptionLevelSocket:
                *system_level = SOL_SOCKET;

                switch (name)
                {
                    // This is SO_LINGER, because the setsockopt
                    // internal call maps DontLinger to SO_LINGER
                    // with l_onoff=0
                    case kSocketOptionNameDontLinger:
                        *system_name = SO_LINGER;
                        break;
        #ifdef SO_DEBUG
                    case kSocketOptionNameDebug:
                        *system_name = SO_DEBUG;
                        break;
        #endif
        #ifdef SO_ACCEPTCONN
                    case kSocketOptionNameAcceptConnection:
                        *system_name = SO_ACCEPTCONN;
                        break;
        #endif
                    case kSocketOptionNameReuseAddress:
                        *system_name = SO_REUSEADDR;
                        break;

                    case kSocketOptionNameKeepAlive:
                        *system_name = SO_KEEPALIVE;
                        break;
        #ifdef SO_DONTROUTE
                    case kSocketOptionNameDontRoute:
                        *system_name = SO_DONTROUTE;
                        break;
        #endif
                    case kSocketOptionNameBroadcast:
                        *system_name = SO_BROADCAST;
                        break;

                    case kSocketOptionNameLinger:
                        *system_name = SO_LINGER;
                        break;
        #ifdef SO_OOBINLINE
                    case kSocketOptionNameOutOfBandInline:
                        *system_name = SO_OOBINLINE;
                        break;
        #endif
                    case kSocketOptionNameSendBuffer:
                        *system_name = SO_SNDBUF;
                        break;

                    case kSocketOptionNameReceiveBuffer:
                        *system_name = SO_RCVBUF;
                        break;

                    case kSocketOptionNameSendLowWater:
                        *system_name = SO_SNDLOWAT;
                        break;

                    case kSocketOptionNameReceiveLowWater:
                        *system_name = SO_RCVLOWAT;
                        break;

                    case kSocketOptionNameSendTimeout:
                        *system_name = SO_SNDTIMEO;
                        break;

                    case kSocketOptionNameReceiveTimeout:
                        *system_name = SO_RCVTIMEO;
                        break;

                    case kSocketOptionNameError:
                        *system_name = SO_ERROR;
                        break;

                    case kSocketOptionNameType:
                        *system_name = SO_TYPE;
                        break;

                    case kSocketOptionNameExclusiveAddressUse:
        #ifdef SO_EXCLUSIVEADDRUSE
                        *system_name = SO_EXCLUSIVEADDRUSE;
                        break;
        #elif SO_REUSEADDR
                        *system_name = SO_REUSEADDR;
                        break;
        #endif
                    case kSocketOptionNameUseLoopback:
        #ifdef SO_USELOOPBACK
                        *system_name = SO_USELOOPBACK;
                        break;
        #endif
                    case kSocketOptionNameMaxConnections:
        #ifdef SO_MAXCONN
                        *system_name = SO_MAXCONN;
                        break;
        #elif defined(SOMAXCONN)
                        *system_name = SOMAXCONN;
                        break;
        #endif
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;

            case kSocketOptionLevelIP:
        #ifdef SOL_IP
                *system_level = SOL_IP;
        #else
                *system_level = IPPROTO_IP;
        #endif

                switch (name)
                {
        #ifdef IP_OPTIONS
                    case kSocketOptionNameIPOptions:
                        *system_name = IP_OPTIONS;
                        break;
        #endif
        #ifdef IP_HDRINCL
                    case kSocketOptionNameHeaderIncluded:
                        *system_name = IP_HDRINCL;
                        break;
        #endif
        #ifdef IP_TOS
                    case kSocketOptionNameTypeOfService:
                        *system_name = IP_TOS;
                        break;
        #endif
        #ifdef IP_TTL
                    case kSocketOptionNameIpTimeToLive:
                        *system_name = IP_TTL;
                        break;
        #endif
                    case kSocketOptionNameMulticastInterface:
                        *system_name = IP_MULTICAST_IF;
                        break;

                    case kSocketOptionNameMulticastTimeToLive:
                        *system_name = IP_MULTICAST_TTL;
                        break;

                    case kSocketOptionNameMulticastLoopback:
                        *system_name = IP_MULTICAST_LOOP;
                        break;

                    case kSocketOptionNameAddMembership:
                        *system_name = IP_ADD_MEMBERSHIP;
                        break;

                    case kSocketOptionNameDropMembership:
                        *system_name = IP_DROP_MEMBERSHIP;
                        break;

        #ifdef HAVE_IP_PKTINFO
                    case kSocketOptionNamePacketInformation:
                        *system_name = IP_PKTINFO;
                        break;
        #endif

                    case kSocketOptionNameDontFragment:
        #ifdef IP_DONTFRAGMENT
                        *system_name = IP_DONTFRAGMENT;
        #elif IP_MTU_DISCOVER
                        *system_name = IP_MTU_DISCOVER;
        #elif IP_DONTFRAG
                        *system_name = IP_DONTFRAG;
        #else
                        return SKIP_OPTION;
        #endif
                        break;

                    case kSocketOptionNameAddSourceMembership:
                    case kSocketOptionNameDropSourceMembership:
                    case kSocketOptionNameBlockSource:
                    case kSocketOptionNameUnblockSource:
                    // Can't figure out how to map these, so fall
                    // through
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;
#if IL2CPP_SUPPORT_IPV6
            case kSocketOptionLevelIPv6:
        #ifdef SOL_IPV6
                *system_level = SOL_IPV6;
        #else
                *system_level = IPPROTO_IPV6;
        #endif

                switch (name)
                {
                    case kSocketOptionNameMulticastInterface:
                        *system_name = IPV6_MULTICAST_IF;
                        break;
                    case kSocketOptionNameMulticastTimeToLive:
                        *system_name = IPV6_MULTICAST_HOPS;
                        break;
                    case kSocketOptionNameMulticastLoopback:
                        *system_name = IPV6_MULTICAST_LOOP;
                        break;
                    case kSocketOptionNameAddMembership:
                        *system_name = IPV6_JOIN_GROUP;
                        break;
                    case kSocketOptionNameDropMembership:
                        *system_name = IPV6_LEAVE_GROUP;
                        break;
                    case kSocketOptionNamePacketInformation:
#ifdef HAVE_IPV6_PKTINFO
                        *system_name = IPV6_PKTINFO;
                        break;
#endif
                    case kSocketOptionNameIPv6Only:
#ifdef IPV6_V6ONLY
                        *system_name = IPV6_V6ONLY;
                        break;
#endif
                    case kSocketOptionNameHeaderIncluded:
                    case kSocketOptionNameIPOptions:
                    case kSocketOptionNameTypeOfService:
                    case kSocketOptionNameDontFragment:
                    case kSocketOptionNameAddSourceMembership:
                    case kSocketOptionNameDropSourceMembership:
                    case kSocketOptionNameBlockSource:
                    case kSocketOptionNameUnblockSource:
                    // Can't figure out how to map these, so fall
                    // through
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;
#endif // IL2CPP_SUPPORT_IPV6
            case kSocketOptionLevelTcp:
        #ifdef SOL_TCP
                *system_level = SOL_TCP;
        #else
                *system_level = IPPROTO_TCP;
        #endif

                switch (name)
                {
                    case kSocketOptionNameNoDelay:
                        *system_name = TCP_NODELAY;
                        break;
                    default:
                        return INVALID_OPTION_NAME;
                }
                break;

            case kSocketOptionLevelUdp:
            default:
                return INVALID_OPTION_NAME;
        }

        return 0;
    }

    WaitStatus SocketImpl::GetSocketOption(SocketOptionLevel level, SocketOptionName name, uint8_t *buffer, int32_t *length)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            *((int32_t*)buffer) = 0;
            *length = sizeof(int32_t);

            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct timeval tv;
        uint8_t *tmp_val = buffer;

        if (system_level == SOL_SOCKET && (system_name == SO_RCVTIMEO || system_name == SO_SNDTIMEO))
        {
            tmp_val = (uint8_t*)&tv;
            *length = sizeof(tv);
        }

        const int32_t ret = getsockopt(_fd, system_level, system_name, tmp_val, (socklen_t*)length);
        if (ret == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        if (system_level == SOL_SOCKET && (system_name == SO_RCVTIMEO || system_name == SO_SNDTIMEO))
        {
            // milliseconds from microseconds
            *((int32_t*)buffer)  = (int32_t)(tv.tv_sec * 1000 + (tv.tv_usec / 1000));
            *length = sizeof(int32_t);

            return kWaitStatusSuccess;
        }

        if (system_name == SO_ERROR)
        {
            if (*((int32_t*)buffer) != 0)
            {
                StoreLastError(*((int32_t*)buffer));
            }
            else
            {
                *((int32_t*)buffer) = _saved_error;
            }
        }

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::GetSocketOptionFull(SocketOptionLevel level, SocketOptionName name, int32_t *first, int32_t *second)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

#if !defined(SO_EXCLUSIVEADDRUSE) && defined(SO_REUSEADDR)
        if (level == kSocketOptionLevelSocket && name == kSocketOptionNameExclusiveAddressUse)
        {
            system_level = SOL_SOCKET;
            system_name = SO_REUSEADDR;
        }
        else
#endif
        {
            const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

            if (o_res == SKIP_OPTION)
            {
                *first = 0;
                *second = 0;

                return kWaitStatusSuccess;
            }

            if (o_res == INVALID_OPTION_NAME)
            {
                _saved_error = kWSAenoprotoopt;

                return kWaitStatusFailure;
            }
        }

        int32_t ret = -1;

        switch (name)
        {
            case kSocketOptionNameLinger:
            {
                struct linger linger;
                socklen_t lingersize = sizeof(linger);

                ret = getsockopt(_fd, system_level, system_name, &linger, &lingersize);

                *first = linger.l_onoff;
                *second = linger.l_linger;
            }
            break;

            case kSocketOptionNameDontLinger:
            {
                struct linger linger;
                socklen_t lingersize = sizeof(linger);

                ret = getsockopt(_fd, system_level, system_name, &linger, &lingersize);

                *first = !linger.l_onoff;
            }
            break;

            case kSocketOptionNameSendTimeout:
            case kSocketOptionNameReceiveTimeout:
            {
                struct timeval time;
                socklen_t time_size = sizeof(time);
                ret = getsockopt(_fd, system_level, system_name, &time, &time_size);

                // Use a 64-bit integer to avoid overflow
                uint64_t timeInMilliseconds = (time.tv_sec * (uint64_t)1000) + (time.tv_usec / 1000);

                // Truncate back to a 32-bit integer to return the value back to the caller.
                *first = (int32_t)timeInMilliseconds;
            }
            break;

            default:
            {
                socklen_t valsize = sizeof(*first);
                ret = getsockopt(_fd, system_level, system_name, first, &valsize);
            }
            break;
        }

        if (ret == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

#if !defined(SO_EXCLUSIVEADDRUSE) && defined(SO_REUSEADDR)
        if (level == kSocketOptionLevelSocket && name == kSocketOptionNameExclusiveAddressUse)
            *first = *first ? 0 : 1;
#endif

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t count, int32_t timeout, int32_t *result, int32_t *error)
    {
        const int32_t n_fd = count;
        pollfd *p_fd = (pollfd*)calloc(n_fd, sizeof(pollfd));

        for (int32_t i = 0; i < n_fd; ++i)
        {
            if (requests[i].fd == -1)
            {
                p_fd[i].fd = -1;
                p_fd[i].events = kPollFlagsNone;
                p_fd[i].revents = kPollFlagsNone;
            }
            else
            {
                p_fd[i].fd = requests[i].fd;
                p_fd[i].events = posix::PollFlagsToPollEvents(requests[i].events);
                p_fd[i].revents = kPollFlagsNone;
            }
        }

        int32_t ret = os::posix::Poll(p_fd, n_fd, timeout);
        *result = ret;

        if (ret == -1)
        {
            free(p_fd);

            *error = SocketErrnoToErrorCode(errno);

            return kWaitStatusFailure;
        }

        if (ret == 0)
        {
            free(p_fd);

            return kWaitStatusSuccess;
        }

        for (int32_t i = 0; i < n_fd; ++i)
        {
            requests[i].revents = posix::PollEventsToPollFlags(p_fd[i].revents);
        }

        free(p_fd);

        return kWaitStatusSuccess;
    }

    WaitStatus SocketImpl::Poll(std::vector<PollRequest> &requests, int32_t timeout, int32_t *result, int32_t *error)
    {
        return Poll(requests, (int32_t)requests.size(), timeout, result, error);
    }

    WaitStatus SocketImpl::Poll(PollRequest& request, int32_t timeout, int32_t *result, int32_t *error)
    {
        std::vector<PollRequest> requests;
        requests.push_back(request);
        return Poll(requests, 1, timeout, result, error);
    }

    WaitStatus SocketImpl::SetSocketOption(SocketOptionLevel level, SocketOptionName name, int32_t value)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct linger linger;

        WaitStatus ret = kWaitStatusFailure;

        switch (name)
        {
            case kSocketOptionNameDontLinger:
                linger.l_onoff = !value;
                linger.l_linger = 0;
                ret = SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
                break;

            case kSocketOptionNameDontFragment:
#ifdef IP_PMTUDISC_DO
                // Fiddle with the value slightly if we're turning DF on
                if (value == 1)
                    value = IP_PMTUDISC_DO;
#endif
                ret = SetSocketOptionInternal(system_level, system_name, (char*)&value, sizeof(value));
                break;

            default:
                ret = SetSocketOptionInternal(system_level, system_name, (char*)&value, sizeof(value));
                break;
        }

        return ret;
    }

    WaitStatus SocketImpl::SetSocketOptionLinger(SocketOptionLevel level, SocketOptionName name, bool enabled, int32_t seconds)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct linger linger;

        linger.l_onoff = enabled;
        linger.l_linger = seconds;

        return SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
    }

    WaitStatus SocketImpl::SetSocketOptionArray(SocketOptionLevel level, SocketOptionName name, const uint8_t *buffer, int32_t length)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct linger linger;

        WaitStatus ret = kWaitStatusFailure;

        switch (name)
        {
            case kSocketOptionNameDontLinger:
                if (length == 1)
                {
                    linger.l_linger = 0;
                    linger.l_onoff = (*((char*)buffer)) ? 0 : 1;

                    ret = SetSocketOptionInternal(system_level, system_name, &linger, sizeof(linger));
                }
                else
                {
                    _saved_error = kWSAeinval;

                    return kWaitStatusFailure;
                }
                break;

            default:
                ret = SetSocketOptionInternal(system_level, system_name, buffer, length);
                break;
        }

        return ret;
    }

    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, uint32_t group_address, uint32_t local_address)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);

        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct ip_mreqn mreq = {{0}};

        mreq.imr_multiaddr.s_addr = group_address;
        mreq.imr_address.s_addr = local_address;

        return SetSocketOptionInternal(system_level, system_name, &mreq, sizeof(mreq));
    }

#if IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_LINUX
    #include <sys/types.h>
    #include <ifaddrs.h>
    #include <sys/socket.h>
    #include <net/if.h>
    static int get_local_interface_id(int family)
    {
        struct ifaddrs *ifap = NULL, *ptr;
        int idx = 0;
        if (getifaddrs(&ifap))
            return 0;

        for (ptr = ifap; ptr; ptr = ptr->ifa_next)
        {
            if (!ptr->ifa_addr || !ptr->ifa_name)
                continue;
            if (ptr->ifa_addr->sa_family != family)
                continue;
            if ((ptr->ifa_flags & IFF_LOOPBACK) != 0)
                continue;
            if ((ptr->ifa_flags & IFF_MULTICAST) == 0)
                continue;

            idx = if_nametoindex(ptr->ifa_name);
            break;
        }

        freeifaddrs(ifap);
        return idx;
    }

#endif // IL2CPP_TARGET_DARWIN

#if IL2CPP_SUPPORT_IPV6
    WaitStatus SocketImpl::SetSocketOptionMembership(SocketOptionLevel level, SocketOptionName name, IPv6Address ipv6, uint64_t interfaceOffset)
    {
        int32_t system_level = 0;
        int32_t system_name = 0;

        const int32_t o_res = level_and_name_to_system(level, name, &system_level, &system_name);
        if (o_res == SKIP_OPTION)
        {
            return kWaitStatusSuccess;
        }

        if (o_res == INVALID_OPTION_NAME)
        {
            _saved_error = kWSAenoprotoopt;

            return kWaitStatusFailure;
        }

        struct ipv6_mreq mreq6 = {{0}};
        struct in6_addr in6addr;
        for (int i = 0; i < 16; ++i)
            in6addr.s6_addr[i] = ipv6.addr[i];
        mreq6.ipv6mr_multiaddr = in6addr;

#if IL2CPP_TARGET_DARWIN || IL2CPP_TARGET_LINUX
        if (interfaceOffset == 0)
            interfaceOffset = get_local_interface_id(AF_INET6);
#endif
        mreq6.ipv6mr_interface = interfaceOffset;

        return SetSocketOptionInternal(system_level, system_name, &mreq6, sizeof(mreq6));
    }

#endif

    WaitStatus SocketImpl::SetSocketOptionInternal(int32_t level, int32_t name, const void *value, int32_t len)
    {
        const void *real_val = value;
        struct timeval tv;

        if (level == SOL_SOCKET && (name == SO_RCVTIMEO || name == SO_SNDTIMEO))
        {
            const int32_t ms = *((int32_t*)value);

            tv.tv_sec = ms / 1000;
            tv.tv_usec = (ms % 1000) * 1000;
            real_val = &tv;

            len = sizeof(tv);
        }

        const int32_t ret = setsockopt(_fd, level, name, real_val, (socklen_t)len);

        if (ret == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

#if defined(SO_REUSEPORT)
        // BSD's and MacOS X multicast sockets also need SO_REUSEPORT when SO_REUSEADDR is requested.
        if (level == SOL_SOCKET && name == SO_REUSEADDR)
        {
            int32_t type;
            socklen_t type_len = sizeof(type);

            if (!getsockopt(_fd, level, SO_TYPE, &type, &type_len))
            {
                if (type == SOCK_DGRAM)
                    setsockopt(_fd, level, SO_REUSEPORT, real_val, len);
            }
        }
#endif

        return kWaitStatusSuccess;
    }

#if IL2CPP_SUPPORT_IPV6_SUPPORT_QUERY
    bool SocketImpl::IsIPv6Supported()
    {
        ifaddrs* interfaces;
        if (getifaddrs(&interfaces))
            return false;

        bool ipv6IsSupported = false;
        for (ifaddrs* iface = interfaces; iface != NULL; iface = iface->ifa_next)
        {
            if (iface->ifa_addr && iface->ifa_addr->sa_family == AF_INET6)
            {
                ipv6IsSupported = true;
                break;
            }
        }

        freeifaddrs(interfaces);
        return ipv6IsSupported;
    }

#endif

    WaitStatus SocketImpl::SendFile(const char *filename, TransmitFileBuffers *buffers, TransmitFileOptions options)
    {
#if IL2CPP_SUPPORT_SEND_FILE
        int32_t file = open(filename, O_RDONLY);

        if (file == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        int32_t ret;

        // Write the header
        if (buffers != NULL && buffers->head != NULL && buffers->head_length > 0)
        {
            do
            {
                ret = (int32_t)send(_fd, (void*)buffers->head, buffers->head_length, 0);
            }
            while (ret == -1 && errno == EINTR);

            if (ret == -1)
            {
                StoreLastError();

                SOCK_CLOSE(file);

                return kWaitStatusFailure;
            }
        }

        struct stat statbuf;

        ret = fstat(file, &statbuf);
        if (ret == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        do
        {
#if IL2CPP_TARGET_DARWIN
            ret = sendfile(file, _fd, 0, &statbuf.st_size, NULL, 0);
#else
            ret = sendfile(_fd, file, NULL, statbuf.st_size);
#endif
        }
        while (ret != -1 && (errno == EINTR || errno == EAGAIN));

        if (ret == -1)
        {
            StoreLastError();

            SOCK_CLOSE(file);

            return kWaitStatusFailure;
        }

        // Write the tail
        if (buffers != NULL && buffers->tail != NULL && buffers->tail_length > 0)
        {
            do
            {
                ret = (int32_t)send(_fd, (void*)buffers->tail, buffers->tail_length, 0);
            }
            while (ret == -1 && errno == EINTR);

            if (ret == -1)
            {
                StoreLastError();

                SOCK_CLOSE(file);

                return kWaitStatusFailure;
            }
        }

        if (SOCK_CLOSE(file) == -1)
        {
            StoreLastError();

            return kWaitStatusFailure;
        }

        return kWaitStatusSuccess;
#else
        return kWaitStatusFailure;
#endif
    }
}
}
#endif
