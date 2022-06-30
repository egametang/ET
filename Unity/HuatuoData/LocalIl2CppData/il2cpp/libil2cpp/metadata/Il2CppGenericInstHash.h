#pragma once

struct Il2CppGenericInst;

namespace il2cpp
{
namespace metadata
{
    class Il2CppGenericInstHash
    {
    public:
        size_t operator()(const Il2CppGenericInst* ea) const;
        static size_t Hash(const Il2CppGenericInst* t1);
    };
} /* namespace vm */
} /* namespace il2cpp */
