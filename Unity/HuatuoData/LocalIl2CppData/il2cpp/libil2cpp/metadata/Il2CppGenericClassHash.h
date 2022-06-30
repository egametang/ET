#pragma once

struct Il2CppGenericClass;

namespace il2cpp
{
namespace metadata
{
    class Il2CppGenericClassHash
    {
    public:
        size_t operator()(const Il2CppGenericClass* ea) const;
        static size_t Hash(const Il2CppGenericClass* t1);
    };
} /* namespace vm */
} /* namespace il2cpp */
