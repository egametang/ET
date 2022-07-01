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
        static bool GetHostByAddr(Il2CppString*, Il2CppString**, Il2CppArray**, Il2CppArray**);
        static bool GetHostByName(Il2CppString*, Il2CppString**, Il2CppArray**, Il2CppArray**);
        static bool GetHostByAddr40(Il2CppString*, Il2CppString**, Il2CppArray**, Il2CppArray**, int32_t hint);
        static bool GetHostByName40(Il2CppString*, Il2CppString**, Il2CppArray**, Il2CppArray**, int32_t hint);
        static bool GetHostName(Il2CppString**);
    };
} /* namespace Net */
} /* namespace System */
} /* namespace System */
} /* namespace icalls */
} /* namespace il2cpp */
