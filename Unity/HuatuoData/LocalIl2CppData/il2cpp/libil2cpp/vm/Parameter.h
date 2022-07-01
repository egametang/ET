#pragma once

#include "il2cpp-config.h"
struct ParameterInfo;
struct Il2CppObject;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Parameter
    {
    public:
        // internal
        static Il2CppObject* GetDefaultParameterValueObject(const MethodInfo* method, const ParameterInfo* parameter, bool* isExplicitySetNullDefaultValue);
    };
} /* namespace vm */
} /* namespace il2cpp */
