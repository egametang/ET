#pragma once

struct Il2CppType;

namespace il2cpp
{
namespace metadata
{
    struct Il2CppSignature
    {
        size_t Count;
        const Il2CppType** Types;
    };

    struct Il2CppSignatureHash
    {
    public:
        size_t operator()(const Il2CppSignature& signature) const;
        static size_t Hash(const Il2CppSignature& signature);
    };

    struct Il2CppSignatureCompare
    {
        bool operator()(const Il2CppSignature& s1, const Il2CppSignature& s2) const;
        static bool Equals(const Il2CppSignature& s1, const Il2CppSignature& s2);
    };
} /* namespace metadata */
} /* namespace il2cpp */
