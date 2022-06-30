#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericInstHash.h"
#include "Il2CppTypeHash.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppGenericInstHash::operator()(const Il2CppGenericInst* item) const
    {
        return Hash(item);
    }

    size_t Il2CppGenericInstHash::Hash(const Il2CppGenericInst* item)
    {
        size_t hash = item->type_argc;
        for (size_t i = 0; i < item->type_argc; ++i)
            hash = HashUtils::Combine(hash, Il2CppTypeHash::Hash(item->type_argv[i]));

        return hash;
    }
} /* namespace vm */
} /* namespace il2cpp */
