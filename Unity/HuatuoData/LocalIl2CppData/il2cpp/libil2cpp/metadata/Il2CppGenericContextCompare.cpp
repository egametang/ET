#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericContextCompare.h"
#include "Il2CppGenericInstCompare.h"

namespace il2cpp
{
namespace metadata
{
    bool Il2CppGenericContextCompare::operator()(const Il2CppGenericContext* gc1, const Il2CppGenericContext* gc2) const
    {
        return Compare(gc1, gc2);
    }

    bool Il2CppGenericContextCompare::Compare(const Il2CppGenericContext* gc1, const Il2CppGenericContext* gc2)
    {
        IL2CPP_NOT_IMPLEMENTED_NO_ASSERT(Il2CppGenericContextCompare::Compare, "We should ensure GenericInst uniqueness and do direct comparison");
        // return gc1->class_inst == gc2->class_inst && gc1->method_inst == gc2->method_inst;

        if ((!gc1->class_inst && gc2->class_inst) || (gc1->class_inst && !gc2->class_inst))
            return false;
        if ((!gc1->method_inst && gc2->method_inst) || (gc1->method_inst && !gc2->method_inst))
            return false;

        return (!gc1->class_inst || Il2CppGenericInstCompare::Compare(gc1->class_inst, gc2->class_inst)) &&
            (!gc1->method_inst || Il2CppGenericInstCompare::Compare(gc1->method_inst, gc2->method_inst));
    }
} /* namespace vm */
} /* namespace il2cpp */
