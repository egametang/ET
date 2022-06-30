#pragma once

#include <stdint.h>
#include "il2cpp-config.h"
struct Il2CppDomain;
struct Il2CppAppContext;

namespace il2cpp
{
namespace vm
{
    class LIBIL2CPP_CODEGEN_API Domain
    {
    public:
        static Il2CppDomain* GetCurrent();
        static Il2CppDomain* GetRoot();
        static void ContextInit(Il2CppDomain *domain);
        static void ContextSet(Il2CppAppContext* context);
        static Il2CppAppContext* ContextGet();

    private:
        static Il2CppDomain* S_domain;
    };
} /* namespace vm */
} /* namespace il2cpp */
