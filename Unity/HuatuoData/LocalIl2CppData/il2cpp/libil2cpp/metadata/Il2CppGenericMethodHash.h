#pragma once

struct Il2CppGenericMethod;

namespace il2cpp
{
namespace metadata
{
    struct Il2CppGenericMethodHash
    {
    public:
        size_t operator()(const Il2CppGenericMethod* method) const;
        static size_t Hash(const Il2CppGenericMethod* method);
    };
} /* namespace metadata */
} /* namespace il2cpp */
