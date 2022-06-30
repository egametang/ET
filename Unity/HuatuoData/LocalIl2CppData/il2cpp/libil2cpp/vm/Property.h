#pragma once

#include <stdint.h>
#include "il2cpp-config.h"

struct MethodInfo;
struct PropertyInfo;
struct Il2CppClass;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Property
    {
    public:
        // exported
        static uint32_t GetFlags(const PropertyInfo* prop);
        static const MethodInfo* GetGetMethod(const PropertyInfo* prop);
        static const MethodInfo* GetSetMethod(const PropertyInfo* prop);
        static const char* GetName(const PropertyInfo* prop);
        static Il2CppClass* GetParent(const PropertyInfo* prop);
        static uint32_t GetToken(const PropertyInfo* prop);
    };
} /* namespace vm */
} /* namespace il2cpp */
