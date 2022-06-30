#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericMethodHash.h"
#include "Il2CppGenericContextHash.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppGenericMethodHash::operator()(const Il2CppGenericMethod* method) const
    {
        return Hash(method);
    }

    size_t Il2CppGenericMethodHash::Hash(const Il2CppGenericMethod* method)
    {
        size_t tokenHash = method->methodDefinition->token;
        size_t contextHash = Il2CppGenericContextHash::Hash(&method->context);

        return HashUtils::Combine(tokenHash, contextHash);
    }
} /* namespace metadata */
} /* namespace il2cpp */
