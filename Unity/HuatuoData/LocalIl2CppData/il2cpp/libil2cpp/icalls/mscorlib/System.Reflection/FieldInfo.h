#pragma once

#include "il2cpp-object-internals.h"
#include "il2cpp-config.h"

struct Il2CppReflectionField;
struct Il2CppReflectionMarshal;

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
    class LIBIL2CPP_CODEGEN_API FieldInfo
    {
    public:
        static Il2CppReflectionField* internal_from_handle_type(intptr_t field_handle, intptr_t type_handle);
        static Il2CppObject* get_marshal_info(Il2CppObject* thisPtr);
    };
} /* namespace Reflection */
} /* namespace System */
} /* namespace mscorlib */
} /* namespace icalls */
} /* namespace il2cpp */
