#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericInstCompare.h"
#include "Il2CppTypeCompare.h"

namespace il2cpp
{
namespace metadata
{
    bool Il2CppGenericInstCompare::operator()(const KeyWrapper<const Il2CppGenericInst*>& t1, const KeyWrapper<const Il2CppGenericInst*>& t2) const
    {
        return Compare(t1, t2);
    }

    bool Il2CppGenericInstCompare::Compare(const KeyWrapper<const Il2CppGenericInst*>& t1, const KeyWrapper<const Il2CppGenericInst*>& t2)
    {
        if (t1.type != t2.type)
            return false;
        else if (!t1.isNormal())
            return true;

        if (t1.key->type_argc != t2.key->type_argc)
            return false;

        for (size_t i = 0; i < t1.key->type_argc; ++i)
        {
            if (!Il2CppTypeEqualityComparer::AreEqual(t1.key->type_argv[i], t2.key->type_argv[i]))
                return false;
        }

        return true;
    }
} /* namespace vm */
} /* namespace il2cpp */
