#pragma once
#include "il2cpp-config.h"

struct Il2CppString;

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace Mono
{
namespace Security
{
namespace Cryptography
{
    class LIBIL2CPP_CODEGEN_API KeyPairPersistence
    {
    public:
        static bool _CanSecure(Il2CppString* root);
        static bool _IsUserProtected(Il2CppString* path);
        static bool _ProtectMachine(Il2CppString* path);
        static bool _ProtectUser(Il2CppString* path);
        static bool _IsMachineProtected(Il2CppString* path);
    };
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace Mono */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
