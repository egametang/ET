#pragma once

struct Il2CppGenericContext;

namespace il2cpp
{
namespace metadata
{
    class Il2CppGenericContextHash
    {
    public:
        size_t operator()(const Il2CppGenericContext* ea) const;
        static size_t Hash(const Il2CppGenericContext* t1);
    };
} /* namespace vm */
} /* namespace il2cpp */
