#include "il2cpp-config.h"

#include "icalls/mscorlib/System.Reflection/ParameterInfo.h"
#include "vm/Exception.h"
#include "il2cpp-object-internals.h"
#include "il2cpp-class-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
namespace Reflection
{
    int32_t ParameterInfo::GetMetadataToken(Il2CppReflectionParameter* self)
    {
        //ReturnParameter Parameter infos are constructed at runtime.
        if (self->PositionImpl == -1)
            return 0x8000000; // This is what mono returns as a fixed value.

        Il2CppReflectionMethod* method = (Il2CppReflectionMethod*)self->MemberImpl;
        const ::ParameterInfo* info = &method->method->parameters[self->PositionImpl];
        return (int32_t)info->token;
    }

    Il2CppArray* ParameterInfo::GetTypeModifiers(void* /* System.Reflection.ParameterInfo */ self, bool optional)
    {
        NOT_SUPPORTED_IL2CPP(ParameterInfo::GetTypeModifiers, "This icall is not supported by il2cpp.");

        return 0;
    }
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
