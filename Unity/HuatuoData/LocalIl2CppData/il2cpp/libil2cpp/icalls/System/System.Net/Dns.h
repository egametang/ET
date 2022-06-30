#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct Il2CppArray;
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
    class LIBIL2CPP_CODEGEN_API Dns
    {
    public:
        static bool GetHostByAddr_icall(Il2CppString* addr, Il2CppString** h_name, Il2CppArray** h_aliases, Il2CppArray** h_addr_list, int32_t hint);
        static bool GetHostByName_icall(Il2CppString* host, Il2CppString** h_name, Il2CppArray** h_aliases, Il2CppArray** h_addr_list, int32_t hint);
        static bool GetHostName_icall(Il2CppString** h_name);
    };
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
