#include "il2cpp-config.h"
#include "il2cpp-api.h"
#include "icalls/mscorlib/System.Runtime.Remoting.Activation/ActivationServices.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"
#include "il2cpp-runtime-metadata.h"
#include "vm/Image.h"
#include "vm/Class.h"
#include "vm/Exception.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Runtime
{
namespace Remoting
{
namespace Activation
{
    void ActivationServices::EnableProxyActivation(Il2CppReflectionType*, bool)
    {
        NOT_SUPPORTED_REMOTING(ActivationServices::EnableProxyActivation);
    }

    Il2CppObject* ActivationServices::AllocateUninitializedClassInstance(Il2CppReflectionType * type)
    {
        Il2CppClass* typeInfo = vm::Class::FromIl2CppType(type->type);

        if (typeInfo == NULL)
            return NULL;

        il2cpp::vm::Class::Init(typeInfo);
        return il2cpp_object_new(typeInfo);
    }
} /* namespace Activation */
} /* namespace Remoting */
} /* namespace Runtime */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
