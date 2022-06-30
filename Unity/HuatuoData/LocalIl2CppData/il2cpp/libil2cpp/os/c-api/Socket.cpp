#include "os/c-api/il2cpp-config-platforms.h"

#if !RUNTIME_TINY

#include "os/c-api/Socket-c-api.h"
#include "os/c-api/Allocator.h"
#include "os/Socket.h"
#include "utils/Memory.h"
#include <string>
#include <vector>

UnityPalWaitStatus UnityPalGetHostByName(const char* host, char** name, int32_t* family, char*** aliases, void*** address_list, int32_t* address_size)
{
    std::string palName;
    int32_t palFamily;
    std::vector<std::string> palAliases;
    std::vector<void*> palAddressList;
    int32_t palAddressSize;
    il2cpp::os::WaitStatus result = il2cpp::os::Socket::GetHostByName(host, palName, palFamily, palAliases, palAddressList, palAddressSize);

    if (name != NULL)
        *name = Allocator::CopyToAllocatedStringBuffer(palName.c_str());

    if (family != NULL)
        *family = palFamily;

    Allocator::CopyStringVectorToNullTerminatedArray(palAliases, (void***)aliases);
    Allocator::CopyDataVectorToNullTerminatedArray(palAddressList, address_list, palAddressSize);

    if (address_size != NULL)
        *address_size = palAddressSize;

    for (size_t i = 0; i < palAddressList.size(); ++i)
        il2cpp::utils::Memory::Free(palAddressList[i]);

    return result;
}

#endif
