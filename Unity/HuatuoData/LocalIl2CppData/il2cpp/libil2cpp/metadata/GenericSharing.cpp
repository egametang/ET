#include "il2cpp-config.h"
#include "metadata/GenericSharing.h"
#include "vm/Type.h"
#include "il2cpp-runtime-metadata.h"

using il2cpp::vm::Type;

namespace il2cpp
{
namespace metadata
{
    bool GenericSharing::IsShareable(Il2CppGenericClass* gclass)
    {
        const Il2CppGenericInst* classInst = gclass->context.class_inst;
        if (classInst)
        {
            for (uint32_t i = 0; i < classInst->type_argc; ++i)
            {
                if (!Type::IsReference(classInst->type_argv[i]))
                    return false;
            }
        }

        return true;
    }

    bool GenericSharing::IsShareable(Il2CppGenericMethod* gmethod)
    {
        const Il2CppGenericInst* methodInst = gmethod->context.method_inst;
        if (methodInst)
        {
            for (uint32_t i = 0; i < methodInst->type_argc; ++i)
            {
                if (!Type::IsReference(methodInst->type_argv[i]))
                    return false;
            }
        }

        return true;
    }
} /* namespace vm */
} /* namespace il2cpp */
