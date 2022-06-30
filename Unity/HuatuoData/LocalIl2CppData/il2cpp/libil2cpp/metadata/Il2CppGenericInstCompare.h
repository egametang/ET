#pragma once

#include "utils/KeyWrapper.h"

struct Il2CppGenericInst;

namespace il2cpp
{
namespace metadata
{
    class Il2CppGenericInstCompare
    {
    public:
        bool operator()(const KeyWrapper<const Il2CppGenericInst*>& t1, const KeyWrapper<const Il2CppGenericInst*>& t2) const;
        static bool Compare(const KeyWrapper<const Il2CppGenericInst*>& t1, const KeyWrapper<const Il2CppGenericInst*>& t2);
        static bool AreEqual(const Il2CppGenericInst* left, const Il2CppGenericInst* right);
    };
} /* namespace vm */
} /* namespace il2cpp */
