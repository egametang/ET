#pragma once

#include "utils/KeyWrapper.h"

struct Il2CppType;

namespace il2cpp
{
namespace metadata
{
    class Il2CppTypeEqualityComparer
    {
    public:
        bool operator()(const Il2CppType* t1, const Il2CppType* t2) const { return AreEqual(t1, t2); }
        static bool AreEqual(const Il2CppType* t1, const Il2CppType* t2);
    };

    class Il2CppTypeLess
    {
    public:
        bool operator()(const Il2CppType* t1, const Il2CppType* t2) const;
    };
} /* namespace vm */
} /* namespace il2cpp */
