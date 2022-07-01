#pragma once
#include "il2cpp-object-internals.h"

namespace il2cpp
{
namespace icalls
{
namespace mscorlib
{
namespace System
{
    class LIBIL2CPP_CODEGEN_API RuntimeFieldHandle
    {
    public:
        static void SetValueDirect(Il2CppReflectionField* field, Il2CppObject* fieldType, Il2CppTypedRef* typedRef, Il2CppObject* value, Il2CppObject* contextType);
        static void SetValueInternal(Il2CppReflectionField* fi, Il2CppObject* obj, Il2CppObject* value);
    };
} // namespace System
} // namespace mscorlib
} // namespace icalls
} // namespace il2cpp
