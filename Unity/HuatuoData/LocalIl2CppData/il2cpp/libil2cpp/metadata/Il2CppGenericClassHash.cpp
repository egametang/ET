#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericClassHash.h"
#include "Il2CppGenericContextHash.h"
#include "Il2CppTypeHash.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppGenericClassHash::operator()(const Il2CppGenericClass* item) const
    {
        return Hash(item);
    }

    size_t Il2CppGenericClassHash::Hash(const Il2CppGenericClass* item)
    {
        size_t containerHash = Il2CppTypeHash::Hash(item->type);
        size_t contextHash = Il2CppGenericContextHash::Hash(&item->context);

        return HashUtils::Combine(containerHash, contextHash);
    }
} /* namespace vm */
} /* namespace il2cpp */
