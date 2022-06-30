#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppSignature.h"
#include "Il2CppTypeCompare.h"
#include "Il2CppTypeHash.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppSignatureHash::operator()(const Il2CppSignature& signature) const
    {
        return Hash(signature);
    }

    size_t Il2CppSignatureHash::Hash(const Il2CppSignature& signature)
    {
        size_t retVal = signature.Count;

        for (size_t i = 0; i < signature.Count; ++i)
            retVal = HashUtils::Combine(retVal, Il2CppTypeHash::Hash(signature.Types[i]));

        return retVal;
    }

    bool Il2CppSignatureCompare::operator()(const Il2CppSignature& s1, const Il2CppSignature& s2) const
    {
        return Equals(s1, s2);
    }

    bool Il2CppSignatureCompare::Equals(const Il2CppSignature& s1, const Il2CppSignature& s2)
    {
        if (s1.Count != s2.Count)
            return false;


        for (size_t i = 0; i < s1.Count; ++i)
        {
            if (!Il2CppTypeEqualityComparer::AreEqual(s1.Types[i], s2.Types[i]))
                return false;
        }

        return true;
    }
} /* namespace metadata */
} /* namespace il2cpp */
