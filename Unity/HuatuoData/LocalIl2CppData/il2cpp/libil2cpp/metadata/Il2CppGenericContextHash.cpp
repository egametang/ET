#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppGenericContextHash.h"
#include "Il2CppGenericInstHash.h"
#include "Il2CppTypeHash.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppGenericContextHash::operator()(const Il2CppGenericContext* context) const
    {
        return Hash(context);
    }

    size_t Il2CppGenericContextHash::Hash(const Il2CppGenericContext* context)
    {
        //size_t classInstHash = HashUtils::AlignedPointerHash(context->class_inst);
        //size_t methodInstHash = HashUtils::AlignedPointerHash(context->method_inst);
        size_t classInstHash = 0;
        size_t methodInstHash = 0;

        if (context->class_inst)
            classInstHash = Il2CppGenericInstHash::Hash(context->class_inst);
        if (context->method_inst)
            methodInstHash = Il2CppGenericInstHash::Hash(context->method_inst);
        return HashUtils::Combine(classInstHash, methodInstHash);
    }
} /* namespace vm */
} /* namespace il2cpp */
