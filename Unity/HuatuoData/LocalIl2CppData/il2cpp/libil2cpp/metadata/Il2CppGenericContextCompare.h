#pragma once

struct Il2CppGenericContext;

namespace il2cpp
{
namespace metadata
{
    class Il2CppGenericContextCompare
    {
    public:
        bool operator()(const Il2CppGenericContext* t1, const Il2CppGenericContext* t2) const;
        static bool Compare(const Il2CppGenericContext* t1, const Il2CppGenericContext* t2);
    };
} /* namespace vm */
} /* namespace il2cpp */
