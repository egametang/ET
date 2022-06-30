#include "il2cpp-config.h"
#include "MonoTlsProviderFactory.h"
#include "vm/String.h"

namespace il2cpp
{
namespace icalls
{
namespace System
{
namespace Mono
{
namespace Net
{
namespace Security
{
    bool MonoTlsProviderFactory::IsBtlsSupported()
    {
        return false;
    }

    Il2CppString* MonoTlsProviderFactory::GetDefaultProviderForPlatform()
    {
#if IL2CPP_TARGET_IOS
        return vm::String::New("apple");
#else
        return vm::String::New("mbedtls");
#endif
    }
} // namespace Security
} // namespace Net
} // namespace Mono
} // namespace System
} // namespace icalls
} // namespace il2cpp
