#include "il2cpp-config.h"
#include "il2cpp-class-internals.h"
#include "Il2CppTypeHash.h"
#include "utils/StringUtils.h"
#include "utils/HashUtils.h"

using il2cpp::utils::HashUtils;
using il2cpp::utils::StringUtils;

namespace il2cpp
{
namespace metadata
{
    size_t Il2CppTypeHash::operator()(const Il2CppType* t1) const
    {
        return Hash(t1);
    }

    size_t Il2CppTypeHash::Hash(const Il2CppType* t1)
    {
        size_t hash = t1->type;

        hash = HashUtils::Combine(hash, t1->byref);

        switch (t1->type)
        {
            case IL2CPP_TYPE_VALUETYPE:
            case IL2CPP_TYPE_CLASS:
            {
                return HashUtils::Combine(hash, reinterpret_cast<size_t>(t1->data.typeHandle));
            }
            case IL2CPP_TYPE_SZARRAY:
            case IL2CPP_TYPE_PTR:
            {
                return HashUtils::Combine(hash, Hash(t1->data.type));
            }
            case IL2CPP_TYPE_GENERICINST:
            {
                const Il2CppGenericInst *inst = t1->data.generic_class->context.class_inst;
                hash = HashUtils::Combine(hash, Hash(t1->data.generic_class->type));
                for (uint32_t i = 0; i < inst->type_argc; ++i)
                {
                    hash = HashUtils::Combine(hash, Hash(inst->type_argv[i]));
                }
                return hash;
            }
            default:
                return hash;
        }
        return hash;
    }
} /* namespace vm */
} /* namespace il2cpp */
