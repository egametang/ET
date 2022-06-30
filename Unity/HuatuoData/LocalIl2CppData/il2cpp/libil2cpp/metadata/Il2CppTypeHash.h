#pragma once

struct Il2CppType;

namespace il2cpp
{
namespace metadata
{
    class Il2CppTypeHash
    {
    public:
        size_t operator()(const Il2CppType* t1) const;
        static size_t Hash(const Il2CppType* t1);
    };
} /* namespace vm */
} /* namespace il2cpp */
