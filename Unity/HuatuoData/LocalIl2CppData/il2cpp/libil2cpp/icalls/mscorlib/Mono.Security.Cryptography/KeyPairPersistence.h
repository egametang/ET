#pragma once

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
        static bool _CanSecure(Il2CppChar* root);
        static bool _IsMachineProtected(Il2CppChar* path);
        static bool _IsUserProtected(Il2CppChar* path);
        static bool _ProtectMachine(Il2CppChar* path);
        static bool _ProtectUser(Il2CppChar* path);
    };
} /* namespace Cryptography */
} /* namespace Security */
} /* namespace Mono */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
