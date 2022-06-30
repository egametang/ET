#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericMethodCompare.h"
#include "Il2CppGenericContextCompare.h"

namespace il2cpp
{
namespace metadata
{
    bool Il2CppGenericMethodCompare::operator()(const Il2CppGenericMethod* m1, const Il2CppGenericMethod* m2) const
    {
        return Equals(m1, m2);
    }

    bool Il2CppGenericMethodCompare::Equals(const Il2CppGenericMethod* m1, const Il2CppGenericMethod* m2)
    {
        if (m1->methodDefinition != m2->methodDefinition)
            return false;

        return Il2CppGenericContextCompare::Compare(&m1->context, &m2->context);
    }
} /* namespace vm */
} /* namespace il2cpp */
